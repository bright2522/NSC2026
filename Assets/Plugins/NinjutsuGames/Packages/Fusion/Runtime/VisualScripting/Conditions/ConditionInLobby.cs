using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("In Lobby")]
    [Description("Returns true if peer is in a lobby session.")]

    [Category("Fusion/Lobby/In Lobby")]

    [Keywords("Fusion", "In Lobby", "Lobby")]
    
    [Image(typeof(IconChip), ColorTheme.Type.Blue)]
    
    [Serializable]
    public class ConditionInLobby : Condition
    {
        // PROPERTIES: ----------------------------------------------------------------------------
        protected override string Summary => $"In Lobby";
        
        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            return NetworkManager.RunnerLobby && NetworkManager.RunnerLobby.LobbyInfo.IsValid;
        }
    }
}