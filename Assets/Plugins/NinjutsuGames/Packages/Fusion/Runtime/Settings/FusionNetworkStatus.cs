using System;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Serializable]
    public class FusionNetworkStatus
    {
        [SerializeField] private ShutdownErrorList shutdownErrorList;
        public ShutdownErrorList ShutdownErrorList => shutdownErrorList;
    }
}
