using Content.Server.Atmos.EntitySystems;
using Content.Server.NodeContainer.Nodes;
using Content.Server.NodeContainer;
using Content.Server.Atmos.Piping.Components;
using Content.Server.Power.Components;
using Content.Shared.Atmos;
using Content.Shared.Damage;
using Content.Shared.Destructible;
using Content.Server.Atmos;
using Robust.Server.GameObjects;
using Content.Server.Administration.Logs;
using Content.Shared.Database;
using Robust.Shared.Random;
using Content.Shared.Damage.Prototypes;
using Robust.Shared.Toolshed.TypeParsers;
using Robust.Shared.Prototypes;

namespace Content.Server.Cursed.Energetics.GasTurbine;

/// <summary>
/// Handles the Gas Turbine.
/// </summary>
public sealed class GasTurbineSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmosphere = default!;
    [Dependency] private readonly TransformSystem _xform = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<GasTurbineComponent, AtmosDeviceUpdateEvent>(TurbineUpdate);
        SubscribeLocalEvent<GasTurbineComponent, DamageChangedEvent>(OnDamageChanged);
        base.Initialize();
    }

    private void TurbineUpdate(EntityUid uid, GasTurbineComponent component, ref AtmosDeviceUpdateEvent args)
    {
        var xform = Transform(uid);
        // are you fucking CRAZY to run WHOLE ASS TURBINE unanchored?
        if (!xform.Anchored) return;

        var supplier = Comp<PowerSupplierComponent>(uid) ?? throw new Exception("No power supplier");
        var nodeContainer = Comp<NodeContainerComponent>(uid) ?? throw new Exception("No node container");
        var inlet = (PipeNode)nodeContainer.Nodes["inlet"] ?? throw new Exception("No inlet node");
        var outlet = (PipeNode)nodeContainer.Nodes["outlet"] ?? throw new Exception("No outlet node");

        component.Air = inlet.Air.Clone(); // TODO: make this as IGasMixtureHolder implementation
        if (inlet.Air.Volume > component.ExpectedVolumeEntry) component.Air.Volume = component.ExpectedVolumeEntry;
        inlet.Air.RemoveVolume(Math.Min(component.ExpectedVolumeEntry, inlet.Air.Volume));
        // probable excessive gas compression, but this is YOUR fault.

        if (component.Air.Pressure < component.MinProducingPressure) { supplier.MaxSupply = 0; return; }
        if (component.Air.Pressure > component.ExplosionPressure)
        {
            if (!_protoMan.TryIndex<DamageTypePrototype>("Structural", out var proto)) return;
            _damageable.TryChangeDamage(uid, new(proto, 100), damageable: Comp<DamageableComponent>(uid));
        }

        var mass = component.Air.Volume * component.Air.Pressure / Atmospherics.R / component.Air.Temperature;
        var work = mass * Atmospherics.R * inlet.Air.Temperature * Math.Log(component.Air.Pressure / component.ExpanderRate / inlet.Air.Pressure);
        work *= component.Efficiency;

        supplier.MaxSupply = (float)work;

        outlet.Air = component.Air.Clone();
        outlet.Air.Volume *= component.ExpanderRate;
        component.Air.Clear();
    }

    public void PurgeContents(EntityUid uid, GasTurbineComponent? turbine = null, TransformComponent? transform = null)
    {
        if (!Resolve(uid, ref turbine, ref transform))
            return;

        var environment = _atmosphere.GetContainingMixture((uid, transform), false, true);
        if (environment is not null)
            _atmosphere.Merge(environment, turbine.Air);

        _adminLogger.Add(LogType.CanisterPurged, LogImpact.Medium, $"Purged contents of {ToPrettyString(uid):turbine}");
        turbine.Air.Clear();
    }

    /// <summary>
    /// Representing CYKA BLYAT(TM) efficiency boost (or decrease).
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="component"></param>
    /// <param name="args"></param>
    private void OnDamageChanged(EntityUid uid, GasTurbineComponent component, DamageChangedEvent args)
    {
        if (!args.DamageIncreased || args.DamageDelta is null) return;
        Random random = new();
        var sum = 0f;
        foreach (var damage in args.DamageDelta.DamageDict.Values) sum += damage.Float();
        if (sum <= 0) return;
        component.Efficiency += random.Pick((ICollection<float>)[-0.001f, 0.001f]) * sum;
    }
}
