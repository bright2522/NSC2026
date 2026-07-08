using System.Collections.Generic;
using System.Linq;
using GameCreator.Editor.Common;
using NinjutsuGames.FusionNetwork.Runtime;
using UnityEditor;
using UnityEngine.UIElements;

namespace NinjutsuGames.FusionNetwork.Editor
{
    [CustomPropertyDrawer(typeof(RegionSelector))]
    public class RegionSelectorDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var regions = FusionRepository.Get.Regions.RegionList.GetAvailable();
            var root = new VisualElement();
            
            var regionNames = new List<string>();
            foreach (var region in regions)
            {
                regionNames.Add(region.Title);
            }

            var regionProp = property.FindPropertyRelative("region");

            // Get the current region token
            var currentRegionToken = regionProp.stringValue;
            var currentIndex = 0;
            for (var i = 0; i < regions.Length; i++)
            {
                if (string.IsNullOrEmpty(currentRegionToken)) break;
                if (regions[i].Token == currentRegionToken)
                {
                    currentIndex = i;
                    break;
                }
            }

            var selectedRegion = regions[currentIndex];
            if (selectedRegion != null)
            {
                regionProp.stringValue = selectedRegion.Token;
                property.serializedObject.ApplyModifiedProperties();
            }
            
            // Create a dropdown menu with all available regions
            var regionDropdown = new PopupField<string>("Region", regionNames, currentIndex);
            // hide the label
            regionDropdown.labelElement.style.opacity = 0;
            regionDropdown.RegisterValueChangedCallback(evt =>
            {
                var selectedRegion = regions.FirstOrDefault(region => region.Title == evt.newValue);
                regionProp.stringValue = selectedRegion == null || string.IsNullOrEmpty(selectedRegion.Token) ? string.Empty : selectedRegion.Token;

                property.serializedObject.ApplyModifiedProperties();

            });
            AlignLabel.On(regionDropdown);
            root.Add(regionDropdown);

            return root;
        }
    }
}