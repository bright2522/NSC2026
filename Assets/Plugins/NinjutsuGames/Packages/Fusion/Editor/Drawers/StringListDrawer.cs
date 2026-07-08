using NinjutsuGames.FusionNetwork.Runtime;
using UnityEditor;
using UnityEngine.UIElements;

namespace NinjutsuGames.FusionNetwork.Editor
{
    [CustomPropertyDrawer(typeof(StringList))]
    public class StringListDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            return new StringListTool(property);
        }
    }
}