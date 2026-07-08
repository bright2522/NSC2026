using System;
using Fusion;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Serializable]
    public class EventAuthorityChanged : NetworkBehaviour, IStateAuthorityChanged
    {
        public event Action EventOnStateAuthorityChanged;
        
        public void StateAuthorityChanged()
        {
            EventOnStateAuthorityChanged?.Invoke();
        }
    }
}