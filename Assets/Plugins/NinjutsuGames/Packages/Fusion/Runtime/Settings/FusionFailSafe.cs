using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Serializable]
    public class FusionFailSafe
    {
        [SerializeField] private bool m_Enabled = true;
        [SerializeField] private PropertyGetDecimal m_CloseErrorThreshold = GetDecimalInteger.Create(20);
        [SerializeField] private PropertyGetDecimal m_ShutdownErrorThreshold = GetDecimalInteger.Create(50);
        [SerializeField] private PropertyGetDecimal m_ErrorTimeWindow = GetDecimalDecimal.Create(60);

        // PROPERTIES: ----------------------------------------------------------------------------

        public bool Enabled => m_Enabled;
        public int CloseErrorThreshold => (int)m_CloseErrorThreshold.Get(Args.EMPTY);
        public int ShutdownErrorThreshold => (int)m_ShutdownErrorThreshold.Get(Args.EMPTY);
        public float ErrorTimeWindow => (float)m_ErrorTimeWindow.Get(Args.EMPTY);
    }
}