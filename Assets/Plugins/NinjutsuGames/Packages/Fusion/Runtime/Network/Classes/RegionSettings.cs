using System;
using GameCreator.Runtime.Common;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Serializable]
    public class RegionSettings
    {
        public enum RegionType
        {
            BestRegion,
            FixedRegion,
        }
        
        public RegionType regionType = RegionType.BestRegion;
        public PropertyGetString region = GetStringAvailableRegion.Create();
        public PropertyGetBool useCachedRegions = GetBoolFalse.Create;

    }
}