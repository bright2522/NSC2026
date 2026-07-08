using Fusion;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    public class NetworkObjectDespawned : NetworkObjectBase, IDespawned
    {
        public void Despawned(NetworkRunner runner, bool hasState)
        {
            TryRunTriggers();
        }
    }
}