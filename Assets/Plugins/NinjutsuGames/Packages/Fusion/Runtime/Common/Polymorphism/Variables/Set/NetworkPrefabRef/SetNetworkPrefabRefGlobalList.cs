using System;
using Fusion;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Global List Variable")]
    [Category("Variables/Global List Variable")]
    
    [Description("Sets the Network Prefab Ref value of a Global List Variable")]
    [Image(typeof(IconListVariable), ColorTheme.Type.Teal, typeof(OverlayDot))]

    [Serializable]
    public class SetNetworkPrefabRefGlobalList : PropertyTypeSetNetworkPrefabRef
    {
        [SerializeField]
        protected FieldSetGlobalList m_Variable = new(ValueNetworkPrefabRef.TYPE_ID);

        public override void Set(NetworkPrefabRef value, Args args) => m_Variable.Set(value, args);
        public override NetworkPrefabRef Get(Args args) => m_Variable.Get(args) is NetworkPrefabRef ? (NetworkPrefabRef)m_Variable.Get(args) : default;

        public static PropertySetNetworkPrefabRef Create => new(
            new SetNetworkPrefabRefGlobalList()
        );
        
        public override string String => m_Variable.ToString();
    }
}