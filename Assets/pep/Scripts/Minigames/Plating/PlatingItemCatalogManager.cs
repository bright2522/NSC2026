using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pep.Minigames.Plating
{
    public class PlatingItemCatalogManager : MonoBehaviour
    {
        [SerializeField] private List<PlateItemSO> availableItems = new List<PlateItemSO>();
        [SerializeField] private Transform trayRoot;
        [SerializeField] private float trayItemSpacing = 1.8f;
        [SerializeField] private bool autoSpawnTrayOnStart = true;

        public event Action<DraggablePlateItem> OnItemPickedUp;
        public event Action<DraggablePlateItem> OnItemDropped;

        private readonly List<DraggablePlateItem> spawnedTrayItems = new List<DraggablePlateItem>();
        private readonly Dictionary<string, int> placedCountByItemId = new Dictionary<string, int>();

        public IReadOnlyList<PlateItemSO> AvailableItems => availableItems;
        public IReadOnlyList<DraggablePlateItem> SpawnedTrayItems => spawnedTrayItems;

        private void Start()
        {
            if (autoSpawnTrayOnStart)
                SpawnTray();
        }

        public void SpawnTray()
        {
            ClearTray();

            if (trayRoot == null)
            {
                var go = new GameObject("PlatingTrayRoot");
                trayRoot = go.transform;
            }

            for (int i = 0; i < availableItems.Count; i++)
            {
                PlateItemSO data = availableItems[i];
                if (data == null || data.WorldPrefab == null) continue;

                Vector3 pos = trayRoot.position + Vector3.right * i * trayItemSpacing;
                GameObject instance = Instantiate(data.WorldPrefab, pos, Quaternion.identity, trayRoot);
                instance.name = $"TrayItem_{data.ItemId}";

                var draggable = instance.GetComponent<DraggablePlateItem>();
                if (draggable == null)
                    draggable = instance.AddComponent<DraggablePlateItem>();

                draggable.Initialise(data, pos, this);
                draggable.OnPickedUp += HandleItemPickedUp;
                draggable.OnDropped += HandleItemDropped;

                spawnedTrayItems.Add(draggable);
            }
        }

        public void ClearTray()
        {
            foreach (var item in spawnedTrayItems)
            {
                if (item != null)
                {
                    item.OnPickedUp -= HandleItemPickedUp;
                    item.OnDropped -= HandleItemDropped;
                    Destroy(item.gameObject);
                }
            }
            spawnedTrayItems.Clear();
        }

        public bool CanPlaceMore(string itemId)
        {
            PlateItemSO data = GetItemData(itemId);
            if (data == null) return false;
            int placed = placedCountByItemId.TryGetValue(itemId, out int c) ? c : 0;
            return placed < data.MaxOnPlate;
        }

        public void RegisterPlaced(string itemId)
        {
            if (!placedCountByItemId.ContainsKey(itemId))
                placedCountByItemId[itemId] = 0;
            placedCountByItemId[itemId]++;
        }

        public void UnregisterPlaced(string itemId)
        {
            if (placedCountByItemId.ContainsKey(itemId))
                placedCountByItemId[itemId] = Mathf.Max(0, placedCountByItemId[itemId] - 1);
        }

        public int GetPlacedCount(string itemId) =>
            placedCountByItemId.TryGetValue(itemId, out int c) ? c : 0;

        public void ResetPlacedCounts()
        {
            placedCountByItemId.Clear();
        }

        private PlateItemSO GetItemData(string itemId)
        {
            foreach (var item in availableItems)
                if (item != null && item.ItemId == itemId) return item;
            return null;
        }

        private void HandleItemPickedUp(DraggablePlateItem item) => OnItemPickedUp?.Invoke(item);
        private void HandleItemDropped(DraggablePlateItem item) => OnItemDropped?.Invoke(item);

        public void SetAvailableItems(List<PlateItemSO> items)
        {
            availableItems = items ?? new List<PlateItemSO>();
        }
    }
}
