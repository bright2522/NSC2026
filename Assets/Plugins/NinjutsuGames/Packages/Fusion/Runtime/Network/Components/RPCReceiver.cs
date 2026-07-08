using System;
using Fusion;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Serializable]
    public class RPCReceiver : NetworkBehaviour
    {
        [Networked, Capacity(12)] private NetworkDictionary<NetworkString<_32>, NetworkRpc> cachedRpcs => default;
        
        protected bool debug;
        
        // Ensures cached RPCs are replayed once per proxy after replication
        private bool _cachedRpcsReplayed;

        #region Gamecreator RPCs

        public enum RpcType { Trigger = 0, Actions = 1, Conditions = 2 }
        
        [Serializable]
        public struct NetworkRpc : INetworkStruct
        {
            public NetworkId NetworkId;
            public int Type;
        }

        public void TryRemoveCachedRpc(NetworkId networkId, RpcType type)
        {
            if (HasStateAuthority) RemoveCachedRpc(networkId, type);
            else RPC_RemoveCachedRpc(networkId, (int)type);
        }
        
        public void TryAddCachedRpc(NetworkId networkId, RpcType type)
        {
            if ( HasStateAuthority) AddCachedRpc(networkId, type);
            else RPC_AddCachedRpc(networkId, (int)type);
        }
        
        protected void RemoveCachedRpc(NetworkId networkId, RpcType type)
        {
            var id = $"{networkId}{(int)type}";
            if(cachedRpcs.ContainsKey(id)) cachedRpcs.Remove(id);
        }
        
        protected void AddCachedRpc(NetworkId networkId, RpcType type)
        {
            var id = $"{networkId}{(int)type}";
            var data = new NetworkRpc
            {
                NetworkId = networkId,
                Type = (int)type
            };
            cachedRpcs.Add(id, data);
        }

        public void RPC(RpcTargets target, NetworkId networkId, RpcType type, bool cacheState)
        {
            if (cacheState)
            {
                TryAddCachedRpc(networkId, type);
            }
            else
            {
                TryRemoveCachedRpc(networkId, type);
            }
            
            if (target == (RpcTargets.Proxies | RpcTargets.InputAuthority))
            {
                RPC_ProxiesInputAuth(networkId, (int)type);
            }
            else if (target == (RpcTargets.Proxies | RpcTargets.StateAuthority))
            {
                RPC_ProxiesStateAuth(networkId, (int)type);
            }
            else if (target == (RpcTargets.InputAuthority | RpcTargets.StateAuthority))
            {
                RPC_InputAuthStateAuth(networkId, (int)type);
            }
            else if (target == RpcTargets.Proxies)
            {
                RPC_Proxies(networkId, (int)type);
            }
            else if (target == RpcTargets.InputAuthority)
            {
                RPC_InputAuth(networkId, (int)type);
            }
            else if (target == RpcTargets.StateAuthority)
            {
                RPC_StateAuth(networkId, (int)type);
            }
            else
            {
                RPC_All(networkId, (int)type);
            }
        }

        [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable)]
        protected void RPC_AddCachedRpc(NetworkId networkId, int type)
        {
            AddCachedRpc(networkId, (RpcType)type);
        }
        
        [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable)]
        protected void RPC_RemoveCachedRpc(NetworkId networkId, int type)
        {
            RemoveCachedRpc(networkId, (RpcType)type);
        }

        [Rpc(RpcSources.All, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer, Channel = RpcChannel.Reliable)]
        protected void RPC_Target([RpcTarget] PlayerRef player, NetworkId networkId, int type, RpcInfo info = default)
        {
            if(debug) Debug.Log($"RPC_Target player: {player} networkId: {networkId} type: {type} auth: {HasStateAuthority} input: {HasInputAuthority} player: {Object.InputAuthority}");
            RunRpc(networkId, type, info.Source);
        }

        [Rpc(RpcSources.All, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer, Channel = RpcChannel.Reliable)]
        protected void RPC_All(NetworkId networkId, int type, RpcInfo info = default) => RunRpc(networkId, type, info.Source);
        
        [Rpc(RpcSources.All, RpcTargets.Proxies, HostMode = RpcHostMode.SourceIsHostPlayer, Channel = RpcChannel.Reliable)]
        protected void RPC_Proxies(NetworkId networkId, int type, RpcInfo info = default) => RunRpc(networkId, type, info.Source);
        
        [Rpc(RpcSources.All, RpcTargets.InputAuthority, HostMode = RpcHostMode.SourceIsHostPlayer, Channel = RpcChannel.Reliable)]
        protected void RPC_InputAuth(NetworkId networkId, int type, RpcInfo info = default) => RunRpc(networkId, type, info.Source);
        
        [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer, Channel = RpcChannel.Reliable)]
        protected void RPC_StateAuth(NetworkId networkId, int type, RpcInfo info = default) => RunRpc(networkId, type, info.Source);
        
        [Rpc(RpcSources.All, RpcTargets.Proxies | RpcTargets.InputAuthority, HostMode = RpcHostMode.SourceIsHostPlayer, Channel = RpcChannel.Reliable)]
        protected void RPC_ProxiesInputAuth(NetworkId networkId, int type, RpcInfo info = default) => RunRpc(networkId, type, info.Source);

        [Rpc(RpcSources.All, RpcTargets.Proxies | RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer, Channel = RpcChannel.Reliable)]
        protected void RPC_ProxiesStateAuth(NetworkId networkId, int type, RpcInfo info = default) => RunRpc(networkId, type, info.Source);

        [Rpc(RpcSources.All, RpcTargets.InputAuthority | RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer, Channel = RpcChannel.Reliable)]
        protected void RPC_InputAuthStateAuth(NetworkId networkId, int type, RpcInfo info = default) => RunRpc(networkId, type, info.Source);
        
        protected virtual void RunRpc(NetworkId networkId, int type, PlayerRef sender)
        {
            if(debug) Debug.Log($"Running RPC networkId: {networkId} type: {type} sender: {sender} hasInputAuthority: {HasInputAuthority} hasStateAuthority: {HasStateAuthority} isLocalPlayer: {sender == NetworkManager.Runner.LocalPlayer}");
            if (Runner.TryFindObject(networkId, out var obj))
            {
                var playerObject = PlayerManager.Instance.GetAvatar(sender);
                if(!playerObject)
                {
                    Debug.LogWarning($"Could not find player with id {sender}");
                    return;
                }
                var args = new Args(obj.gameObject, playerObject.gameObject);
                // Debug.Log($"#2 Running RPC with target: {args.Target} self: {args.Self}");
                switch (type)
                {
                    case 0:
                        var trigger = obj.Get<Trigger>();
                        if (!trigger)
                        {
                            Debug.LogWarning($"Could not find Trigger with id {networkId}");
                            return;
                        }
                        _ = trigger.Execute(args);
                        break;
                    case 1:
                        var actions = obj.Get<Actions>();
                        if (!actions)
                        {
                            Debug.LogWarning($"Could not find Actions with id {networkId}");
                            return;
                        }
                        _ = actions.Run(args);
                        break;
                    case 2:
                        var conditions = obj.Get<Conditions>();
                        if (!conditions)
                        {
                            Debug.LogWarning($"Could not find Conditions with id {networkId}");
                            return;
                        }
                        _ = conditions.Run(args);
                        break;
                }
            }
            else
            {
                Debug.LogWarning($"Could not find object with id {networkId}");
            }
        }
        
        [ContextMenu("Run Cached RPCs")]
        protected void RunCachedRpcs()
        {
            if(debug) Debug.Log($"Running cached RPCs: {cachedRpcs.Count} HasInputAuthority: {HasInputAuthority}");
            if (HasInputAuthority) return;
            if (!AreCachedRpcTargetsReady()) return;
            foreach (var cachedRpc in cachedRpcs)
            {
                RunRpc(cachedRpc.Value.NetworkId, cachedRpc.Value.Type, Object.InputAuthority);
            }
            _cachedRpcsReplayed = true;
        }

        protected void TryRunCachedRpcsOnce()
        {
            if (_cachedRpcsReplayed) return;
            if (HasInputAuthority) return; 
            if (cachedRpcs.Count == 0) return; 
            if (!AreCachedRpcTargetsReady()) return; 
            RunCachedRpcs();
        }

        /// <summary>
        /// Ensure all referenced objects exist locally before attempting cached replay
        /// </summary>
        /// <returns></returns>
        private bool AreCachedRpcTargetsReady()
        {
            if (!Runner) return false;
            foreach (var kv in cachedRpcs)
            {
                if (!Runner.TryFindObject(kv.Value.NetworkId, out _))
                {
                    return false;
                }
            }
            var sender = Object != null ? Object.InputAuthority : default;
            if (sender.IsNone) return true;
            var avatar = PlayerManager.Instance != null ? PlayerManager.Instance.GetAvatar(sender) : null;
            return avatar;
        }
        #endregion
    }
}