using Content.Server.Atmos;
using Content.Shared.Atmos;

namespace Content.Server.Cursed.Energetics.GasTurbine
{
    [RegisterComponent]
    public sealed partial class GasTurbineComponent : Component, IGasMixtureHolder
    {
        public GasMixture Air { get; set; } = new(volume: 0);

        [DataField("maxProduction")]
        public float MaxProduction = 1000000f;

        /// <summary>
        /// Minimal required pressure to start producing energy.
        /// </summary>
        [DataField("minProducingPressure")]
        public float MinProducingPressure = 400f;

        /// <summary>
        /// Stacking pressure higher won't increase production from this point
        /// </summary>
        [DataField("maxProducingPressure")]
        public float MaxProducingPressure = 1200f;

        /// <summary>
        /// Pressure at which the turbine shall explode.
        /// </summary>
        [DataField("explosionPressure")]
        public float ExplosionPressure = 1400f;

        /// <summary>
        /// Turbine outlet volume coefficient <br/>
        /// Turbine expander expands the gas to produce more energy, btw.
        /// </summary>
        [DataField("expanderOutputRate")]
        public float ExpanderRate = 9.2f;

        [DataField("efficiency")]
        public float Efficiency = 0.5f;

        [DataField("expectedVolumeEntry")]
        public float ExpectedVolumeEntry = 200f;
    }
}
