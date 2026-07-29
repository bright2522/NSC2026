#if CMPSETUP_COMPLETE
using UnityEngine;
using System.Collections.Generic;

namespace AvocadoShark
{
    public class PlayerHUDManager : MonoBehaviour
    {
        public static PlayerHUDManager Instance { get; private set; }

        [SerializeField] private GameObject playerUiPrefab;
        [SerializeField] private Transform hudContainer;

        private readonly List<GameObject> activeUiItems = new List<GameObject>();

        public void Configure(GameObject prefab, Transform container)
        {
            playerUiPrefab = prefab;
            hudContainer = container;
        }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        public void RefreshHUD()
        {
            foreach (var item in activeUiItems)
            {
                if (item != null)
                    Destroy(item);
            }
            activeUiItems.Clear();

            if (SessionPlayers.instance == null || SessionPlayers.instance.activePlayers == null)
                return;
            if (playerUiPrefab == null || hudContainer == null)
                return;

            foreach (PlayerStats player in SessionPlayers.instance.activePlayers)
            {
                if (player == null)
                    continue;

                GameObject uiObj = Instantiate(playerUiPrefab, hudContainer);
                PlayerUIItem uiItem = uiObj.GetComponent<PlayerUIItem>();
                if (uiItem != null)
                    uiItem.Setup(player);

                activeUiItems.Add(uiObj);
            }
        }
    }
}
#endif
