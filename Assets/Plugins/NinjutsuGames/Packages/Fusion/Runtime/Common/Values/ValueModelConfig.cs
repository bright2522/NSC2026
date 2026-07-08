using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using UnityEditor;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Image(typeof(IconCharacter), ColorTheme.Type.Yellow)]
    [Title("Model Config")]
    [Category("Fusion/Model Config")]
    
    [Serializable]
    public class ValueModelConfig : TValue
    {
        public static readonly IdString TYPE_ID = new("model-config");
        
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField] private ModelConfig m_Value;
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public override IdString TypeID => TYPE_ID;
        public override Type Type => typeof(ModelConfig);
        
        public override bool CanSave => false;
        
        public override TValue Copy => new ValueModelConfig
        {
            m_Value = m_Value
        };

        // CONSTRUCTORS: --------------------------------------------------------------------------
        
        public ValueModelConfig()
        { }

        public ValueModelConfig(ModelConfig value) : this()
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
            m_Value = value is ModelConfig @ref ? @ref : default;
        }
        
        public override string ToString()
        {
            return m_Value.ToString();
        }
        
        // REGISTRATION METHODS: ------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RuntimeInit() => RegisterValueType(
            TYPE_ID, 
            new TypeData(typeof(ValueModelConfig), CreateValue),
            typeof(ModelConfig)
        );
        
        #if UNITY_EDITOR
        
        [InitializeOnLoadMethod]
        private static void EditorInit() => RegisterValueType(
            TYPE_ID, 
            new TypeData(typeof(ValueModelConfig), CreateValue),
            typeof(ModelConfig)
        );
        
        #endif

        private static ValueModelConfig CreateValue(object value)
        {
            return new ValueModelConfig(value is ModelConfig @ref ? @ref : default);
        }
    }
}