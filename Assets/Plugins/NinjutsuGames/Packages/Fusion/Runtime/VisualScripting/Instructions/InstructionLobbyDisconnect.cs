using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Shutdown Lobby")]
    [Description("Attempts to shutdown the current lobby session or cancel the connection process.")]

    [Category("Fusion/Lobby/Shutdown Lobby")]
    
    [Image(typeof(IconCancel), ColorTheme.Type.Red, typeof(OverlayListVariable))] 
    
    [Keywords("Shutdown", "Lobby", "Shutdown Lobby", "Fusion", "Disconnect", "Cancel", "Connection")]
    [Serializable]
    public class InstructionLobbyDisconnect : Instruction
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private bool m_WaitToComplete = true;
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Shutdown Lobby";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override async Task Run(Args args)
        {
            var task = NetworkManager.DisconnectLobbyAsync();
            if (m_WaitToComplete) await task;
        }
    }
}