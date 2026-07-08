using System;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Editor
{
    [Serializable]
    internal class LatestEntry
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        [SerializeField] private string id;
        [SerializeField] private string path;
        [SerializeField] private bool core;

        // PROPERTIES: ----------------------------------------------------------------------------

        public string Id => id;
        public string Path => path;
        public bool Core => core;
    }
}