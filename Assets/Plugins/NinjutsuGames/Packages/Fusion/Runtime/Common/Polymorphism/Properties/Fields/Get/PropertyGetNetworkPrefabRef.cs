using System;
using Fusion;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Serializable]
    public class PropertyGetNetworkPrefabRef : TPropertyGet<PropertyTypeGetNetworkPrefabRef, NetworkPrefabRef>
    {
        public PropertyGetNetworkPrefabRef() : base(new GetNetworkPrefabRefInstance())
        { }

        public PropertyGetNetworkPrefabRef(PropertyTypeGetNetworkPrefabRef defaultType) : base(defaultType)
        { }

        public T Get<T>(Args args) where T : Component
        {
            return m_Property.Get<T>(args);
        }

        public T Get<T>(GameObject target) where T : Component
        {
            return m_Property.Get<T>(target);
        }
        
        public T Get<T>(Component component) where T : Component
        {
            return m_Property.Get<T>(component);
        }
    }
}