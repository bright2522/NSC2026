using GameCreator.Editor.Common;
using NinjutsuGames.FusionNetwork.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace NinjutsuGames.FusionNetwork.Editor
{
    [CustomPropertyDrawer(typeof(RegionSettings))]
    public class RegionSettingsDrawer : TBoxDrawer
    {
        protected override string Name(SerializedProperty property) => "Region";
        
        protected override void CreatePropertyContent(VisualElement container, SerializedProperty property)
        {
            container.Clear();

            var regionType = property.FindPropertyRelative("regionType");
            var fixedRegion = property.FindPropertyRelative("region");
            var cachedRegion = property.FindPropertyRelative("useCachedRegions");

            var fieldRegionType = new PropertyField(regionType);
            var fieldFixedRegion = new PropertyField(fixedRegion);
            var fieldCachedRegion = new PropertyField(cachedRegion);
            
            fieldRegionType.RegisterValueChangeCallback(_ =>
            {
                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();
                
                fieldFixedRegion.style.display = regionType.enumValueIndex == 0 ? DisplayStyle.None : DisplayStyle.Flex;
                fieldCachedRegion.style.display = regionType.enumValueIndex == 0 ? DisplayStyle.Flex : DisplayStyle.None;
            });
            
            container.Add(fieldRegionType);
            container.Add(fieldFixedRegion);
            container.Add(fieldCachedRegion);
            
            fieldFixedRegion.style.display = regionType.enumValueIndex == 0 ? DisplayStyle.None : DisplayStyle.Flex;
            fieldCachedRegion.style.display = regionType.enumValueIndex == 0 ? DisplayStyle.Flex : DisplayStyle.None;

            fieldRegionType.Bind(property.serializedObject);
            fieldFixedRegion.Bind(property.serializedObject);
            fieldCachedRegion.Bind(property.serializedObject);
        }
    }
}