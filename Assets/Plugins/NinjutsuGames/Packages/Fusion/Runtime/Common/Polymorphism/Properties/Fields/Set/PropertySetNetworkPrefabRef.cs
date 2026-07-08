using System;
using Fusion;
using GameCreator.Runtime.Common;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Serializable]
    public class PropertySetNetworkPrefabRef : TPropertySet<PropertyTypeSetNetworkPrefabRef, NetworkPrefabRef>
    {
        public PropertySetNetworkPrefabRef() : base(new SetNetworkPrefabRefNone())
        { }

        public PropertySetNetworkPrefabRef(PropertyTypeSetNetworkPrefabRef defaultType) : base(defaultType)
        { }
    }
}