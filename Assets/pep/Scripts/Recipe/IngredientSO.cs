using UnityEngine;

namespace Pep.Recipe
{
    [CreateAssetMenu(fileName = "Ingredient_", menuName = "PEP/Recipe/Ingredient SO")]
    public class IngredientSO : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private string category;
        [SerializeField] private int kcal;
        [SerializeField] private Sprite icon;
        [SerializeField] private bool isPourable;
        [SerializeField] private bool isFragile;
        [SerializeField] private float idealPourMin = 65f;
        [SerializeField] private float idealPourMax = 85f;
        [SerializeField] private float fragileMaxTiltVelocity = 2.5f;

        public string Id => id;
        public string DisplayName => displayName;
        public string Category => category;
        public int Kcal => kcal;
        public Sprite Icon => icon;
        public bool IsPourable => isPourable;
        public bool IsFragile => isFragile;
        public float IdealPourMin => idealPourMin;
        public float IdealPourMax => idealPourMax;
        public float FragileMaxTiltVelocity => fragileMaxTiltVelocity;
    }
}
