using System;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Editor
{
    [Serializable]
    internal class AssetDate
    {
        private static readonly string[] MONTHS = {
            "Unknown",
            "January",
            "February",
            "March",
            "April",
            "May",
            "June",
            "July",
            "August",
            "September",
            "October",
            "November",
            "December"
        };
        
        // MEMBERS: -------------------------------------------------------------------------------
        
        [SerializeField] private int day;
        [SerializeField] private int month;
        [SerializeField] private int year;

        public AssetDate()
        {
            day = 0;
            month = 0;
            year = 0;
        }
        public AssetDate(string date)
        {
            var parts = date.Split(' ');
            if(parts.Length != 3) return;

            day = int.Parse(parts[0]);
            // convert months string to number
            for (var i = 0; i < MONTHS.Length; i++)
            {
                if (parts[1] != MONTHS[i]) continue;
                month = i;
                break;
            }
            year = int.Parse(parts[2]);
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public int Day => day;
        public string Month => MONTHS[month];
        public int Year => year;
        
        // STRING: --------------------------------------------------------------------------------

        public override string ToString() => $"{Month} {Day}, {Year}";
    }
}