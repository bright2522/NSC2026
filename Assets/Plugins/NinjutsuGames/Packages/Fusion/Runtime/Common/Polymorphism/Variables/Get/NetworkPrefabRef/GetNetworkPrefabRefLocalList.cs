using System;
using Fusion;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Local List Variable")]
    [Category("Variables/Local List Variable")]
    
    [Image(typeof(IconListVariable), ColorTheme.Type.Teal)]
    [Description("Returns the Network Prefab Ref value of a Local List Variable")]

    [Serializable]
    public class GetNetworkPrefabRefLocalList : PropertyTypeGetNetworkPrefabRef
    {
        [SerializeField]
        protected FieldGetLocalList m_Variable = new(ValueNetworkPrefabRef.TYPE_ID);

        public override NetworkPrefabRef Get(Args args) => m_Variable.Get<NetworkPrefabRef>(args);

        public override string String => m_Variable.ToString();
    }
}