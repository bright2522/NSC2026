using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Is Single Player")]
    [Description("Returns true if the Simulation is in Single Player mode.")]

    [Category("Fusion/Session/Is Single Player")]

    [Keywords("Fusion", "Is Single Player", "Single Player")]
    
    [Image(typeof(IconCharacter), ColorTheme.Type.Green)]
    
    [Serializable]
    public class ConditionIsSinglePlayer : Condition
    {
        // PROPERTIES: ----------------------------------------------------------------------------
        protected override string Summary => $"Is Single Player";
        
        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            return NetworkManager.Runner && NetworkManager.Runner.IsSinglePlayer;
        }
    }
}