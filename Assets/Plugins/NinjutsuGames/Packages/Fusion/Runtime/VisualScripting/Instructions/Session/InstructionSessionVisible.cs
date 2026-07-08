using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Session Visible")]
    [Description("Signal if the current connected Session is visible. Only host, server or master client can change session visibility.")]

    [Category("Fusion/Session/Session Visible")]
    
    [Image(typeof(IconEye), ColorTheme.Type.Green)] 
    
    [Keywords("Session", "Game", "Visible", "Fusion")]
    [Serializable]
    public class InstructionSessionVisible : Instruction
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private PropertyGetBool m_Visible = GetBoolValue.Create(true);
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Session Visible <b>{m_Visible}</b>";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override Task Run(Args args)
        {
            NetworkManager.Runner.SessionInfo.IsVisible = m_Visible.Get(args);
            return DefaultResult;
        }
    }
}