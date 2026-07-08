using System;
using Fusion;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using UnityEditor;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Image(typeof(IconCubeOutline), ColorTheme.Type.Green)]
    [Title("Network Prefab Ref")]
    [Category("Fusion/Network Prefab Ref")]
    
    [Serializable]
    public class ValueNetworkPrefabRef : TValue
    {
        public static readonly IdString TYPE_ID = new("network-prefab-ref");
        
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField] private NetworkPrefabRef m_Value;
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public override IdString TypeID => TYPE_ID;
        public override Type Type => typeof(NetworkPrefabRef);
        
        public override bool CanSave => false;
        
        public override TValue Copy => new ValueNetworkPrefabRef
        {
            m_Value = m_Value
        };

        // CONSTRUCTORS: --------------------------------------------------------------------------
        
        public ValueNetworkPrefabRef()
        { }

        public ValueNetworkPrefabRef(NetworkPrefabRef value) : this()
        {
            m_Value = value;
        }

        // OVERRIDE METHODS: ----------------------------------------------------------------------

        protected override object Get()
        {
            return m_Value;
        }

        protected override void Set(object value)
        {
            m_Value = value is NetworkPrefabRef @ref ? @ref : default;
        }
        
        public override string ToString()
        {
            return m_Value.ToString();
        }
        
        // REGISTRATION METHODS: ------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RuntimeInit() => RegisterValueType(
            TYPE_ID, 
            new TypeData(typeof(ValueNetworkPrefabRef), CreateValue),
            typeof(NetworkPrefabRef)
        );
        
        #if UNITY_EDITOR
        
        [InitializeOnLoadMethod]
        private static void EditorInit() => RegisterValueType(
            TYPE_ID, 
            new TypeData(typeof(ValueNetworkPrefabRef), CreateValue),
            typeof(NetworkPrefabRef)
        );
        
        #endif

        private static ValueNetworkPrefabRef CreateValue(object value)
        {
            return new ValueNetworkPrefabRef(value is NetworkPrefabRef @ref ? @ref : default);
        }
    }
}