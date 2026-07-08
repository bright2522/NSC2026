using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Is Session Open")]
    [Description("Returns true if the current session is open")]

    [Category("Fusion/Session/Is Session Open")]

    [Keywords("Fusion", "Is Session", "Session", "Open")]
    
    [Image(typeof(IconFrame), ColorTheme.Type.Teal)]
    
    [Serializable]
    public class ConditionIsSessionOpen : Condition
    {
        // PROPERTIES: ----------------------------------------------------------------------------
        protected override string Summary => $"Session Open";
        
        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            return NetworkManager.Runner && NetworkManager.Runner.SessionInfo.IsOpen;
        }
    }
}