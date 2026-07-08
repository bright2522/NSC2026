using GameCreator.Editor.Common;
using NinjutsuGames.FusionNetwork.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace NinjutsuGames.FusionNetwork.Editor
{
    [CustomPropertyDrawer(typeof(AuthenticationSettings))]
    public class AuthenticationSettingsDrawer : TBoxDrawer
    {
        protected override string Name(SerializedProperty property) => "Authentication";
        
        protected override void CreatePropertyContent(VisualElement container, SerializedProperty property)
        {
            container.Clear();

            var authType = property.FindPropertyRelative("authType");
            var values = property.FindPropertyRelative("values");

            var fieldAuthType = new PropertyField(authType);
            var fieldValues = new PropertyField(values);
            
            fieldAuthType.RegisterValueChangeCallback(_ =>
            {
                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();
                
                fieldValues.SetEnabled(authType.enumValueIndex != 255);
            });
            
            container.Add(fieldAuthType);
            container.Add(new SpaceSmaller());
            container.Add(fieldValues);
            
            fieldValues.SetEnabled(authType.enumValueIndex != 255);

            fieldAuthType.Bind(property.serializedObject);
            fieldValues.Bind(property.serializedObject);
        }
    }
}