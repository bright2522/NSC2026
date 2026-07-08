using NinjutsuGames.FusionNetwork.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace NinjutsuGames.FusionNetwork.Editor
{
    [CustomPropertyDrawer(typeof(SceneConfig))]

    public class SceneConfigDrawer : PropertyDrawer
    {
        // OVERRIDE METHODS: ----------------------------------------------------------------------
        
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();
            var name = property.FindPropertyRelative("name");
            var sprite = property.FindPropertyRelative("sprite");
            var scene = property.FindPropertyRelative("scene");
            
            root.Add(new PropertyField(name));

            var prefabField = new PropertyField(scene, "");
            root.Add(prefabField);
            root.Add(new PropertyField(sprite));

            return root;
        }
    }
}