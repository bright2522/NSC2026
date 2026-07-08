using GameCreator.Editor.Common;
using NinjutsuGames.FusionNetwork.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace NinjutsuGames.FusionNetwork.Editor
{
    [CustomPropertyDrawer(typeof(RegionItem))]
    public class RegionItemDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();

            var enabled = property.FindPropertyRelative("m_Enabled");
            var fieldEnabled = new PropertyField(enabled);
            
            var name = property.FindPropertyRelative("m_Name");
            var fieldName = new PropertyField(name);
            fieldName.SetEnabled(false);
            
            var token = property.FindPropertyRelative("m_Token");
            var fieldToken = new PropertyField(token);
            fieldToken.SetEnabled(false);
            
            root.Add(new SpaceSmallest());
            root.Add(fieldEnabled);
            root.Add(fieldName);
            root.Add(fieldToken);

            return root;
        }
    }
}