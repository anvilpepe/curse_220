using System.Linq;
using Content.Server.Administration.Logs;
using Content.Server.DeviceLinking.Systems;
using Content.Server.Explosion.EntitySystems;
using Content.Shared.Database;
using Content.Shared.Interaction.Events;
using Content.Shared.Timing;

namespace Content.Server.Cursed.SignallerExtended;
// mostly 'taken' from SignallerSystem
public sealed class SignallerExtendedSystem : EntitySystem
{
    /// <summary>
    ///  anti-dumb const. update if add more duplicates
    ///  in sourceports.yml
    /// </summary>
    private const int MAX_COUNT = 5;


    [Dependency] private readonly DeviceLinkSystem _link = default!;
    [Dependency] private readonly UseDelaySystem _useDelay = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SignallerExtendedComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<SignallerExtendedComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<SignallerExtendedComponent, TriggerEvent>(OnTrigger);
    }

    private void OnInit(EntityUid uid, SignallerExtendedComponent component, ComponentInit args)
    {
        for (int i = 0; i < component.Count; i++)
        {
            if (i >= MAX_COUNT) break;
            _link.EnsureSourcePorts(uid, "Pressed" + i);
        }
    }

    private void OnUseInHand(EntityUid uid, SignallerExtendedComponent component, UseInHandEvent args)
    {
        if (args.Handled)
            return;

        _adminLogger.Add(LogType.Action, LogImpact.Low, $"{ToPrettyString(args.User):actor} triggered signaler {ToPrettyString(uid):tool}");
        for (int i = 0; i < component.Count; i++)
        {
            if (i >= MAX_COUNT) break;
            _link.InvokePort(uid, "Pressed" + i);
        }
        args.Handled = true;
    }

    private void OnTrigger(EntityUid uid, SignallerExtendedComponent component, TriggerEvent args)
    {
        if (!TryComp(uid, out UseDelayComponent? useDelay)
            // if on cooldown, do nothing
            // and set cooldown to prevent clocks
            || !_useDelay.TryResetDelay((uid, useDelay), true))
            return;

        for (int i = 0; i < component.Count; i++)
        {
            if (i >= MAX_COUNT) break;
            _link.InvokePort(uid, "Pressed" + i);
        }
        args.Handled = true;
    }
}
