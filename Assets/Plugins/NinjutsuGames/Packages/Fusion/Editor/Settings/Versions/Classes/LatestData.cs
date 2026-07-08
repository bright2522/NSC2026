using System;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Editor
{
    [Serializable]
    internal class LatestData
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        [SerializeField] private LatestEntry[] list = Array.Empty<LatestEntry>();
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public LatestEntry[] List => list;
        
        [field: NonSerialized] public State State { get; set; } = State.Loading;
        
        // CONSTRUCTOR: ---------------------------------------------------------------------------
        
        public LatestData()
        { }

        public LatestData(State state) : this()
        {
            State = state;
        }
    }
}