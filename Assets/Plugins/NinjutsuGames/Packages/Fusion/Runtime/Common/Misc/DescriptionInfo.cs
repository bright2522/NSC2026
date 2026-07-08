using System;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Serializable]
    public class DescriptionInfo
    {
        public string description;
        public DescriptionInfo(string info)
        {
            description = info;
        }
    }
}