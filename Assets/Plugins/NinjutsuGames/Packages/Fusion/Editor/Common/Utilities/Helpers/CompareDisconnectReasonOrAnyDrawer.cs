using NinjutsuGames.FusionNetwork.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace NinjutsuGames.FusionNetwork.Editor
{
    [CustomPropertyDrawer(typeof(CompareDisconnectReasonOrAny))]
    public class CompareDisconnectReasonOrAnyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();
            var head = new VisualElement();
            var body = new VisualElement(); 

            var option = property.FindPropertyRelative("m_Option");
            var reason = property.FindPropertyRelative("m_DisconnectReason");
            
            var fieldOption = new PropertyField(option, property.displayName);
            var fieldGameObject = new PropertyField(reason, " ");
            head.Add(fieldOption);
            
            fieldOption.RegisterValueChangeCallback(changeEvent =>
            {
                body.Clear();
                if (changeEvent.changedProperty.intValue != 1) return;
                body.Add(fieldGameObject);
                body.Bind(changeEvent.changedProperty.serializedObject);
            });

            if (option.intValue == 1)
            {
                body.Add(fieldGameObject);
            }

            root.Add(head);
            root.Add(body);
            
            return root;
        }
    }
}