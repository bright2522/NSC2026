using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Serializable]
    public class RegionList : TPolymorphicList<RegionItem>
    {
        [SerializeReference] private RegionItem[] m_Regions =
        {
            new("USA, East", "us"),
            new("USA, West", "usw"),
            new("USA, South Central", "ussc"),
            new("Asia", "asia"),
            new("Australia", "au"),
            new("Canada, East", "cae"),
            new("Chinese Mainland", "cn"),
            new("Europe", "eu"),
            new("Hong Kong", "hk"),
            new("India", "in"),
            new("Japan", "jp"),
            new("South America", "sa"),
            new("South Korea", "kr"),
            new("Turkey", "tr"),
            new("United Arab Emirates", "uae"),
        };
    
        // PROPERTIES: ----------------------------------------------------------------------------

        public override int Length => m_Regions.Length;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void ResetDefaultRegions()
        {
            m_Regions = new RegionItem[]
            {
                new("USA, East", "us"),
                new("USA, West", "usw"),
                new("USA, South Central", "ussc"),
                new("Asia", "asia"),
                new("Australia", "au"),
                new("Canada, East", "cae"),
                new("Chinese Mainland", "cn"),
                new("Europe", "eu"),
                new("Hong Kong", "hk"),
                new("India", "in"),
                new("Japan", "jp"),
                new("South America", "sa"),
                new("South Korea", "kr"),
                new("Turkey", "tr"),
                new("United Arab Emirates", "uae"),
            };
        }

        public RegionItem Get(int index) => m_Regions[index];
        
        public RegionItem[] GetAvailable()
        {
            var list = new RegionItem[m_Regions.Length];
            var count = 0;
            for (var i = 0; i < m_Regions.Length; ++i)
            {
                if (m_Regions[i].IsEnabled)
                {
                    list[count] = m_Regions[i];
                    count++;
                }
            }

            Array.Resize(ref list, count);
            return list;
        }
    }
}