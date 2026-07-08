using System;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Editor
{
    [Serializable]
    internal class AssetRelease
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] public bool available;
        [SerializeField] private AssetDate date;
        
        public AssetRelease(){}

        public AssetRelease(string date)
        {
            this.date = new AssetDate(date);
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public bool Available => available;
        public AssetDate Date => date;
    }
}