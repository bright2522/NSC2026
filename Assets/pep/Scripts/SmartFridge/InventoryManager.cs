using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pep.SmartFridge
{
    [Serializable]
    public class InventoryEntry
    {
        public string ingredientId;
        public int amount;

        public InventoryEntry(string ingredientId, int amount)
        {
            this.ingredientId = ingredientId;
            this.amount = amount;
        }
    }

    public class InventoryManager : MonoBehaviour
    {
        [SerializeField] private List<InventoryEntry> initialInventory = new List<InventoryEntry>();

        public event Action<string, int> OnIngredientChanged;
        public event Action OnInventorySynced;

        private readonly Dictionary<string, int> stockByIngredient = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        private void Awake()
        {
            foreach (var entry in initialInventory)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.ingredientId)) continue;
                stockByIngredient[entry.ingredientId] = Mathf.Max(0, entry.amount);
            }
        }

        public int GetAmount(string ingredientId)
        {
            if (string.IsNullOrWhiteSpace(ingredientId)) return 0;
            return stockByIngredient.TryGetValue(ingredientId, out int amount) ? amount : 0;
        }

        public bool HasIngredient(string ingredientId, int requiredAmount = 1)
        {
            return GetAmount(ingredientId) >= Mathf.Max(1, requiredAmount);
        }

        public void AddIngredient(string ingredientId, int amount = 1)
        {
            if (string.IsNullOrWhiteSpace(ingredientId) || amount <= 0) return;

            int current = GetAmount(ingredientId);
            int next = current + amount;
            stockByIngredient[ingredientId] = next;
            OnIngredientChanged?.Invoke(ingredientId, next);
        }

        public bool ConsumeIngredient(string ingredientId, int amount = 1)
        {
            amount = Mathf.Max(1, amount);
            int current = GetAmount(ingredientId);
            if (current < amount) return false;

            int next = current - amount;
            if (next == 0)
            {
                stockByIngredient.Remove(ingredientId);
            }
            else
            {
                stockByIngredient[ingredientId] = next;
            }

            OnIngredientChanged?.Invoke(ingredientId, next);
            return true;
        }

        public void ApplySelectionFromSmartFridge(List<string> selectedIngredientIds)
        {
            if (selectedIngredientIds == null) return;

            foreach (string ingredientId in selectedIngredientIds)
            {
                AddIngredient(ingredientId, 1);
            }

            OnInventorySynced?.Invoke();
        }

        public bool TryConsumeRecipeIngredients(List<string> requiredIngredientIds)
        {
            if (requiredIngredientIds == null || requiredIngredientIds.Count == 0) return true;

            var need = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (string id in requiredIngredientIds)
            {
                if (string.IsNullOrWhiteSpace(id)) continue;
                need[id] = need.TryGetValue(id, out int count) ? count + 1 : 1;
            }

            foreach (var pair in need)
            {
                if (!HasIngredient(pair.Key, pair.Value))
                {
                    return false;
                }
            }

            foreach (var pair in need)
            {
                ConsumeIngredient(pair.Key, pair.Value);
            }

            return true;
        }

        public List<InventoryEntry> GetSnapshot()
        {
            var snapshot = new List<InventoryEntry>();
            foreach (var pair in stockByIngredient)
            {
                snapshot.Add(new InventoryEntry(pair.Key, pair.Value));
            }
            return snapshot;
        }

        public List<string> GetIngredientIdList()
        {
            var list = new List<string>(stockByIngredient.Count);
            foreach (var pair in stockByIngredient)
            {
                if (pair.Value > 0) list.Add(pair.Key);
            }
            return list;
        }
    }
}
