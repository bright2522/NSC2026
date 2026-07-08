using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Is Player")]
    [Description("Returns true if this runner represents a Client or Host. Dedicated servers have no local player and will return false.")]

    [Category("Fusion/Session/Is Player")]

    [Keywords("Fusion", "Is Player", "Player", "Local Player")]
    
    [Image(typeof(IconCharacter), ColorTheme.Type.Green)]
    
    [Serializable]
    public class ConditionIsPlayer : Condition
    {
        // PROPERTIES: ----------------------------------------------------------------------------
        protected override string Summary => $"Is Player";
        
        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            return NetworkManager.Runner && NetworkManager.Runner.IsPlayer;
        }
    }
}