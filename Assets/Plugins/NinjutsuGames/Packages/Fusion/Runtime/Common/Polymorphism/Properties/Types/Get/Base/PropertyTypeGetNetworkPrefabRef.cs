using System;
using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Network Prefab Ref")]

    [Serializable]
    public abstract class PropertyTypeGetNetworkPrefabRef : TPropertyTypeGet<NetworkPrefabRef>
    {
        public virtual T Get<T>(Args args) where T : Component
        {
            var prefabRef = Get(args);
            return prefabRef.Get<T>();
        }

        public virtual T Get<T>(GameObject target) where T : Component
        {
            return Get<T>(new Args(target));
        }
        
        public virtual T Get<T>(Component component) where T : Component
        {
            return Get<T>(new Args(component));
        }
    }
}