using System;
using Fusion;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Global Name Variable")]
    [Category("Variables/Global Name Variable")]
    
    [Description("Sets the Network Prefab Ref value of a Global Name Variable")]
    [Image(typeof(IconNameVariable), ColorTheme.Type.Purple, typeof(OverlayDot))]

    [Serializable]
    public class SetNetworkPrefabRefGlobalName : PropertyTypeSetNetworkPrefabRef
    {
        [SerializeField]
        protected FieldSetGlobalName m_Variable = new(ValueNetworkPrefabRef.TYPE_ID);

        public override void Set(NetworkPrefabRef value, Args args) => m_Variable.Set(value, args);
        public override NetworkPrefabRef Get(Args args) => m_Variable.Get(args) is NetworkPrefabRef ? (NetworkPrefabRef)m_Variable.Get(args) : default;

        public static PropertySetNetworkPrefabRef Create => new(
            new SetNetworkPrefabRefGlobalName()
        );
        
        public override string String => m_Variable.ToString();
    }
}