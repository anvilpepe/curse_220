using Content.Server.Atmos.Piping.Unary.EntitySystems;
using Content.Server.Destructible;
using Content.Server.Destructible.Thresholds.Behaviors;

namespace Content.Server.Cursed.Energetics.GasTurbine;

[Serializable]
[DataDefinition]
public sealed partial class PurgeTurbineContentsBehavior : IThresholdBehavior
{
    public void Execute(EntityUid owner, DestructibleSystem system, EntityUid? cause = null)
    {
        system.EntityManager.EntitySysManager.GetEntitySystem<GasTurbineSystem>().PurgeContents(owner);
    }
}

