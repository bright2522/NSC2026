using System;
using Fusion;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Local Name Variable")]
    [Category("Variables/Local Name Variable")]
    
    [Image(typeof(IconNameVariable), ColorTheme.Type.Purple)]
    [Description("Returns the Network Prefab Ref value of a Local Name Variable")]

    [Serializable]
    public class GetNetworkPrefabRefLocalName : PropertyTypeGetNetworkPrefabRef
    {
        [SerializeField]
        protected FieldGetLocalName m_Variable = new(ValueNetworkPrefabRef.TYPE_ID);

        public override NetworkPrefabRef Get(Args args) => m_Variable.Get<NetworkPrefabRef>(args);

        public override string String => m_Variable.ToString();
    }
}