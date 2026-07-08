using GameCreator.Editor.Common;
using NinjutsuGames.FusionNetwork.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace NinjutsuGames.FusionNetwork.Editor
{
    [CustomPropertyDrawer(typeof(StringItem))]
    public class StringItemDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();

            var name = property.FindPropertyRelative("m_Name");
            var fieldName = new PropertyField(name);
            
            var value = property.FindPropertyRelative("m_Value");
            var fieldValue = new PropertyField(value);
            
            root.Add(new SpaceSmallest());
            root.Add(fieldName);
            root.Add(fieldValue);
            return root;
        }
    }
}