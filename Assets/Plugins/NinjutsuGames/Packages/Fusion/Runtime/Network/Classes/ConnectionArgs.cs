using System;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Serializable]
    public class ConnectionArgs
    {
        public string SelectedModel
        {
            get => PlayerPrefs.GetString("Fusion.SelectedModel", "");
            set => PlayerPrefs.SetString("Fusion.SelectedModel", value);
        }
        public string UserName { get; set; }

        public string SelectedRegion
        {
            get => PlayerPrefs.GetString("Fusion.PreferredRegion");
            set => PlayerPrefs.SetString("Fusion.PreferredRegion", value);
        }
        public int SelectedRegionIndex
        {
            get => PlayerPrefs.GetInt("Fusion.PreferredRegionIndex");
            set => PlayerPrefs.SetInt("Fusion.PreferredRegionIndex", value);
        }
        public string CustomAppVersion
        {
            get => PlayerPrefs.GetString("Fusion.CustomAppVersion");
            set => PlayerPrefs.SetString("Fusion.CustomAppVersion", value);
        }
    }
}