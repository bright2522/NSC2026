using GameCreator.Editor.Common;
using NinjutsuGames.FusionNetwork.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace NinjutsuGames.FusionNetwork.Editor
{
    [CustomEditor(typeof(SessionItemUI))]
    public class SessionItemUIEditor : UnityEditor.Editor
    {
        private static readonly StyleLength DefaultMarginTop = new(5);

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement
            {
                style =
                {
                    marginTop = DefaultMarginTop
                }
            };
            
            var alternateBackground = serializedObject.FindProperty("m_AlternateBackground");
            root.Add(new PropertyField(alternateBackground));
            
            var joinButton = serializedObject.FindProperty("joinButton");
            root.Add(new PropertyField(joinButton));
            
            root.Add(new SpaceSmall());
            
            var fields = serializedObject.FindProperty("fieldList");
            root.Add(new PropertyField(fields));

            return root;
        }
    }
}