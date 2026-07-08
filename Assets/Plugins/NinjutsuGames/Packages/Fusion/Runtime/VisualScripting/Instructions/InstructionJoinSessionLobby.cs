using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Join Lobby Session")]
    [Description("Join a Session Lobby")]

    [Category("Fusion/Session/Join Lobby Session")]
    
    [Parameter("Session Lobby", " Lobby Type to Join")]
    [Parameter("Lobby Id", "The name of the session to join or create")]
    
    [Image(typeof(IconHome), ColorTheme.Type.Blue)] 
    
    [Keywords("Start", "Lobby", "Start Lobby", "Fusion")]
    [Serializable]
    public class InstructionJoinSessionLobby : Instruction
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private JoinLobbySettings lobbySettings = new();
        [SerializeField] private RegionSettings regionSettings = new();
        [SerializeField] private LobbyAdvancedSettings advancedSettings = new();
        [SerializeField] private AuthenticationSettings authenticationSettings = new();
        [SerializeField, Space(4)] private bool m_WaitToFinish = true;
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Join Session Lobby: {lobbySettings.sessionLobby}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override async Task Run(Args args)
        {
            var task = NetworkManager.JoinLobbyAsync(args, regionSettings, lobbySettings, advancedSettings, authenticationSettings);
            if (m_WaitToFinish)
            {
                await task;
            }
        }
    }
}