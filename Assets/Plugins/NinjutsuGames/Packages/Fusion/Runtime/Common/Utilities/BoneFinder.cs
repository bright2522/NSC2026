using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    public static class BoneFinder
    {
        // Static dictionary to cache normalized bone names mapped to HumanBodyBones
        private static readonly Dictionary<string, HumanBodyBones> NormalizedBoneDictionary;

        // Static constructor to initialize the dictionary once
        static BoneFinder()
        {
            NormalizedBoneDictionary = new Dictionary<string, HumanBodyBones>(StringComparer.OrdinalIgnoreCase);

            foreach (HumanBodyBones bone in Enum.GetValues(typeof(HumanBodyBones)))
            {
                if (bone == HumanBodyBones.LastBone)
                    continue; // Skip the placeholder value

                var normalizedBoneName = NormalizeBoneName(bone.ToString());

                // Handle potential duplicates due to normalization
                NormalizedBoneDictionary.TryAdd(normalizedBoneName, bone);
            }
        }

        public static HumanBodyBones FindClosestBone(string boneName)
        {
            if (string.IsNullOrEmpty(boneName))
                return HumanBodyBones.LastBone;

            // Normalize the input bone name
            var normalizedInput = NormalizeBoneName(boneName);

            // Direct match
            if (NormalizedBoneDictionary.TryGetValue(normalizedInput, out var exactMatch))
            {
                return exactMatch;
            }

            // Partial match: check if any normalized bone name contains the input
            foreach (var kvp in NormalizedBoneDictionary)
            {
                if (normalizedInput.Contains(kvp.Key) || kvp.Key.Contains(normalizedInput))
                {
                    return kvp.Value;
                }
            }

            return HumanBodyBones.LastBone; // Default or error value
        }

        // Helper method to normalize bone names
        private static string NormalizeBoneName(string name)
        {
            // Remove common prefixes and suffixes
            string[] patterns =
            {
                @"^CC_Base_", @"^CC_", @"^Base_", @"^Bip01_", @"^Bip_", @"^mixamorig_", @"^DEF_",
                @"_jnt$", @"_bone$", @"_L$", @"_R$", @"_L_", @"_R_", @"\.L$", @"\.R$"
            };
            foreach (var pattern in patterns)
            {
                name = Regex.Replace(name, pattern, "", RegexOptions.IgnoreCase);
            }

            // Replace underscores, hyphens, and spaces, then convert to lowercase
            return Regex.Replace(name, @"[_\-\s]", "", RegexOptions.IgnoreCase).ToLowerInvariant();
        }
    }
}