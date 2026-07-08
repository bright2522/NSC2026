using System;
using Fusion;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    public class NetworkPlayer : RPCReceiver, IInputAuthorityGained, IStateAuthorityChanged
    {
        private const string NameFormat = "{0}_Data";
        
        public static NetworkPlayer LocalPlayer { get; private set; }
        
        public bool HasSpawned { get; private set; }
        
        [Networked] public NetworkString<_32> Username { get; set; }
        [Networked] public int Token { get; set; }
        [Networked] public double NetworkPing { get; set; }
        public int Ping => (int)(NetworkPing * 1000);
        [field:NonSerialized] public static string CachedUsername { get; set; }
        
        private ChangeDetector _changeDetector;
        public static event Action<NetworkPlayer> EventPlayerSpawned;
        public static event Action<NetworkPlayer> EventPlayerDespawned;
        public static event Action<NetworkPlayer> EventUsernameChanged;

        public override void Spawned()
        {
            _changeDetector = GetChangeDetector(ChangeDetector.Source.SnapshotTo);

#if UNITY_EDITOR
            FormatName();
#endif
            if (Object.InputAuthority == Runner.LocalPlayer)
            {
                LocalPlayer = this;
                CachedUsername = NetworkManager.ConnectionArgs.UserName;
                if (!string.IsNullOrEmpty(CachedUsername))
                {
                    Username = CachedUsername;
                    CachedUsername = null;
                }
            }
            HasSpawned = true;
            PlayerManager.LastJoinedPlayerData = this;
            // Debug.Log($"[NetworkPlayer] Spawned isResume: {Runner.IsResume} player: {Object.InputAuthority} total: {Players.Count}");
            if(!Object.InputAuthority.IsNone) EventPlayerSpawned?.Invoke(this);
        }

        public override void FixedUpdateNetwork()
        {
            if (HasStateAuthority)
            {
                NetworkPing = Runner.GetPlayerRtt(Object.InputAuthority);
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            HasSpawned = false;
            PlayerManager.LastLeftPlayerData = this;
            EventPlayerDespawned?.Invoke(this);
        }

        public override void Render()
        {
            foreach (var change in _changeDetector.DetectChanges(this))
            {
                if (change == nameof(Username))
                {
                    EventUsernameChanged?.Invoke(this);
                }
            }
        }

#if UNITY_EDITOR
        private void FormatName()
        {
            gameObject.name = string.Format(NameFormat, Object.InputAuthority.ToString());
        }
#endif
        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_SetUsername(string value)
        {
            Username = value;
        }
        public void SetUsername(string value)
        {
            if (Runner.Topology == Topologies.ClientServer && !HasStateAuthority)
            {
                RPC_SetUsername(value);
            }
            else
            {
                if (!HasSpawned)
                {
                    CachedUsername = value;
                    return;
                }
                Username = value;
                if(!string.IsNullOrEmpty(CachedUsername)) CachedUsername = null;
            }
        }
        
        public void InputAuthorityGained()
        {
            Debug.LogWarning($"Player InputAuthorityGained: {Object.InputAuthority} is Resume: {Runner.IsResume}", gameObject);

            if (Object.InputAuthority == Runner.LocalPlayer)
            {
                LocalPlayer = this;
                if (!string.IsNullOrEmpty(CachedUsername))
                {
                    Username = CachedUsername;
                    CachedUsername = null;
                }
            }
            
#if UNITY_EDITOR
            FormatName();
#endif
            EventPlayerSpawned?.Invoke(this);
        }

        public void StateAuthorityChanged()
        {
            Debug.LogWarning($"Player StateAuthorityChanged: {Object.StateAuthority}", gameObject);
        }
        
        protected override void RunRpc(NetworkId networkId, int type, PlayerRef sender)
        {
            if(debug) Debug.Log($"Running RPC networkId: {networkId} type: {type} sender: {sender} hasInputAuthority: {HasInputAuthority} hasStateAuthority: {HasStateAuthority} isLocalPlayer: {sender == NetworkManager.Runner.LocalPlayer}");
            if (Runner.TryFindObject(networkId, out var obj))
            {
                var playerObject = PlayerManager.Instance.GetPlayer(sender);
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
    }
}