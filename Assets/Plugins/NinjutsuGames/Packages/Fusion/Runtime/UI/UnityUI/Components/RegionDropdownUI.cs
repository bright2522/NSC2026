using System;
using System.Collections.Generic;
using System.Linq;
using GameCreator.Runtime.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [AddComponentMenu("Game Creator/UI/Fusion/Region Dropdown UI")]
    [Icon(RuntimePaths.GIZMOS + "GizmoSessionListUI.png")]
    
    [DefaultExecutionOrder(ApplicationManager.EXECUTION_ORDER_LAST_LATER)]
    [HelpURL("https://docs.ninjutsugames.com/game-creator-2/fusion-module/user-interface#region-selection")]
    [Serializable]
    public class RegionDropdownUI : MonoBehaviour
    {
        [SerializeField] private string defaultRegion = "Best Region";
        [SerializeField] private bool pingRegions = true;
        [SerializeField] private bool selectBestRegion = true;
        private Dropdown _dropdown;
        private TMP_Dropdown _TMPDropdown;
        private RegionItem[] _regions;
        private Dictionary<string, float> _regionPings = new();

        private void Awake()
        {
            _regions = FusionRepository.Get.Regions.RegionList.GetAvailable();
            NetworkManager.EventLobbyStarted += OnLobbyStarted;
            NetworkManager.EventGameStarted += OnGameStarted;
        }

        private void Start()
        {
            if(!string.IsNullOrEmpty(NetworkManager.ConnectionArgs.SelectedRegion))
            {
                SetRegion(NetworkManager.ConnectionArgs.SelectedRegion);
            }
            if (pingRegions) GetRegionsPing();
        }

        private void OnDestroy()
        {
            NetworkManager.EventLobbyStarted -= OnLobbyStarted;
            NetworkManager.EventGameStarted -= OnGameStarted;
        }
        
        [ContextMenu("Ping Regions")]
        private async void GetRegionsPing()
        {
            try
            {
                await NetworkManager.PingRegions();

                if (_TMPDropdown)
                {
                    SetupTextMeshPro();
                    return;
                }

                if (_dropdown)
                {
                    SetupUnity();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e, gameObject);
            }
        }
        
        private void OnLobbyStarted()
        {
            var region = NetworkManager.RunnerLobby.LobbyInfo.Region;
            if(!string.IsNullOrEmpty(NetworkManager.ConnectionArgs.SelectedRegion))
            {
                region = NetworkManager.ConnectionArgs.SelectedRegion;
            }
            if (selectBestRegion)
            {
                var bestRegion = _regions
                    .Where(r => r.Token == region)
                    .OrderBy(r => r.Ping)
                    .FirstOrDefault();
                
                if (bestRegion != null)
                {
                    region = bestRegion.Token;
                }
            }
            SetRegion(region);
        }
        
        private void OnGameStarted()
        {
            var region = NetworkManager.Runner.SessionInfo.Region;
            if(!string.IsNullOrEmpty(NetworkManager.ConnectionArgs.SelectedRegion))
            {
                region = NetworkManager.ConnectionArgs.SelectedRegion;
            }
            SetRegion(region);
        }

        private void SetRegion(string region)
        {
            var regionIndex = Array.FindIndex(_regions, r => r.Token == region) + 1;
            if (_TMPDropdown)
            {
                _TMPDropdown.SetValueWithoutNotify(regionIndex);
            }
            else if (_dropdown)
            {
                _dropdown.SetValueWithoutNotify(regionIndex);
            }
        }

        private void OnEnable()
        {
            _TMPDropdown = gameObject.Get<TMP_Dropdown>();
            if (_TMPDropdown)
            {
                SetupTextMeshPro();
                return;
            }
            
            _dropdown = gameObject.Get<Dropdown>();
            if (_dropdown)
            {
                SetupUnity();
            }
        }

        private void SetupUnity()
        {
            var regionNames = new List<string>();
            regionNames.Add(defaultRegion);
            
            foreach (var region in _regions)
            {
                regionNames.Add(region.Title);
            }
    
            _dropdown.ClearOptions();
            _dropdown.AddOptions(regionNames);
            _dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
            
            OnLobbyStarted();
        }

        private void OnDropdownValueChanged(int index)
        {
            NetworkManager.ConnectionArgs.SelectedRegionIndex = index;
            if (index == 0)
            {
                NetworkManager.Instance.SetRegion(string.Empty, -1);
                return;
            }
            var region = _regions[index - 1];
            NetworkManager.Instance.SetRegion(region.Token, index - 1);
        }

        private void SetupTextMeshPro()
        {
            var regionNames = new List<TMP_Dropdown.OptionData>();
            regionNames.Add(new TMP_Dropdown.OptionData(defaultRegion));

            foreach (var region in _regions)
            {
                regionNames.Add(new TMP_Dropdown.OptionData(region.Title));
            }
    
            _TMPDropdown.ClearOptions();
            _TMPDropdown.AddOptions(regionNames);
            _TMPDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
            
            OnLobbyStarted();
        }
    }
}