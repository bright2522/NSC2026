using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Session Open")]
    [Description("Signal if the current connected Session is open. Only host, server or master client can change this.")]

    [Category("Fusion/Session/Session Open")]
    
    [Image(typeof(IconFrame), ColorTheme.Type.Teal)] 
    
    [Keywords("Session", "Game", "Open", "Fusion")]
    [Serializable]
    public class InstructionSessionOpen : Instruction
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private PropertyGetBool m_Open = GetBoolValue.Create(true);
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Session Open <b>{m_Open}</b>";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override Task Run(Args args)
        {
            NetworkManager.Runner.SessionInfo.IsOpen = m_Open.Get(args);
            return DefaultResult;
        }
    }
}