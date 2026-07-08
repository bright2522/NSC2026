using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Serializable]
    public class RegionItem : TPolymorphicItem<RegionItem>
    {
        [SerializeField] private string m_Name;
        [SerializeField] private string m_Token;
        
        private int _ping = -1;

        // PROPERTIES: ----------------------------------------------------------------------------
        public override string Title => $"{m_Name} ({m_Token}){(_ping == -1 ? string.Empty : $" - {_ping}ms")}";
        public string Name => m_Name;
        public string Token => m_Token;
        public int Ping => _ping;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public RegionItem(string name, string token)
        {
            m_Name = name;
            m_Token = token;
            _ping = -1;
        }

        public RegionItem()
        {
            m_Name = "New Region";
            m_Token = "nr";
            _ping = -1;
        }

        public void SetPing(int ping)
        {
            _ping = ping;
        }
    }
}