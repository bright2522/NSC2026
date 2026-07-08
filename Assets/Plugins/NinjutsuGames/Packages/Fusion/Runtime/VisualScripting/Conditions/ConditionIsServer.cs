using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Is Server")]
    [Description("Returns true if this Simulation represents a Server connection.")]

    [Category("Fusion/Session/Is Server")]

    [Keywords("Fusion", "Is Server", "Server")]
    
    [Image(typeof(IconChip), ColorTheme.Type.Blue)]
    
    [Serializable]
    public class ConditionIsServer : Condition
    {
        // PROPERTIES: ----------------------------------------------------------------------------
        protected override string Summary => $"Is Server";
        
        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            return NetworkManager.Runner && NetworkManager.Runner.IsServer;
        }
    }
}