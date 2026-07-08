using System;
using Fusion;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Serializable]
    public class CompareShutdownReasonOrAny
    {
        private enum Option
        {
            Any = 0,
            Specific = 1
        }
        
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private Option m_Option = Option.Any;
        [SerializeField] private ShutdownReason m_ShutdownReason = ShutdownReason.Ok; 
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public bool Any => m_Option == Option.Any;

        // CONSTRUCTORS: --------------------------------------------------------------------------

        public CompareShutdownReasonOrAny()
        { }

        public CompareShutdownReasonOrAny(ShutdownReason shutdownReason) : this(false, shutdownReason)
        { }
        
        public CompareShutdownReasonOrAny(bool defaultAny, ShutdownReason shutdownReason) : this()
        {
            m_Option = defaultAny ? Option.Any : Option.Specific;
            m_ShutdownReason = shutdownReason;
        }
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public bool Match()
        {
            if (Any) return true;
            return NetworkManager.LastShutdownReason == m_ShutdownReason;
        }
    }
}