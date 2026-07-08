using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Is In Session")]
    [Description("Returns true if the current session is in progress")]

    [Category("Fusion/Session/Is In Session")]

    [Keywords("Fusion", "Is In Session", "Session")]
    
    [Image(typeof(IconChip), ColorTheme.Type.Green, typeof(OverlayFlame))]
    
    [Serializable]
    public class ConditionIsInSession : Condition
    {
        // PROPERTIES: ----------------------------------------------------------------------------
        protected override string Summary => $"In Session";
        
        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            return NetworkManager.Runner && NetworkManager.Runner.IsInSession;
        }
    }
}