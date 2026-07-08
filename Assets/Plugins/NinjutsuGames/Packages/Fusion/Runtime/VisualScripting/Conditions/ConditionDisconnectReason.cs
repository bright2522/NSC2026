using System;
using Fusion.Sockets;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Compare Disconnect Reason")]
    [Description("Returns true if the last disconnect reason matches the comparison.")]

    [Category("Fusion/Session/Compare Disconnect Reason")]

    [Keywords("Fusion", "Server", "Disconnect", "Reason")]
    
    [Image(typeof(IconDisconnected), ColorTheme.Type.Red)]
    
    [Serializable]
    public class ConditionDisconnectReason : Condition
    {
        private enum Comparison
        {
            Equals,
            Different
        }
        
        // MEMBERS: -------------------------------------------------------------------------------
        
        [SerializeField] private Comparison m_Comparison = Comparison.Equals;
        [SerializeField] private NetDisconnectReason m_CompareTo = NetDisconnectReason.Unknown;
        
        // PROPERTIES: ----------------------------------------------------------------------------
        
        protected override string Summary =>
            $"Disconnect Reason {m_Comparison switch { Comparison.Equals => "=", Comparison.Different => "≠", _ => string.Empty }} {m_CompareTo}";
        
        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            var a = NetworkManager.LastDisconnectReason;
            var b = m_CompareTo;

            return m_Comparison switch
            {
                Comparison.Equals => a == b,
                Comparison.Different => a != b,
                _ => throw new ArgumentOutOfRangeException($"DisconnectReason Comparison '{m_Comparison}' not found")
            };
        }
    }
}