using System;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Serializable]
    public class FusionRegions
    {
        [SerializeField] private RegionList regionList;
        
        public RegionList RegionList => regionList;
    }
}
