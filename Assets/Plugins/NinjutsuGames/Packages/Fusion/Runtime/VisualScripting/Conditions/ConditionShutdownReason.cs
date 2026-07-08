using System;
using Fusion;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Compare Shutdown Reason")]
    [Description("Returns true if the last shutdown reason matches the comparison.")]

    [Category("Fusion/Session/Compare Shutdown Reason")]

    [Keywords("Fusion", "Shutdown", "Server", "Reason")]
    
    [Image(typeof(IconShutdown), ColorTheme.Type.Red)]
    
    [Serializable]
    public class ConditionShutdownReason : Condition
    {
        private enum Comparison
        {
            Equals,
            Different
        }
        
        // MEMBERS: -------------------------------------------------------------------------------
        
        [SerializeField] private Comparison m_Comparison = Comparison.Equals;
        [SerializeField] private ShutdownReason m_CompareTo = ShutdownReason.Ok;
        
        // PROPERTIES: ----------------------------------------------------------------------------
        
        protected override string Summary =>
            $"Shutdown Reason {m_Comparison switch { Comparison.Equals => "=", Comparison.Different => "≠", _ => string.Empty }} {m_CompareTo}";
        
        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            var a = NetworkManager.LastShutdownReason;
            var b = m_CompareTo;

            return m_Comparison switch
            {
                Comparison.Equals => a == b,
                Comparison.Different => a != b,
                _ => throw new ArgumentOutOfRangeException($"ShutdownReason Comparison '{m_Comparison}' not found")
            };
        }
    }
}