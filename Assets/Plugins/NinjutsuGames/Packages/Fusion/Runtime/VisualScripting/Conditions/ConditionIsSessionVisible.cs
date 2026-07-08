using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Is Session Visible")]
    [Description("Returns true if the current session is visible")]

    [Category("Fusion/Session/Is Session Visible")]

    [Keywords("Fusion", "Is Session", "Session", "Visible")]
    
    [Image(typeof(IconEye), ColorTheme.Type.Green)]
    
    [Serializable]
    public class ConditionIsSessionVisible : Condition
    {
        // PROPERTIES: ----------------------------------------------------------------------------
        protected override string Summary => $"Session Visible";
        
        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            return NetworkManager.Runner && NetworkManager.Runner.SessionInfo.IsVisible;
        }
    }
}