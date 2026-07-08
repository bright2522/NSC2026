using GameCreator.Editor.Common;
using NinjutsuGames.FusionNetwork.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace NinjutsuGames.FusionNetwork.Editor
{
    [CustomPropertyDrawer(typeof(FieldItem))]
    public class FieldItemDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();

            var type = property.FindPropertyRelative("m_Type");
            var fieldType = new PropertyField(type);
            
            var text = property.FindPropertyRelative("m_Text");
            var fieldText = new PropertyField(text);
            
            var number = property.FindPropertyRelative("m_Number");
            var fieldNumber = new PropertyField(number);
            
            var str = property.FindPropertyRelative("m_String");
            var fieldString = new PropertyField(str);
            
            var useColor = property.FindPropertyRelative("m_UseColor");
            var fieldUseColor = new PropertyField(useColor);
            
            var color = property.FindPropertyRelative("m_Color");
            var fieldColor = new PropertyField(color);
            
            var useFormat = property.FindPropertyRelative("m_UseFormat");
            var fieldUseFormat = new PropertyField(useFormat);
            
            var format = property.FindPropertyRelative("format");
            var fieldFormat = new PropertyField(format);
            
            root.Add(new SpaceSmallest());
            root.Add(fieldType);
            root.Add(fieldNumber);
            root.Add(fieldString);
            
            root.Add(new SpaceSmall());
            root.Add(fieldText);
            root.Add(new SpaceSmallest());
            root.Add(fieldUseFormat);
            root.Add(fieldFormat);
            
            // root.Add(new SpaceSmaller());
            root.Add(fieldUseColor);
            root.Add(fieldColor);
            
            fieldFormat.style.display = useFormat.boolValue ? DisplayStyle.Flex : DisplayStyle.None;
            fieldFormat.style.paddingBottom = 5;
            fieldFormat.style.paddingLeft = 5;
            
            fieldUseFormat.RegisterValueChangeCallback(evt =>
            {
                fieldFormat.style.display = evt.changedProperty.boolValue ? DisplayStyle.Flex : DisplayStyle.None;
            });
            
            fieldColor.style.display = useColor.boolValue ? DisplayStyle.Flex : DisplayStyle.None;
            fieldColor.style.paddingLeft = 5;
            
            fieldUseColor.RegisterValueChangeCallback(evt =>
            {
                fieldColor.style.display = evt.changedProperty.boolValue ? DisplayStyle.Flex : DisplayStyle.None;
            });
            
            fieldNumber.style.display = type.enumValueIndex == 0 ? DisplayStyle.Flex : DisplayStyle.None;
            fieldString.style.display = type.enumValueIndex == 1 ? DisplayStyle.Flex : DisplayStyle.None;
            
            fieldType.RegisterValueChangeCallback(evt =>
            {
                fieldNumber.style.display = evt.changedProperty.enumValueIndex == 0 ? DisplayStyle.Flex : DisplayStyle.None;
                fieldString.style.display = evt.changedProperty.enumValueIndex == 1 ? DisplayStyle.Flex : DisplayStyle.None;
            });


            return root;
        }
    }
}