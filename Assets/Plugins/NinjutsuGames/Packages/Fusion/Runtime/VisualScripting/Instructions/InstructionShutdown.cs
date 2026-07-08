using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Shutdown Game")]
    [Description("Attempts to shutdown the current game session or cancel the connection process.")]

    [Category("Fusion/Session/Shutdown Game")]
    
    [Image(typeof(IconCancel), ColorTheme.Type.Red)] 
    
    [Keywords("Shutdown", "Game", "Shutdown Game", "Fusion", "Disconnect", "Cancel", "Connection")]
    [Serializable]
    public class InstructionShutdown : Instruction
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private bool m_WaitToComplete = true;
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Shutdown Game";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override async Task Run(Args args)
        {
            var task = NetworkManager.DisconnectAsync();
            if (m_WaitToComplete) await task;
        }
    }
}