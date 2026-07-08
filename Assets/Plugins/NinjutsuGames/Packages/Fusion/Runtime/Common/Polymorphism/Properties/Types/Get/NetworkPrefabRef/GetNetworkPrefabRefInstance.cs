using System;
using Fusion;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Network Prefab Ref")]
    [Category("Network Prefab Ref")]
    
    [Image(typeof(IconCubeSolid), ColorTheme.Type.Green)]
    [Description("A Network Prefab reference")]

    [Serializable, HideLabelsInEditor]
    public class GetNetworkPrefabRefInstance : PropertyTypeGetNetworkPrefabRef
    {
        [SerializeField] private NetworkPrefabRef prefab;

        public override NetworkPrefabRef Get(Args args) => prefab;
        
        public override NetworkPrefabRef Get(GameObject gameObject) => prefab;

        public static PropertyGetNetworkPrefabRef Create()
        {
            var instance = new GetNetworkPrefabRefInstance();
            return new PropertyGetNetworkPrefabRef(instance);
        }

        public override string String => $"{prefab.GetPrefabName()}";
    }
}