using System;
using Fusion;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Local Name Variable")]
    [Category("Variables/Local Name Variable")]
    
    [Description("Sets the Network Prefa bRef value of a Local Name Variable")]
    [Image(typeof(IconNameVariable), ColorTheme.Type.Purple)]

    [Serializable]
    public class SetNetworkPrefabRefLocalName : PropertyTypeSetNetworkPrefabRef
    {
        [SerializeField]
        protected FieldSetLocalName m_Variable = new(ValueNetworkPrefabRef.TYPE_ID);

        public override void Set(NetworkPrefabRef value, Args args) => m_Variable.Set(value, args);
        public override NetworkPrefabRef Get(Args args) => m_Variable.Get(args) is NetworkPrefabRef ? (NetworkPrefabRef)m_Variable.Get(args) : default;

        public static PropertySetNetworkPrefabRef Create => new(
            new SetNetworkPrefabRefLocalName()
        );
        
        public override string String => m_Variable.ToString();
    }
}