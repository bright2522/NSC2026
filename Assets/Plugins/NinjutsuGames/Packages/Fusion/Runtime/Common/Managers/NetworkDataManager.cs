using System;
using Fusion;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [DefaultExecutionOrder(-1000)]
    public class NetworkDataManager : NetworkBehaviour
    {
        public static NetworkDataManager Instance { get; private set; }
        public static event Action EventSpawned;
        [Networked, OnChangedRender(nameof(OnSeedChanged))] public int RandomSeed { get; set; }

        public bool IsSpawned => Object && Object.IsValid;

        [Networked]
        [Capacity(20)]
        [UnitySerializeField]
        public NetworkDictionary<NetworkId, CustomTickTimer> Timers => default;
        
        public static event Action<NetworkId> EventOnTickTimerExpired;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void OnSubsystemsInit()
        {
            Instance = null;
        }

        private void Awake()
        {
            Instance = this;
        }
        
        private void Cleanup()
        {
        }

        public override void Spawned()
        {
            EventSpawned?.Invoke();
            InitSeed();
            Cleanup();
        }
        
        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            Cleanup();
        }

        public override void FixedUpdateNetwork()
        {
            if(!Runner.IsInSession) return;
            foreach (var timer in Timers)
            {
                if(timer.Value.Expired(Runner))
                {
                    RPC_TimerExpired(timer.Key);
                    Timers.Remove(timer.Key);
                }
            }
        }

        #region Tick Timer
        
        [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable)]
        private void RPC_TimerExpired(NetworkId networkId)
        {
            EventOnTickTimerExpired?.Invoke(networkId);
        }

        public CustomTickTimer GetTimer(NetworkObject networkObject)
        {
            // Check if spawned before accessing networked properties
            if (!IsSpawned)
            {
                return default;
            }
            
            return !Timers.ContainsKey(networkObject.Id) ? default : Timers[networkObject.Id];
        }
        
        public CustomTickTimer GetTimer(NetworkObject networkObject, float seconds, bool createFromTicks)
        {
            if (!IsSpawned)
            {
                return default;
            }
            
            if(!Runner.IsInSession) return default;

            if(!networkObject) networkObject = Object;
            var runner = NetworkRunner.GetRunnerForGameObject(networkObject.gameObject);
            
            if(Timers.ContainsKey(networkObject.Id))
            {
                var timer = Timers[networkObject.Id];
                if (timer.ExpiredOrNotRunning(runner))
                {
                    Timers.Remove(networkObject.Id);
                }
                else return timer;
            }
            
            var newTimer = createFromTicks ? CustomTickTimer.CreateFromTicks(runner, (int)seconds) : CustomTickTimer.CreateFromSeconds(runner, seconds);
            Timers.Add(networkObject.Id, newTimer);
            return newTimer;
        }

        public void RemoveTimer(NetworkId networkId)
        {
            if(!Runner) return;
            if(!Runner.IsInSession) return;
            if (!IsSpawned)
            {
                Debug.LogWarning("[NetworkDataManager] Cannot access Timers - NetworkBehaviour not spawned yet");
                return;
            }

            Timers.Remove(networkId);
        }

        #endregion

        #region Random Seed

        private void InitSeed()
        {
            if(HasStateAuthority | Runner.IsSharedModeMasterClient) RandomSeed = UnityEngine.Random.Range(10000, 99999);
            UnityEngine.Random.InitState(RandomSeed);
        }

        private void OnSeedChanged()
        {
            UnityEngine.Random.InitState(RandomSeed);
        }

        #endregion

        public static void RPC(RpcTargets target, NetworkId networkObjectId, RPCReceiver.RpcType rpcType, bool cache)
        {
            if(NetworkCharacter.LocalPlayer)
            {
                NetworkCharacter.LocalPlayer.RPC(target, networkObjectId, rpcType, cache);
            }
            else if(NetworkPlayer.LocalPlayer)
            {
                NetworkPlayer.LocalPlayer.RPC(target, networkObjectId, rpcType, cache);
            }
        }

        public static void TryRemoveCachedRpc(NetworkId networkObjectId, RPCReceiver.RpcType rpcType)
        {
            if(NetworkCharacter.LocalPlayer)
            {
                NetworkCharacter.LocalPlayer.TryRemoveCachedRpc(networkObjectId, rpcType);
            }
            else if(NetworkPlayer.LocalPlayer)
            {
                NetworkPlayer.LocalPlayer.TryRemoveCachedRpc(networkObjectId, rpcType);
            }
        }
    }
}