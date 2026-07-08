using GameCreator.Editor.Common;
using NinjutsuGames.FusionNetwork.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace NinjutsuGames.FusionNetwork.Editor
{
    [CustomEditor(typeof(SessionListUI))]
    public class SessionListUIEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            
            var content = serializedObject.FindProperty("m_Content");
            var prefab = serializedObject.FindProperty("m_Prefab");
            var emptyMessage = serializedObject.FindProperty("m_EmptyMessage");
            
            root.Add(new SpaceSmaller());
            root.Add(new PropertyField(content));
            root.Add(new PropertyField(prefab));
            root.Add(new PropertyField(emptyMessage));
            root.Add(new SpaceSmall());
            
            
            var sortDirection = serializedObject.FindProperty("m_SortDirection");
            root.Add(new PropertyField(sortDirection));
            
            var sortFieldIndex = serializedObject.FindProperty("m_SortFieldIndex");
            root.Add(new PropertyField(sortFieldIndex));

            return root;
        }
    }
}