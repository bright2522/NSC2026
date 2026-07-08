using System;
using Fusion;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Global List Variable")]
    [Category("Variables/Global List Variable")]
    
    [Image(typeof(IconListVariable), ColorTheme.Type.Teal, typeof(OverlayDot))]
    [Description("Returns the Network Prefab Ref value of a Global List Variable")]

    [Serializable]
    public class GetNetworkPrefabRefGlobalList : PropertyTypeGetNetworkPrefabRef
    {
        [SerializeField]
        protected FieldGetGlobalList m_Variable = new(ValueNetworkPrefabRef.TYPE_ID);

        public override NetworkPrefabRef Get(Args args) => m_Variable.Get<NetworkPrefabRef>(args);

        public override string String => m_Variable.ToString();
    }
}