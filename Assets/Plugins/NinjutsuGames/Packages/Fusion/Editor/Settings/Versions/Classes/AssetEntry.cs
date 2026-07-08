using System;
using System.Collections.Generic;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Editor
{
    [Serializable]
    internal class AssetEntry
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        [SerializeField] private AssetVersion version = new();
        [SerializeField] private AssetRelease release = new();
        [SerializeField] private AssetChanges changes = new();
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public AssetVersion Version => version;
        public AssetRelease Release => release;
        public AssetChanges Changes => changes;

        [field: NonSerialized] public State State { get; set; } = State.Loading;
        public bool Unavailable => Release?.Date == null || Release.Date.Month == "Unknown";

        // CONSTRUCTOR: ---------------------------------------------------------------------------

        public AssetEntry()
        { }

        public AssetEntry(State state) : this()
        {
            State = state;
        }

        public AssetEntry(string assetVersion, string date, AssetChanges assetChanges)
        {
            version = new AssetVersion(assetVersion);
            release = new AssetRelease(date);
            changes = assetChanges;
        }
        
        public AssetEntry(AssetVersion assetVersion)
        {
            version = assetVersion;
        }
    }
}