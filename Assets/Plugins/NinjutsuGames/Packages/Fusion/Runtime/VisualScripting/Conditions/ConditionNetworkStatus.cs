using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Compare Network Status")]
    [Description("Returns true if the last network status matches the comparison.")]

    [Category("Fusion/Session/Compare Network Status")]

    [Keywords("Fusion", "Server", "Network", "Status")]
    
    [Image(typeof(IconCircleOutline), ColorTheme.Type.Green)]
    
    [Serializable]
    public class ConditionNetworkStatus : Condition
    {
        private enum Comparison
        {
            Equals,
            Different
        }
        
        // MEMBERS: -------------------------------------------------------------------------------
        
        [SerializeField] private Comparison m_Comparison = Comparison.Equals;
        [SerializeField] private NetworkManager.Status m_CompareTo = NetworkManager.Status.JoiningSession;
        
        // PROPERTIES: ----------------------------------------------------------------------------
        
        protected override string Summary =>
            $"Network Status {m_Comparison switch { Comparison.Equals => "=", Comparison.Different => "≠", _ => string.Empty }} {m_CompareTo}";
        
        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            var a = NetworkManager.NetworkStatus;
            var b = m_CompareTo;

            return m_Comparison switch
            {
                Comparison.Equals => a == b,
                Comparison.Different => a != b,
                _ => throw new ArgumentOutOfRangeException($"Network Status Comparison '{m_Comparison}' not found")
            };
        }
    }
}