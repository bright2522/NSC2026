using System;
using System.Collections.Generic;
using Fusion;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [DefaultExecutionOrder(-1000)]
    public class PlayerManager : NetworkBehaviour, IPlayerLeft, IPlayerJoined
    {
        public static PlayerManager Instance { get; private set; }
        public static AvatarSpawnData AvatarSpawnData { get; set; }
        public static NetworkCharacter LastJoinedPlayer { get; set; }
        public static NetworkPlayer LastJoinedPlayerData { get; set; }
        
        public static NetworkCharacter LastLeftPlayer { get; set; }
        public static NetworkPlayer LastLeftPlayerData { get; set; }
        
        public static Dictionary<PlayerRef, NetworkCharacter> Avatars = new();
        public static Dictionary<PlayerRef, NetworkPlayer> Players = new();

        private readonly Dictionary<int, NetworkObject> _mapPlayerToken = new();
        private readonly Dictionary<int, NetworkObject> _mapAvatarToken = new();
        
        // Pending Players to Join
        private List<int> _pendingTokens;

        private bool _spawned;
        private bool _avatarSpawned;
        private float _spawnDelay;
        
        private const int InvalidToken = 0;
        private string _defaultName;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void OnSubsystemsInit()
        {
            Avatars = new Dictionary<PlayerRef, NetworkCharacter>();
            Players = new Dictionary<PlayerRef, NetworkPlayer>();
            AvatarSpawnData = null;
            LastJoinedPlayer = null;
            Instance = null;
        }

        private void Awake()
        {
            Instance = this;
            _defaultName = FusionRepository.Get.Settings.defaultPlayerName;
            _spawnDelay = Time.time + 0.2f;
            _pendingTokens = new List<int>(); // Initialize _pendingTokens
            
            NetworkCharacter.EventAvatarSpawned += AvatarSpawned;
            NetworkCharacter.EventAvatarDespawned += AvatarDespawned;

            NetworkPlayer.EventPlayerSpawned += PlayerSpawned;
            NetworkPlayer.EventPlayerDespawned += PlayerDespawned;
        }
        
        private void OnDestroy()
        {
            NetworkCharacter.EventAvatarSpawned -= AvatarSpawned;
            NetworkCharacter.EventAvatarDespawned -= AvatarDespawned;
            
            NetworkPlayer.EventPlayerSpawned -= PlayerSpawned;
            NetworkPlayer.EventPlayerDespawned -= PlayerDespawned;
        }

        private void PlayerSpawned(NetworkPlayer obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj), "NetworkPlayer instance cannot be null.");

            var playerRef = obj.Object.InputAuthority; 

            Players.TryAdd(playerRef, obj);
        }

        private void PlayerDespawned(NetworkPlayer obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj), "NetworkPlayer instance cannot be null.");

            var playerRef = obj.Object.InputAuthority; 
            Players.Remove(playerRef);
        }

        private void AvatarSpawned(NetworkCharacter obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj), "NetworkAvatar instance cannot be null.");

            var playerRef = obj.Object.InputAuthority; 

            Avatars.TryAdd(playerRef, obj);
        }

        private void AvatarDespawned(NetworkCharacter obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj), "NetworkAvatar instance cannot be null.");

            var playerRef = obj.Object.InputAuthority; 
            if(_avatarSpawned && playerRef == Runner.LocalPlayer)
            {
                _avatarSpawned = false;
            }
            Avatars.Remove(playerRef);
        }

        public override void Spawned()
        {
            _spawned = true;
            _avatarSpawned = false;
            Cleanup();
            
            TrySpawnPlayer();
            TrySpawnAvatar();
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            _spawned = false;
            _avatarSpawned = false;
            Cleanup();
        }

        private void LateUpdate()
        {
            if (_spawned && !_avatarSpawned && AvatarSpawnData is { prefabId: { IsValid: true } })
            {
                if(_spawnDelay < Time.time) TrySpawnAvatar();
            }
        }

        // Handles the event when a player leaves the game.
        public void PlayerLeft(PlayerRef player)
        {
            DespawnAvatar(player);
            DespawnPlayer(player);
        }
        
        private bool IsPlayerTokenExists(int playerToken, PlayerRef inputAuthority, Dictionary<int, NetworkObject> tokenMap)
        {
            if (!tokenMap.TryGetValue(playerToken, out var player)) return false;
            if(player.InputAuthority == inputAuthority) return true;
            player.AssignInputAuthority(inputAuthority);
            player.Get<NetworkPlayer>().InputAuthorityGained();
            player.Get<NetworkCharacter>().InputAuthorityGained();
            // Avatars.TryAdd(inputAuthority, this);

            return false;
        }
        
        public void PlayerJoined(PlayerRef inputAuthority)
        {
            // Get the player token
           var playerToken = GetPlayerToken(Runner, inputAuthority);
           
           // Check if the player token already exists
           if(IsPlayerTokenExists(playerToken, inputAuthority, _mapPlayerToken) || 
              IsPlayerTokenExists(playerToken, inputAuthority, _mapAvatarToken)) return;
           
           // Safely remove from pending tokens if initialized
           _pendingTokens?.Remove(playerToken);
        }

        #region Avatar
        
        private void TrySpawnAvatar()
        {
            if(Runner.Mode == SimulationModes.Server) return;
            if(Runner.IsResume) return;
            
            if (AvatarSpawnData == null || AvatarSpawnData.prefabId.IsValid == false)
            {
                // If AvatarSpawnData is null or invalid, we cannot spawn the avatar
                // Debug.LogWarning("[PlayerSpawner] AvatarSpawnData is null or invalid");
                return;
            }
            
            _avatarSpawned = true;

            if (Runner.Topology == Topologies.Shared)
            {
                SpawnAvatar(AvatarSpawnData.prefabId, Runner.LocalPlayer, AvatarSpawnData.position,
                    AvatarSpawnData.rotation);
            }
            else
            {
                RPC_RequestAvatarSpawn(AvatarSpawnData.prefabId, Runner.LocalPlayer, AvatarSpawnData.position,
                    AvatarSpawnData.rotation);
            }
            AvatarSpawnData = null;
        }

        private bool GetPlayerAvatar(PlayerRef player, out NetworkCharacter character)
        {
            // Try to get the avatar from the dictionary using the player reference as the key
            return Avatars.TryGetValue(player, out character);
        }
        
        public NetworkCharacter GetAvatar(PlayerRef inputAuthority)
        {
            return Avatars.GetValueOrDefault(inputAuthority);
        }
        
        /// <summary>
        /// Returns the avatar of the player by index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public NetworkCharacter GetPlayerAvatarByIndex(int index)
        {
            var en = Runner.ActivePlayers;
            var num = -1;
            foreach (var playerRef in en)
            {
                num++;
                if (num == index)
                {
                    return GetAvatar(playerRef);
                }
            }
            return null;
        }

        // Sends a request to the state authority to spawn an avatar for the joining player.
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_RequestAvatarSpawn(NetworkPrefabId prefabId, PlayerRef inputAuthority, Vector3 position,
            Quaternion rotation)
        {
            // Call the SpawnPlayerPrefab method to spawn the avatar
            SpawnAvatar(prefabId, inputAuthority, position, rotation);
        }

        private async void SpawnAvatar(NetworkPrefabId prefabId, PlayerRef inputAuthority, Vector3 position, Quaternion rotation)
        {
            try
            {
                if(prefabId.IsValid == false)
                {
                    throw new Exception("[SpawnAvatar] Invalid prefabId");
                }
            
                // Get the player token
                var playerToken = GetPlayerToken(Runner, inputAuthority);
                var spawnOp = Runner.SpawnAsync(prefabId, position, rotation, inputAuthority, (r, o) =>
                {
                    var avatar = o.GetBehaviour<NetworkCharacter>();
                    avatar.Token = playerToken;
                    if(!string.IsNullOrEmpty(NetworkManager.ConnectionArgs.SelectedModel))
                    {
                        avatar.SetCurrentModel(NetworkManager.ConnectionArgs.SelectedModel);
                    }
                    o.transform.SetPositionAndRotation(position, rotation);
                    Physics.SyncTransforms();

                });
                await spawnOp;

                var networkObject = spawnOp.Object;

                if (!Runner) return;
                if (!networkObject) return;
                if (!inputAuthority.IsRealPlayer) return;
                
                // Additional validation before setting player object
                if (!Runner.IsRunning)
                {
                    Debug.LogWarning("[PlayerManager] Runner is not running, cannot set player object");
                    return;
                }
                
                try
                {
                    Runner.SetPlayerObject(inputAuthority, networkObject);
                    _mapAvatarToken[playerToken] = networkObject;
                }
                catch (Exception setPlayerException)
                {
                    Debug.LogError($"[PlayerManager] Failed to set player object: {setPlayerException.Message}");
                    throw;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Fusion] Failed to spawn Player: {e.Message} stackTrace: {e.StackTrace}");
            }
        }
        
        private void DespawnAvatar(PlayerRef player)
        {
            _mapAvatarToken.Remove(GetPlayerToken(Runner, player));

            // Get the avatar of the player who left and despawn it if valid
            // This is to clean up any resources associated with the player
            if (!GetPlayerAvatar(player, out var avatar)) return;
            
            // Only run on state authority
            if (HasStateAuthority) Runner.Despawn(avatar.Object);
        }

        #endregion


        #region Player
        
        private void TrySpawnPlayer()
        {
            if(Runner.Mode == SimulationModes.Server) return;
            if(Runner.IsResume) return;
            if(Runner.LocalPlayer.IsNone) return;
            
            var userName = NetworkManager.ConnectionArgs.UserName;
            if(string.IsNullOrEmpty(userName)) userName = string.Format(_defaultName, Runner.LocalPlayer.PlayerId);
            
            if (Runner.Topology == Topologies.Shared)
            {
                SpawnPlayer(Runner.LocalPlayer, userName);
            }
            else
            {
                RPC_RequestPlayerSpawn(Runner.LocalPlayer, userName);
            }
        }

        // Sends a request to the state authority to spawn an avatar for the joining player.
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_RequestPlayerSpawn(PlayerRef inputAuthority, string username)
        {
            // Call the SpawnPlayerPrefab method to spawn the avatar
            SpawnPlayer(inputAuthority, username);
        }
        
        private void SpawnPlayer(PlayerRef inputAuthority, string username)
        {
            // Get the player token
            var playerToken = GetPlayerToken(Runner, inputAuthority);
            Runner.SpawnAsync(NetworkPrefabId.FromRaw(PooledNetworkObjectProvider.PLAYER_DATA), Vector3.zero, Quaternion.identity, inputAuthority, (r, o) =>
            {
                var player = o.GetBehaviour<NetworkPlayer>();
                player.Username = username;
                player.Token = playerToken;
            }, NetworkSpawnFlags.DontDestroyOnLoad, onCompleted: result =>
            {
                if(!result.Object) return;
                _mapPlayerToken[playerToken] = result.Object;
            });
        }
        
        private bool GetPlayer(PlayerRef player, out NetworkPlayer avatar)
        {
            // Try to get the avatar from the dictionary using the player reference as the key
            return Players.TryGetValue(player, out avatar);
        }

        public NetworkPlayer GetPlayer(PlayerRef inputAuthority)
        {
            return Players.GetValueOrDefault(inputAuthority);
        }
        
        private void DespawnPlayer(PlayerRef player)
        {
            _mapPlayerToken.Remove(GetPlayerToken(Runner, player));
            Players.Remove(player);

            // Get the avatar of the player who left and despawn it if valid
            // This is to clean up any resources associated with the player
            if (!GetPlayer(player, out var avatar)) return;
            
            // Only run on state authority
            if (HasStateAuthority) Runner.Despawn(avatar.Object);
        }
        #endregion

        /// <summary>
        /// Get the Connection Token hash associated with a particular PlayerRef
        /// </summary>
        /// <param name="runner">NetworkRunner to check for the Connection Token</param>
        /// <param name="player">PlayerRef to get the associated token</param>
        /// <returns>Connection Token Hash, or 0 if not found or invalid</returns>
        public static int GetPlayerToken(NetworkRunner runner, PlayerRef player)
        {
            if (runner.LocalPlayer == player)
            {
                // Just use the local Player Connection Token
                return ConnectionTokenUtils.HashToken(NetworkManager.ConnectionToken);
            }

            // Get the Connection Token stored when the Client connects to this Host
            var token = runner.GetPlayerConnectionToken(player);

            if (token != null)
            {
                return ConnectionTokenUtils.HashToken(token);
            }
            
            // Debug.LogWarning($"GetPlayerToken: No token found for {player} in {runner}");
                
            return InvalidToken; // invalid
        }

        public void SetAvatarConnectionToken(int token, NetworkObject avatar)
        {
            _mapAvatarToken[token] = avatar;
            _pendingTokens?.Add(avatar.Get<NetworkCharacter>().Token);

        }
        
        public void SetPlayerConnectionToken(int token, NetworkObject player)
        {
            _mapPlayerToken[token] = player;
            _pendingTokens?.Add(player.Get<NetworkPlayer>().Token);
        }

        public void CleanupAfterHostMigration()
        {
            if(!HasStateAuthority) return;

            if (_pendingTokens != null)
            {
                lock (_pendingTokens)
                {
                    foreach (var token in _pendingTokens)
                    {
                        DespawnIfNoInputAuthority(_mapAvatarToken, token);
                        DespawnIfNoInputAuthority(_mapPlayerToken, token);
                    }

                    _pendingTokens.Clear();
                }
            }
            
            // Cleanup the Avatars and Players look for any invalid entries or duplicates
            foreach (var avatar in Avatars)
            {
                if (avatar.Value == null)
                {
                    Avatars.Remove(avatar.Key);
                    continue;
                }

                if (avatar.Value.Object.InputAuthority.IsNone)
                {
                    Avatars.Remove(avatar.Key);
                }
            }
            
            foreach (var player in Players)
            {
                if (player.Value == null)
                {
                    Players.Remove(player.Key);
                    continue;
                }

                if (player.Value.Object.InputAuthority.IsNone)
                {
                    Players.Remove(player.Key);
                }
            }
        }
        
        private void DespawnIfNoInputAuthority(Dictionary<int, NetworkObject> map, int token)
        {
            if (!map.TryGetValue(token, out var controller)) return;
            if(controller.InputAuthority.IsNone) NetworkManager.Runner.Despawn(controller);
            map.Remove(token);
        }

        public void Cleanup()
        {
            if (_pendingTokens == null) 
                _pendingTokens = new List<int>();
            else
                _pendingTokens.Clear();
            
            _mapAvatarToken.Clear();
            _mapPlayerToken.Clear();
        }
    }
}