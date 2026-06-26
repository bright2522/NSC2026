using UnityEngine;
using Pep.Recipe;

namespace Pep.Minigames.Plating
{
    [CreateAssetMenu(fileName = "PlateItem_", menuName = "PEP/Plating/Plate Item SO")]
    public class PlateItemSO : ScriptableObject
    {
        [SerializeField] private string itemId;
        [SerializeField] private string displayName;
        [SerializeField] private IngredientSO ingredient;
        [SerializeField] private GameObject worldPrefab;
        [SerializeField] private Sprite previewIcon;
        [SerializeField] private int maxOnPlate = 1;

        public string ItemId => itemId;
        public string DisplayName => displayName;
        public IngredientSO Ingredient => ingredient;
        public GameObject WorldPrefab => worldPrefab;
        public Sprite PreviewIcon => previewIcon;
        public int MaxOnPlate => maxOnPlate;

        public string LinkedIngredientId => ingredient != null ? ingredient.Id : itemId;
    }
}
