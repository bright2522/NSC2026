using System;
using Fusion.Sockets;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Serializable]
    public class CompareDisconnectReasonOrAny
    {
        private enum Option
        {
            Any = 0,
            Specific = 1
        }
        
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private Option m_Option = Option.Any;
        [SerializeField] private NetDisconnectReason m_DisconnectReason = NetDisconnectReason.Unknown;
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public bool Any => m_Option == Option.Any;

        // CONSTRUCTORS: --------------------------------------------------------------------------

        public CompareDisconnectReasonOrAny()
        { }

        public CompareDisconnectReasonOrAny(NetDisconnectReason disconnectReason) : this(false, disconnectReason)
        { }
        
        public CompareDisconnectReasonOrAny(bool defaultAny, NetDisconnectReason disconnectReason) : this()
        {
            m_Option = defaultAny ? Option.Any : Option.Specific;
            m_DisconnectReason = disconnectReason;
        }
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public bool Match()
        {
            if (Any) return true;
            return NetworkManager.LastDisconnectReason == m_DisconnectReason;
        }
    }
}