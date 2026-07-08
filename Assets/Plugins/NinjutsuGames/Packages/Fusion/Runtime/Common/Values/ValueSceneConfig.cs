using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using UnityEditor;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Image(typeof(IconUnity), ColorTheme.Type.White)]
    [Title("Scene Config")]
    [Category("Fusion/Scene Config")]
    
    [Serializable]
    public class ValueSceneConfig : TValue
    {
        public static readonly IdString TYPE_ID = new("scene-config");
        
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField] private SceneConfig m_Value;
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public override IdString TypeID => TYPE_ID;
        public override Type Type => typeof(SceneConfig);
        
        public override bool CanSave => false;
        
        public override TValue Copy => new ValueSceneConfig
        {
            m_Value = m_Value
        };

        // CONSTRUCTORS: --------------------------------------------------------------------------
        
        public ValueSceneConfig()
        { }

        public ValueSceneConfig(SceneConfig value) : this()
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
            m_Value = value is SceneConfig @ref ? @ref : default;
        }
        
        public override string ToString()
        {
            return m_Value.ToString();
        }
        
        // REGISTRATION METHODS: ------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RuntimeInit() => RegisterValueType(
            TYPE_ID, 
            new TypeData(typeof(ValueSceneConfig), CreateValue),
            typeof(SceneConfig)
        );
        
        #if UNITY_EDITOR
        
        [InitializeOnLoadMethod]
        private static void EditorInit() => RegisterValueType(
            TYPE_ID, 
            new TypeData(typeof(ValueSceneConfig), CreateValue),
            typeof(SceneConfig)
        );
        
        #endif

        private static ValueSceneConfig CreateValue(object value)
        {
            return new ValueSceneConfig(value is SceneConfig @ref ? @ref : default);
        }
    }
}