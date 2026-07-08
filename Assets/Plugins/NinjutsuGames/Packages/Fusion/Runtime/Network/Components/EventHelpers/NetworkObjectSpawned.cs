using Fusion;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    public class NetworkObjectSpawned : NetworkObjectBase, ISpawned
    {
        public void Spawned()
        {
            TryRunTriggers();
        }
    }
}