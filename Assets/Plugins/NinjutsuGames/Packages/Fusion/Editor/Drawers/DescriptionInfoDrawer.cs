using GameCreator.Editor.Common;
using NinjutsuGames.FusionNetwork.Runtime;
using UnityEditor;
using UnityEngine.UIElements;

namespace NinjutsuGames.FusionNetwork.Editor
{
    [CustomPropertyDrawer(typeof(DescriptionInfo))]
    public class DescriptionInfoDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();
            var info = new InfoMessage(property.FindPropertyRelative("description").stringValue)
            {
                style =
                {
                    marginTop = 3,
                    marginBottom = 3,
                }
            };
            root.Add(info);
            return root;
        }
    }
}