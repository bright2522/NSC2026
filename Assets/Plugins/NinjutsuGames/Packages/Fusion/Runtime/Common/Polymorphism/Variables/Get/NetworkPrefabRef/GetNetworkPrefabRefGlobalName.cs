using System;
using Fusion;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Global Name Variable")]
    [Category("Variables/Global Name Variable")]
    
    [Image(typeof(IconNameVariable), ColorTheme.Type.Purple, typeof(OverlayDot))]
    [Description("Returns the Network Prefab Ref value of a Global Name Variable")]

    [Serializable]
    public class GetNetworkPrefabRefGlobalName : PropertyTypeGetNetworkPrefabRef
    {
        [SerializeField]
        protected FieldGetGlobalName m_Variable = new(ValueGameObject.TYPE_ID);

        public override NetworkPrefabRef Get(Args args) => m_Variable.Get<NetworkPrefabRef>(args);

        public override string String => m_Variable.ToString();
    }
}