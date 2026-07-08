using System;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Editor
{
    [Serializable]
    internal class AssetChanges
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        [SerializeField] private string[] @new = Array.Empty<string>();
        [SerializeField] private string[] enhanced = Array.Empty<string>();
        [SerializeField] private string[] changed = Array.Empty<string>();
        [SerializeField] private string[] removed = Array.Empty<string>();
        [SerializeField] private string[] @fixed = Array.Empty<string>();

        // CONSTRUCTORS: --------------------------------------------------------------------------
        
        public AssetChanges() { }
        
        public AssetChanges(string[] @new, string[] enhanced, string[] changed, string[] removed, string[] @fixed)
        {
            this.@new = @new ?? Array.Empty<string>();
            this.enhanced = enhanced ?? Array.Empty<string>();
            this.changed = changed ?? Array.Empty<string>();
            this.removed = removed ?? Array.Empty<string>();
            this.@fixed = @fixed ?? Array.Empty<string>();
        }

        // PROPERTIES: ----------------------------------------------------------------------------
        
        public string[] New => @new;
        public string[] Enhanced => enhanced;
        public string[] Changed => changed;
        public string[] Removed => removed;
        public string[] Fixed => @fixed;
    }
}