using Content.Shared.DeviceLinking;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Cursed.SignallerExtended
{
    [RegisterComponent]
    public sealed partial class SignallerExtendedComponent : Component
    {
        /// <summary>
        /// How many times the signaler should be triggered.
        /// TODO: antidumb protection from using more than pressed duplicates count?
        /// </summary>
        [DataField("count")]
        public int Count = 1;
    }
}
