using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pep.Core
{
    [Serializable]
    public class PlayerDataSnapshot
    {
        public string playerId = "guest";
        public string playerName = "Player";
        public int totalScore;
        public int level = 1;
        public List<string> ownedIngredients = new List<string>();
        public List<string> unlockedRecipes = new List<string>();
        public string lastUpdatedUtc;
    }

    public class PlayerDataManager : MonoBehaviour
    {
        public const string SaveKey = "PEP_PLAYER_DATA_V1";

        [SerializeField] private bool loadOnAwake = true;
        [SerializeField] private PlayerDataSnapshot currentData = new PlayerDataSnapshot();

        public event Action<PlayerDataSnapshot> OnDataLoaded;
        public event Action<PlayerDataSnapshot> OnDataSaved;

        public PlayerDataSnapshot CurrentData => currentData;

        private void Awake()
        {
            if (loadOnAwake)
            {
                Load();
            }
        }

        public void SetIdentity(string playerId, string playerName)
        {
            if (!string.IsNullOrWhiteSpace(playerId)) currentData.playerId = playerId;
            if (!string.IsNullOrWhiteSpace(playerName)) currentData.playerName = playerName;
        }

        public void AddScore(int scoreDelta)
        {
            currentData.totalScore = Mathf.Max(0, currentData.totalScore + scoreDelta);
        }

        public void SetLevel(int value)
        {
            currentData.level = Mathf.Max(1, value);
        }

        public void SetOwnedIngredients(List<string> ingredientIds)
        {
            currentData.ownedIngredients = ingredientIds ?? new List<string>();
        }

        public void SetUnlockedRecipes(List<string> recipeIds)
        {
            currentData.unlockedRecipes = recipeIds ?? new List<string>();
        }

        public bool HasIngredient(string ingredientId)
        {
            if (string.IsNullOrWhiteSpace(ingredientId)) return false;
            return currentData.ownedIngredients.Contains(ingredientId);
        }

        public void Save()
        {
            currentData.lastUpdatedUtc = DateTime.UtcNow.ToString("O");
            string json = JsonUtility.ToJson(currentData);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
            OnDataSaved?.Invoke(currentData);
        }

        public void Load()
        {
            if (!PlayerPrefs.HasKey(SaveKey))
            {
                currentData = new PlayerDataSnapshot();
                return;
            }

            string json = PlayerPrefs.GetString(SaveKey, string.Empty);
            if (string.IsNullOrWhiteSpace(json))
            {
                currentData = new PlayerDataSnapshot();
                return;
            }

            var loaded = JsonUtility.FromJson<PlayerDataSnapshot>(json);
            currentData = loaded ?? new PlayerDataSnapshot();

            if (currentData.ownedIngredients == null) currentData.ownedIngredients = new List<string>();
            if (currentData.unlockedRecipes == null) currentData.unlockedRecipes = new List<string>();

            OnDataLoaded?.Invoke(currentData);
        }

        public void ClearSave()
        {
            PlayerPrefs.DeleteKey(SaveKey);
            currentData = new PlayerDataSnapshot();
        }
    }
}
