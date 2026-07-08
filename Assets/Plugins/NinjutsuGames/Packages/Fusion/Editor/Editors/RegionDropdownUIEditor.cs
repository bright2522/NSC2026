using GameCreator.Editor.Common;
using NinjutsuGames.FusionNetwork.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace NinjutsuGames.FusionNetwork.Editor
{
    [CustomEditor(typeof(RegionDropdownUI))]
    public class RegionDropdownUIEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            
            var defaultRegion = serializedObject.FindProperty("defaultRegion");
            var pingRegions = serializedObject.FindProperty("pingRegions");
            root.Add(new SpaceSmaller());
            root.Add(new PropertyField(defaultRegion));
            root.Add(new PropertyField(pingRegions));

            return root;
        }
    }
}