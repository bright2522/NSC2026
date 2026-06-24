using System.Collections.Generic;

namespace Pep.Integration
{
    public interface IPepExternalFlowBridge
    {
        bool IsAvailable { get; }
        bool TryReadSelectedRecipe(out string recipeIdOrName);
        bool TryReadSelectedIngredients(out List<string> ingredientIdsOrNames);
        void NotifyPepStepCompleted(string stepName, float score);
        void NotifyPepFlowCompleted(float finalScore);
    }

    public class PepExternalFlowBridge : IPepExternalFlowBridge
    {
        public bool IsAvailable => false;

        public bool TryReadSelectedRecipe(out string recipeIdOrName)
        {
            recipeIdOrName = string.Empty;
            return false;
        }

        public bool TryReadSelectedIngredients(out List<string> ingredientIdsOrNames)
        {
            ingredientIdsOrNames = null;
            return false;
        }

        public void NotifyPepStepCompleted(string stepName, float score)
        {
        }

        public void NotifyPepFlowCompleted(float finalScore)
        {
        }
    }
}
