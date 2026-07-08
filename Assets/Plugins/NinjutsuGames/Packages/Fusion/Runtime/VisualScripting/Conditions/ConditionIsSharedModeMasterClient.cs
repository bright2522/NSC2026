using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Is Shared Mode Master Client")]
    [Description("Signal if the Local Peer is in a Room and is the Room Master Client.")]

    [Category("Fusion/Session/Is Shared Mode Master Client")]

    [Keywords("Fusion", "Is Server", "Server")]
    
    [Image(typeof(IconCharacter), ColorTheme.Type.Purple)]
    
    [Serializable]
    public class ConditionIsSharedModeMasterClient : Condition
    {
        // PROPERTIES: ----------------------------------------------------------------------------
        protected override string Summary => $"Is Shared Mode Master Client";
        
        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            return NetworkManager.IsConnected && NetworkManager.Runner.IsSharedModeMasterClient;
        }
    }
}