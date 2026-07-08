using GameCreator.Editor.Common;
using NinjutsuGames.FusionNetwork.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace NinjutsuGames.FusionNetwork.Editor
{
    [CustomPropertyDrawer(typeof(ShutdownErrorItem))]
    public class ShutdownErrorItemDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();
            
            var name = property.FindPropertyRelative("m_Title");
            var fieldName = new PropertyField(name);
            
            var message = property.FindPropertyRelative("m_Message");
            var fieldMessage = new PropertyField(message);
            
            root.Add(new SpaceSmallest());
            root.Add(fieldName);
            root.Add(fieldMessage);

            return root;
        }
    }
}