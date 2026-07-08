using GameCreator.Editor.Common;
using NinjutsuGames.FusionNetwork.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace NinjutsuGames.FusionNetwork.Editor
{
    [CustomEditor(typeof(SessionListUITab))]
    public class SessionListUITabEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            var sessionListUI = serializedObject.FindProperty("m_SessionListUI");
            var sortDirection = serializedObject.FindProperty("m_SortDirection");
            var sortIndex = serializedObject.FindProperty("m_SortIndex");

            root.Add(new PropertyField(sessionListUI));
            root.Add(new SpaceSmaller());
            root.Add(new PropertyField(sortDirection));
            root.Add(new PropertyField(sortIndex));
            
            var activeIndex = serializedObject.FindProperty("m_ActiveIndex");
            var directionArrow = serializedObject.FindProperty("m_DirectionArrow");
            
            root.Add(new SpaceSmall());
            root.Add(new PropertyField(activeIndex));
            root.Add(new PropertyField(directionArrow));

            return root;
        }
    }
}