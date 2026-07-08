using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using Fusion;
using Fusion.Photon.Realtime;
using Fusion.Sockets;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [AddComponentMenu("")]
    public class NetworkManager : Singleton<NetworkManager>, INetworkRunnerCallbacks, ISpawned
    {
        public enum Status
        {
            CreatingSession,
            JoiningSession,
            GameStarted,
            Disconnected,
            ConnectedToServer,
            SceneLoadStart,
            SceneLoadDone,
            Disconnecting
        }
        public enum LobbyStatus
        {
            JoinedLobby,
            CreatingLobby,
            JoiningLobby,
            Disconnected,
            Disconnecting
        }
        public static NetworkRunner Runner
        {
            get
            {
                if (!Instance) return null;
                return !Instance._runner ? Instance._runner = Instance.GetNewRunner() : Instance._runner;
            }
        }

        public static NetworkRunner RunnerLobby => !Instance._runnerLobby ? Instance._runnerLobby = Instance.GetNewLobbyRunner() : Instance._runnerLobby;
        public static INetworkSceneManager NetworkSceneManager { get; private set; }

        public static ShutdownReason LastShutdownReason { get; private set; }
        public static string LastErrorMessage { get; private set; }
        public static NetDisconnectReason LastDisconnectReason { get; private set; }
        public static GameMode LobbyGameMode { get; set; }
        
        public static byte[] ConnectionToken => _connectionToken ??= ConnectionTokenUtils.NewToken();
        
        public static List<SessionInfo> SessionList { get; private set; }

        public static event Action EventGameStarting;
        public static event Action EventGameStarted;
        public static event Action EventGameCanceled;
        public static event Action EventGameFailed;
        public static event Action EventLobbyStarting;
        public static event Action EventLobbyStarted;
        public static event Action EventLobbyCanceled;
        public static event Action EventLobbyFailed;
        public static event Action EventSelectedRegionChanged;
        public static event Action EventSessionListUpdated;
        public static event Action EventSceneLoadStart;
        public static event Action EventSceneLoadDone;
        
        public static NetworkProjectConfigAsset NetworkProjectConfig
        {
            get
            {
                if (!_networkProjectConfig)
                {
                    _networkProjectConfig = NetworkProjectConfigAsset.Global;
                }

                return _networkProjectConfig;
            }
            set => _networkProjectConfig = value;
        }
        
        public static bool IsConnected => Instance && Instance._runner && Instance._runner.IsRunning;
        public static bool IsConnectedInLobby => Instance && Instance._runnerLobby && Instance._runnerLobby.LobbyInfo.IsValid;
        
        public static readonly Dictionary<string, GameObject> RuntimeAttachments = new();
        public static readonly Dictionary<string, ModelConfig> RuntimeModels = new();
        public static ConnectionArgs ConnectionArgs { get; } = new();

        public static Status NetworkStatus { get; private set; }
        public static LobbyStatus LobbyNetworkStatus { get; private set; }
        private NetworkRunner _runner;
        private NetworkRunner _runnerLobby;
        private static NetworkProjectConfigAsset _networkProjectConfig;
        private static byte[] _connectionToken;
        private Coroutine _corMigrationCleanup;
        private bool _processingDirectJoin;
        private static Task<StartGameResult> _startGameTask;
        private static CancellationTokenSource _cancellationTokenSource;
        private static CancellationTokenSource _cancellationTokenSourceLobby;
        private static CancellationToken _cancellationToken;
        private static CancellationToken _cancellationTokenLobby;
        private static bool _connectingSafeCheck;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void OnSubsystemsInit()
        {
            NetworkStatus = Status.Disconnected;
            RuntimeAttachments.Clear();
            RuntimeModels.Clear();
            Instance.WakeUp();
            _connectingSafeCheck = false;
            
            _connectionToken = null;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            _cancellationToken = CancellationToken.None;
            
            _cancellationTokenSourceLobby?.Dispose();
            _cancellationTokenSourceLobby = null;
            _cancellationTokenLobby = CancellationToken.None;
            SessionList = new List<SessionInfo>();
        }
        
        private NetworkRunner GetNewRunner()
        {
            // if current _runner doesn't exist, create a new one with a new game object
            if (_runner != null) return _runner;

            var customRunner = FusionRepository.Get.Settings.customRunnerPrefab.Get(gameObject);
            var go = customRunner ? Instantiate(customRunner) : new GameObject("NetworkRunner");
            DontDestroyOnLoad(go);
            var runner = go.GetComponent<NetworkRunner>() ?? go.AddComponent<NetworkRunner>();
            NetworkSceneManager = runner.Add<NetworkSceneManager>();
            var input = runner.Add<NetworkInputPooling>();
            runner.Add<PooledNetworkObjectProvider>();
            runner.AddCallbacks(this, input);
            return runner;
        }
        
        private NetworkRunner GetNewLobbyRunner()
        {
            // if current _runner doesn't exist, create a new one with a new game object
            if (_runnerLobby != null) return _runnerLobby;

            var go = new GameObject("NetworkLobbyRunner");
            DontDestroyOnLoad(go);
            var runner = go.AddComponent<NetworkRunner>();
            runner.AddCallbacks(this);
            return runner;
        }

        private static void SpawnManagers(NetworkRunner runner)
        {
            if(!runner.IsResume && runner.IsServer || runner.IsSharedModeMasterClient)
            {
                runner.SpawnAsync(NetworkPrefabId.FromRaw(PooledNetworkObjectProvider.PLAYER_MANAGER), Vector3.zero, Quaternion.identity, null, null, NetworkSpawnFlags.DontDestroyOnLoad | NetworkSpawnFlags.SharedModeStateAuthMasterClient);
                // Runner.Spawn(NetworkPrefabId.FromRaw(PooledNetworkObjectProvider.NETWORK_DATA_MANAGER), Vector3.zero, Quaternion.identity, null, null, NetworkSpawnFlags.DontDestroyOnLoad | NetworkSpawnFlags.SharedModeStateAuthMasterClient);
            }
        }

        private static void SpawnLateManagers(NetworkRunner runner)
        {
            if(!runner.IsResume && runner.IsServer || runner.IsSharedModeMasterClient)
            {
                if(runner.IsServer) Runner.SpawnAsync(NetworkPrefabId.FromRaw(PooledNetworkObjectProvider.PLAYER_MANAGER), Vector3.zero, Quaternion.identity, null, null, NetworkSpawnFlags.DontDestroyOnLoad | NetworkSpawnFlags.SharedModeStateAuthMasterClient);
                runner.SpawnAsync(NetworkPrefabId.FromRaw(PooledNetworkObjectProvider.NETWORK_DATA_MANAGER), Vector3.zero, Quaternion.identity, null, null, NetworkSpawnFlags.DontDestroyOnLoad | NetworkSpawnFlags.SharedModeStateAuthMasterClient);
            }
        }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef inputAuthority)
        {
            // Debug.LogWarning($"[NetworkManager] OnPlayerJoined: {player} isServer: {runner.IsServer} IsSharedModeMasterClient: {runner.IsSharedModeMasterClient} isLocalPlayer: {runner.LocalPlayer == player}");
            
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            // Debug.LogWarning($"[NetworkManager] OnPlayerLeft: {player} isServer: {runner.IsServer} IsSharedModeMasterClient: {runner.IsSharedModeMasterClient} isLocalPlayer: {runner.LocalPlayer == player}");
            
            if (!runner.IsSharedModeMasterClient) return;

            var objects = runner.GetAllNetworkObjects();
            foreach (var obj in objects)
            {
                // If the state authority cannot be overridden, it is skipped.
                if ((obj.Flags & NetworkObjectFlags.AllowStateAuthorityOverride) != NetworkObjectFlags.AllowStateAuthorityOverride ||
                    (obj.Flags & NetworkObjectFlags.MasterClientObject) == NetworkObjectFlags.MasterClientObject ||
                    (obj.Flags & NetworkObjectFlags.DestroyWhenStateAuthorityLeaves) == NetworkObjectFlags.DestroyWhenStateAuthorityLeaves)
                    continue;

                // If the state authority of the object is equal to the player who left, we transfer ownership to the shared mode master client.
                if (obj.StateAuthority == player)
                    obj.RequestStateAuthority();
            }
        }

        protected void OnDestroy()
        {
            // Clean up cancellation token sources to prevent memory leaks
            if (_cancellationTokenSource == null) return;
            try
            {
                if (!_cancellationTokenSource.IsCancellationRequested)
                {
                    _cancellationTokenSource.Cancel();
                }
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[NetworkManager] Error disposing token on destroy: {ex.Message}");
            }
        }
        public void OnInput(NetworkRunner runner, NetworkInput input) {}

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            NetworkStatus = Status.Disconnected;
            PlayerManager.Instance?.Cleanup();

            LastErrorMessage = string.Empty;
            LastShutdownReason = shutdownReason;
            if(_corMigrationCleanup != null)
            {
                Instance.StopCoroutine(_corMigrationCleanup);
                _corMigrationCleanup = null;
            }
        }

        public void OnConnectedToServer() {}

        public void OnConnectedToServer(NetworkRunner runner)
        {
            NetworkStatus = Status.ConnectedToServer;
            SpawnManagers(runner);
            // Debug.LogWarning($"[NetworkManager] Connected to server: {Runner} ({Runner.LocalPlayer})");
        }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
            NetworkStatus = Status.Disconnected;
            LastDisconnectReason = reason;
        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request,
            byte[] token)
        {
            // Debug.LogWarning($"[NetworkManager] OnConnectRequest {request.RemoteAddress}");
            // request.Accept();
        }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            SessionList = sessionList;
            EventSessionListUpdated?.Invoke();
        }
        
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

        public async void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
            Debug.LogWarning($"[NetworkManager] OnHostMigration {hostMigrationToken}");
            NetworkSceneInfo sceneInfo = default;
            runner.TryGetSceneInfo(out sceneInfo);
            
            await runner.Shutdown(true, ShutdownReason.HostMigration);

            _runner = null;
            
            // Reload Scene
            /*var completedLoad = new TaskCompletionSource<bool>();
            var op = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);

            op.completed += _ => completedLoad.SetResult(true);

            // Wait scene to reload
            await completedLoad.Task;*/
            // await Task.Delay(100);
            
            await Runner.StartGame(new StartGameArgs
            {
                Scene = sceneInfo,
                GameMode = hostMigrationToken.GameMode,
                HostMigrationToken = hostMigrationToken,
                HostMigrationResume = OnHostMigrationResume,
                ConnectionToken = ConnectionToken,
                ObjectProvider = Runner.Get<PooledNetworkObjectProvider>()
            });
        }

        private void OnHostMigrationResume(NetworkRunner runner)
        {
            Debug.LogWarning($"[NetworkManager] OnHostMigrationResume isResume: {runner.IsResume}");
            
            // Loop through all NetworkObjects in the simulation based on the latest Snapshot sent by the old Host
            // All NetworkObject here must be used only as a reference to re-create the Simulation
            // None of those are attached to the new Simulation but they store all States from the old Simulation
            foreach (var resumeNo in runner.GetResumeSnapshotNetworkObjects())
            {
                var pos = Vector3.zero;
                var rot = Quaternion.identity;
                if(resumeNo.TryGetBehaviour<NetworkCharacterController>(out var posRot))
                {
                    pos = posRot.Data.Position;
                    rot = posRot.Data.Rotation;
                }
                if(resumeNo.TryGetBehaviour<NetworkTransform>(out var posRot2))
                {
                    pos = posRot2.Data.Position;
                    rot = posRot2.Data.Rotation;
                }

                NetworkSpawnFlags flags = 0;
                
                if(resumeNo.NetworkTypeId == NetworkPrefabId.FromRaw(PooledNetworkObjectProvider.PLAYER_MANAGER) || resumeNo.NetworkTypeId == NetworkPrefabId.FromRaw(PooledNetworkObjectProvider.PLAYER_DATA))
                {
                    flags = NetworkSpawnFlags.DontDestroyOnLoad;
                }
                
                runner.SpawnAsync(resumeNo, pos, rot, 
                    onBeforeSpawned: (r, newNo) => newNo.CopyStateFrom(resumeNo), 
                    flags: flags, onCompleted: result =>
                    {
                        if(!result.Object) return;
                        if(result.Object.TryGetBehaviour<NetworkCharacter>(out var avatar))
                        {
                            PlayerManager.Instance.SetAvatarConnectionToken(avatar.Token, avatar.Object);
                        }
                
                        if(result.Object.TryGetBehaviour<NetworkPlayer>(out var player))
                        {
                            PlayerManager.Instance.SetPlayerConnectionToken(player.Token, player.Object);
                        }
                    });
            }
            
            foreach (var resumeNO in runner.GetResumeSnapshotNetworkSceneObjects())
            {
                var sceneObject = resumeNO.Item1; // Reference to local Scene Object
                var objectState = resumeNO.Item2; // Reference to Scene Object State from old Host

                // Copy data back to Scene Object
                sceneObject.CopyStateFrom(objectState);
            }

            _corMigrationCleanup = StartCoroutine(CorHostMigrationCleanup());
        }

        private IEnumerator CorHostMigrationCleanup()
        {
            yield return new WaitForSeconds(10);

            PlayerManager.Instance.CleanupAfterHostMigration();
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

        public void OnSceneLoadDone(NetworkRunner runner)
        {
            NetworkStatus = Status.SceneLoadDone;

            // Debug.LogWarning($"[NetworkManager] OnSceneLoadDone isServer: {runner.IsServer} IsSharedModeMasterClient: {runner.IsSharedModeMasterClient} isResume: {runner.IsResume}");
            EventSceneLoadDone?.Invoke();
        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {
            NetworkStatus = Status.SceneLoadStart;
            SpawnLateManagers(runner);
            EventSceneLoadStart?.Invoke();

            // Debug.LogWarning($"[NetworkManager] OnSceneLoadStart");
        }
        public void Spawned()
        {
            Debug.LogWarning("[NetworkManager] Spawned");
        }

        public static NetworkPrefabId GetPrefabId(NetworkPrefabRef networkPrefabRef)
        {
            return NetworkProjectConfig.Config.PrefabTable.GetId((NetworkObjectGuid)networkPrefabRef);
        }
        
        public static INetworkPrefabSource GetSource(NetworkPrefabRef networkPrefabRef)
        {
            return NetworkProjectConfig.Config.PrefabTable.GetSource((NetworkObjectGuid)networkPrefabRef);
        }

        public static void OnGameStarted(NetworkRunner runner)
        {
            NetworkStatus = Status.GameStarted;
            EventGameStarted?.Invoke();
        }

        public static void OnJoinedLobby()
        {
            LobbyNetworkStatus = LobbyStatus.JoinedLobby;
            EventLobbyStarted?.Invoke();
        }

        

        #region Spawn Object
        
        public void TrySpawn(NetworkPrefabRef prefabId, PlayerRef inputAuthority, Vector3 position, Quaternion rotation,
            NetworkSpawnFlags networkSpawnFlags)
        {
            if (Runner.Topology == Topologies.Shared)
            {
                SpawnObject(prefabId, inputAuthority, position, rotation, networkSpawnFlags);
            }
            else
            {
                RPC_RequestSpawn(prefabId, inputAuthority, position, rotation, networkSpawnFlags);
            }
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_RequestSpawn(NetworkPrefabRef prefabId, PlayerRef inputAuthority, Vector3 position,
            Quaternion rotation, NetworkSpawnFlags networkSpawnFlags)
        {
            SpawnObject(prefabId, inputAuthority, position, rotation, networkSpawnFlags);
        }

        private void SpawnObject(NetworkPrefabRef prefabId, PlayerRef inputAuthority, Vector3 position,
            Quaternion rotation, NetworkSpawnFlags networkSpawnFlags)
        {
            if(prefabId.IsValid == false)
            {
                Debug.LogWarning($"[NetworkSpawn] Invalid prefabId: {prefabId}");
                return;
            }
            Runner.SpawnAsync(prefabId, position, rotation, inputAuthority, null, networkSpawnFlags);
        }
        
        public void TryDespawn(NetworkId obj)
        {
            if (Runner.Topology == Topologies.Shared)
            {
                DespawnObject(obj);
            }
            else
            {
                RPC_RequestDespawn(obj);
            }
        }

        private void RPC_RequestDespawn(NetworkId objId)
        {
            DespawnObject(objId);
        }

        private void DespawnObject(NetworkId networkId)
        {
            if(!Runner) return;
            if(!Runner.IsRunning) return;
            var no = Runner.FindObject(networkId);
            if(no && no.IsValid) Runner.Despawn(no);
        }

        #endregion

        #region Connection
        
        private static void CleanupRunner(NetworkRunner runner)
        {
            if (!runner) return;
            var runnerObject = runner.gameObject;
            if (runnerObject)
            {
                Destroy(runnerObject);
            }
            Instance._runner = null;
        }
        
        public static async Task DisconnectAsync()
        {
            NetworkStatus = Status.Disconnecting;
            if (_cancellationTokenSource != null)
            {
                EventGameCanceled?.Invoke();
                _cancellationTokenSource.Cancel();
            }
            
            if (Instance._runner)
            {
                if(Instance._runner.IsInSession) await Instance._runner.Shutdown();
                CleanupRunner(Instance._runner);
            }
        }
        
        public static async Task<bool> ConnectAsync(Args args, string sessionName, int playerCount, GameMode gameMode, SceneSelector sceneSettings, RegionSettings regionSettings, StartGameSettings advancedSettings, AuthenticationSettings authenticationSettings, CancellationToken externalCancellationToken = default)
        {
            // Skip if we're processing a direct join
            if (Instance._processingDirectJoin)
            {
                Debug.Log("[NetworkManager] Skipping ConnectAsync because a direct join is in progress");
                return false;
            }

            NetworkStatus = Status.CreatingSession;
            
            // Allow status update to be processed
            await Task.Yield();
            
            var codeGenerator = FusionRepository.Get.SessionCodeGenerator;
            if (advancedSettings.validateSessionCode)
            {
                var isValid = codeGenerator.IsValid(sessionName);
                if(!isValid)
                {
                    //"Invalid Session Code"
                    Debug.LogWarning($"The session code '{sessionName}' is not a valid session code. Please enter {codeGenerator.Length} characters or digits.");
                    LastDisconnectReason = NetDisconnectReason.Requested;
                    NetworkStatus = Status.Disconnected;
                    LastShutdownReason = ShutdownReason.InvalidArguments;
                    EventGameFailed?.Invoke();
                    return false;
                }
            }
            
            if (_connectingSafeCheck)
            {
                _startGameTask = null;
                
                // Cancel any existing token
                try
                {
                    _cancellationTokenSource?.Cancel();
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[NetworkManager] Error canceling token: {ex.Message}");
                }

                // Use a timeout to prevent indefinite waiting
                var timeout = Time.realtimeSinceStartup + 3f; // 3 second timeout
                while (_connectingSafeCheck && Time.realtimeSinceStartup < timeout)
                {
                    await Awaitable.NextFrameAsync(externalCancellationToken);
                }

                if (_connectingSafeCheck)
                {
                    Debug.LogWarning("[NetworkManager] Force-resetting connecting flag after timeout");
                    _connectingSafeCheck = false;
                }
            }

            // Check if the external token has been cancelled
            if (externalCancellationToken.IsCancellationRequested)
            {
                Debug.Log("[NetworkManager] Connect operation cancelled by external token");
                NetworkStatus = Status.Disconnected;
                return false;
            }
            
            EventGameStarting?.Invoke();
            
            // Allow event to be processed
            await Task.Yield();
            
            _connectingSafeCheck = true;
            
            if (Instance._runner && Instance._runner.IsInSession)
            {
                await DisconnectAsync();
                // Allow disconnect to complete
                await Awaitable.NextFrameAsync(externalCancellationToken);
            }
            
            Runner.ProvideInput = gameMode != GameMode.Shared;
            
            // Create the NetworkSceneInfo from the current scene
            var sceneInfo = new NetworkSceneInfo();
            if (sceneSettings.scene != SceneSelector.SceneType.None)
            {
                var startingScene = sceneSettings.Get(args);
                var scene = startingScene is int i ? SceneRef.FromIndex(i) : SceneRef.FromPath((string)startingScene);
                if (scene.IsValid)
                {
                    sceneInfo.AddSceneRef(scene, sceneSettings.loadSceneMode);
                }
            }
            
            // Allow a frame to process before continuing with network operations
            await Task.Yield();
            
            var appSettings = BuildCustomAppSetting();
            if(regionSettings.regionType == RegionSettings.RegionType.FixedRegion)
            {
                var fixedRegion = regionSettings.region.Get(args);
                if(!string.IsNullOrEmpty(fixedRegion)) appSettings.FixedRegion = fixedRegion;
            }
            
            var customAppVersion = advancedSettings.CustomAppVersion.Get(args);
            if(!string.IsNullOrEmpty(customAppVersion))
            {
                appSettings.AppVersion = customAppVersion;
                ConnectionArgs.CustomAppVersion = customAppVersion;
            }
            
            var code = codeGenerator.EncodeRegion(codeGenerator.Create(), Mathf.Max(0, ConnectionArgs.SelectedRegionIndex));
            
            // Clean up any existing token
            if (_cancellationTokenSource != null)
            {
                try
                {
                    if (!_cancellationTokenSource.IsCancellationRequested)
                    {
                        _cancellationTokenSource.Cancel();
                    }
                    _cancellationTokenSource.Dispose();
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[NetworkManager] Error disposing token: {ex.Message}");
                }
            }
            
            // Create a new token source
            _cancellationTokenSource = new CancellationTokenSource();

            // If external token is provided, create a linked token source
            CancellationToken effectiveToken;
            if (externalCancellationToken != CancellationToken.None)
            {
                var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token, externalCancellationToken);
                effectiveToken = linkedSource.Token;
            }
            else
            {
                effectiveToken = _cancellationTokenSource.Token;
            }

            _cancellationToken = effectiveToken;
            
            var gameArgs = new StartGameArgs
            {
                GameMode = gameMode,
                SessionName = sessionName,
                SceneManager = NetworkSceneManager,
                CustomLobbyName = advancedSettings.CustomLobbyName.Get(args),
                IsOpen = advancedSettings.IsOpen.Get(args),
                IsVisible = advancedSettings.IsVisible.Get(args),
                EnableClientSessionCreation = advancedSettings.EnableClientSessionCreation.Get(args),
                MatchmakingMode = advancedSettings.MatchmakingMode,
                SessionNameGenerator = () => code,
                OnGameStarted = OnGameStarted,
                ConnectionToken = ConnectionToken,
                AuthValues = authenticationSettings.AuthValues,
                UseCachedRegions = regionSettings.regionType == RegionSettings.RegionType.BestRegion && regionSettings.useCachedRegions.Get(args),
                StartGameCancellationToken = _cancellationToken,
                ObjectProvider = Runner.Get<PooledNetworkObjectProvider>()
            };

            if (sceneSettings.scene != SceneSelector.SceneType.None)
            {
                gameArgs.Scene = sceneInfo;
            }
            
            if (appSettings != null)
            {
                gameArgs.CustomPhotonAppSettings = appSettings;
            }

            if (advancedSettings.SessionProperties.Object != null)
            {
                gameArgs.SessionProperties = advancedSettings.SessionProperties.ToSessionProperties();
            }
            
            if (playerCount > 0)
            {
                gameArgs.PlayerCount = playerCount;
            }
            
            var timeSyncConfig = new TimeSyncConfiguration
            {
                MaxLateSnapshots = 10.0,
                RedundantSnapshots = 0
            };
            Fusion.NetworkProjectConfig.Global.TimeSynchronizationOverride = timeSyncConfig;
            
            // Allow a frame to process before starting the game
            await Task.Yield();
            
            // Check if operation was cancelled
            if (_cancellationToken.IsCancellationRequested)
            {
                Debug.Log("[NetworkManager] Join session was cancelled before starting");
                NetworkStatus = Status.Disconnected;
                return false;
            }

            _startGameTask = Runner.StartGame(gameArgs);

            // Use a timeout to prevent indefinite waiting
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(30), _cancellationToken);
            var completedTask = await Task.WhenAny(_startGameTask, timeoutTask);

            // Check for cancellation
            if (_cancellationToken.IsCancellationRequested)
            {
                Debug.Log("[NetworkManager] Join session was cancelled during operation");
                NetworkStatus = Status.Disconnected;
                EventGameFailed?.Invoke();
                return false;
            }

            // Check for cancellation
            if (_cancellationToken.IsCancellationRequested)
            {
                Debug.Log("[NetworkManager] Connect session was cancelled");
                NetworkStatus = Status.Disconnected;
                EventGameFailed?.Invoke();
                _connectingSafeCheck = false;
                return false;
            }

            if (completedTask == timeoutTask && !_startGameTask.IsCompleted)
            {
                Debug.LogWarning("[NetworkManager] Connect session timed out after 30 seconds");
                _cancellationTokenSource.Cancel();
                NetworkStatus = Status.Disconnected;
                EventGameFailed?.Invoke();
                _connectingSafeCheck = false;
                return false;
            }
            
            try
            {
                var result = await _startGameTask;

                if (result.Ok) return true;
                if (result.ShutdownReason == ShutdownReason.OperationCanceled) return false;
                
                LastShutdownReason = result.ShutdownReason;
                LastErrorMessage = result.ErrorMessage;
                
                Debug.LogWarning($"Failed to Start Game: {result.ShutdownReason} error: {result.ErrorMessage}");
                
                NetworkStatus = Status.Disconnected;
                EventGameFailed?.Invoke();
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                LastShutdownReason = ShutdownReason.Error;
                LastErrorMessage = ex.Message;
                NetworkStatus = Status.Disconnected;
                EventGameFailed?.Invoke();
                return false;
            }
            finally
            {
                _connectingSafeCheck = false;
            }
        }


        private static FusionAppSettings BuildCustomAppSetting(string region = null, string appVersion = null) 
        { 
            //string customAppID = null, 
            var appSettings = PhotonAppSettings.Global.AppSettings.GetCopy();;

            appSettings.UseNameServer = true;
            if (string.IsNullOrEmpty(appVersion) == false) appSettings.AppVersion = appVersion;
            // if (string.IsNullOrEmpty(customAppID) == false) appSettings.AppIdFusion = customAppID;
            if (string.IsNullOrEmpty(region) == false) appSettings.FixedRegion = region.ToLower();

            return appSettings;
        }

        public async Task<bool> JoinSession(string sessionInfo, string lobbyInfoRegion, GameMode gameMode,
                        AuthenticationValues authentication, CancellationToken externalCancellationToken = default)
        {
            try
            {
                _processingDirectJoin = true;

                // Handle concurrent connection attempts with timeout
                if (_connectingSafeCheck)
                {
                    _startGameTask = null;
                    _cancellationTokenSource?.Cancel();

                    // Add timeout to prevent deadlock
                    var retryCount = 0;
                    const int maxRetries = 10; // Reduced max retries for faster response
                    while (_connectingSafeCheck && retryCount < maxRetries)
                    {
                        await Task.Yield();
                        retryCount++;
                    }

                    if (_connectingSafeCheck)
                    {
                        Debug.LogError(
                            "[NetworkManager] Failed to join session: previous connection attempt couldn't complete in time");
                        EventGameFailed?.Invoke();
                        _processingDirectJoin = false;
                        return false;
                    }
                }

                // Validate input parameters
                if (string.IsNullOrEmpty(sessionInfo))
                {
                    Debug.LogError("[NetworkManager] Failed to join session: sessionInfo is null or empty");
                    EventGameFailed?.Invoke();
                    _processingDirectJoin = false;
                    return false;
                }

                EventGameStarting?.Invoke();
                
                // Allow event to be processed before continuing
                await Task.Yield();
                
                NetworkStatus = Status.JoiningSession;
                _connectingSafeCheck = true;

                // Check if runner exists and is running
                if (_runner != null && _runner.IsRunning)
                {
                    await DisconnectAsync();
                    await Awaitable.EndOfFrameAsync();
                }

                // Proper cancellation token management with better error handling
                if (_cancellationTokenSource != null)
                {
                    try
                    {
                        if (!_cancellationTokenSource.IsCancellationRequested)
                        {
                            _cancellationTokenSource.Cancel();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[NetworkManager] Error canceling token: {ex.Message}");
                    }
                    
                    try
                    {
                        _cancellationTokenSource.Dispose();
                    }
                    catch (ObjectDisposedException)
                    {
                        // Already disposed, ignore
                    }
                }

                // Create a new token source
                _cancellationTokenSource = new CancellationTokenSource();

                // If external token is provided, create a linked token source
                CancellationToken effectiveToken;
                if (externalCancellationToken != CancellationToken.None)
                {
                    var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token, externalCancellationToken);
                    effectiveToken = linkedSource.Token;
                }
                else
                {
                    effectiveToken = _cancellationTokenSource.Token;
                }
                _cancellationToken = _cancellationTokenSource.Token;

                // Allow a frame to process before continuing with network operations
                await Task.Yield();

                var appSettings = BuildCustomAppSetting(lobbyInfoRegion);

                _runner = GetNewRunner();
        
                if (!_runner)
                {
                    Debug.LogError("[NetworkManager] Failed to join session: Could not create new Runner");
                    NetworkStatus = Status.Disconnected;
                    _connectingSafeCheck = false;
                    _processingDirectJoin = false;
                    EventGameFailed?.Invoke();
                    return false;
                }

                // Allow a frame to process before starting the game
                await Task.Yield();

                var startGameArgs = new StartGameArgs
                {
                    GameMode = gameMode,
                    SessionName = sessionInfo,
                    OnGameStarted = OnGameStarted,
                    StartGameCancellationToken = _cancellationToken,
                    CustomPhotonAppSettings = appSettings,
                    AuthValues = authentication,
                    SceneManager = NetworkSceneManager,
                    EnableClientSessionCreation = false,
                    ObjectProvider = Runner.Get<PooledNetworkObjectProvider>()
                };

                _startGameTask = Runner.StartGame(startGameArgs);

                // Use a timeout to prevent indefinite waiting
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(30), _cancellationToken);
                var completedTask = await Task.WhenAny(_startGameTask, timeoutTask);

                if (completedTask == timeoutTask && !_startGameTask.IsCompleted)
                {
                    Debug.LogError("[NetworkManager] Join session timed out after 30 seconds");
                    _cancellationTokenSource.Cancel();
                    NetworkStatus = Status.Disconnected;
                    EventGameFailed?.Invoke();
                    _processingDirectJoin = false;
                    _connectingSafeCheck = false;
                    return false;
                }

                var result = await _startGameTask;

                // Handle result
                if (result.Ok)
                {
                    _processingDirectJoin = false;
                    return true;
                }

                // Handle specific error cases
                if (result.ShutdownReason == ShutdownReason.OperationCanceled)
                {
                    Debug.Log("[NetworkManager] Join session operation was canceled");
                    _processingDirectJoin = false;
                    return false;
                }

                // Record failure details
                LastShutdownReason = result.ShutdownReason;
                LastErrorMessage = result.ErrorMessage;

                NetworkStatus = Status.Disconnected;
                EventGameFailed?.Invoke();
                _processingDirectJoin = false;
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex, this);
                NetworkStatus = Status.Disconnected;
                EventGameFailed?.Invoke();
                return false;
            }
            finally
            {
                // Always reset connecting flag to prevent deadlocks
                _connectingSafeCheck = false;
            }
        }
        
        #endregion

        #region Lobby

        public static async Task JoinLobbyAsync(Args args, RegionSettings regionSettings,
            JoinLobbySettings lobbySettings, LobbyAdvancedSettings advancedSettings,
            AuthenticationSettings authenticationSettings, CancellationToken externalCancellationToken = default)
        {
            LobbyNetworkStatus = LobbyStatus.CreatingLobby;
            EventLobbyStarting?.Invoke();
            
            _cancellationTokenSourceLobby?.Dispose();
            _cancellationTokenSourceLobby = new CancellationTokenSource();
            _cancellationTokenLobby = _cancellationTokenSourceLobby.Token;
            
            var appSettings = BuildCustomAppSetting();
            if(regionSettings.regionType == RegionSettings.RegionType.FixedRegion)
            {
                var fixedRegion = regionSettings.region.Get(args);
                if(!string.IsNullOrEmpty(fixedRegion)) appSettings.FixedRegion = fixedRegion;
            }
            
            var customAppVersion = advancedSettings.CustomAppVersion.Get(args);
            if(!string.IsNullOrEmpty(customAppVersion))
            {
                appSettings.AppVersion = customAppVersion;
                ConnectionArgs.CustomAppVersion = customAppVersion;
            }
            
            var joinLobbyTask = RunnerLobby.JoinSessionLobby(lobbySettings.sessionLobby, lobbySettings.lobbyId.Get(args), 
                authenticationSettings.AuthValues, appSettings, advancedSettings.useDefaultCloudPorts.Get(args), 
                _cancellationTokenLobby, 
                regionSettings.regionType == RegionSettings.RegionType.BestRegion && 
                regionSettings.useCachedRegions.Get(args));

            LobbyGameMode = lobbySettings.gameMode;
            
            var result = await joinLobbyTask;
            if (result.Ok)
            {
                OnJoinedLobby();
            }
            else
            {
                LobbyNetworkStatus = LobbyStatus.Disconnected;
                Debug.LogError($"Failed to Start Lobby: {result.ShutdownReason} error: {result.ErrorMessage}");
                LastShutdownReason = result.ShutdownReason;
                LastErrorMessage = result.ErrorMessage;
                EventLobbyFailed?.Invoke();
            }
        } 
        
        private static void CleanupLobbyRunner()
        {
            if (!Instance._runnerLobby) return;
            var runnerObject = Instance._runnerLobby.gameObject;
            if (runnerObject)
            {
                Destroy(runnerObject);
            }
            
            Instance._runnerLobby = null;
        }
        
        public static async Task DisconnectLobbyAsync()
        {
            LobbyNetworkStatus = LobbyStatus.Disconnecting;

            if (_cancellationTokenSourceLobby != null)
            {
                EventLobbyCanceled?.Invoke();
                _cancellationTokenSourceLobby.Cancel();
            }
            
            if (Instance._runnerLobby)
            {
                await Instance._runnerLobby.Shutdown();
                CleanupLobbyRunner();
            }
        }

        #endregion

        public void SetRegion(string regionToken, int index)
        {
            ConnectionArgs.SelectedRegion = regionToken;
            ConnectionArgs.SelectedRegionIndex = index;
            EventSelectedRegionChanged?.Invoke();
        }

        public static GameObject GetSelectedModelPrefab()
        {
            foreach (var model in RuntimeModels)
            {
                if (model.Value.prefab.Get(Args.EMPTY).name == ConnectionArgs.SelectedModel)
                {
                    return model.Value.prefab.Get(Args.EMPTY);
                }
            }
            return null;
        }
        
        public static Sprite GetSelectedModelSprite()
        {
            foreach (var model in RuntimeModels)
            {
                if (model.Value.prefab.Get(Args.EMPTY).name == ConnectionArgs.SelectedModel)
                {
                    return model.Value.sprite.Get(Args.EMPTY);
                }
            }
            return null;
        }
        
        public static async Task<List<RegionInfo>> PingRegions()
        {
            try
            {
                var availableRegions = FusionRepository.Get.Regions.RegionList.GetAvailable();
                var tokenSource = new CancellationTokenSource();
                var task = NetworkRunner.GetAvailableRegions(cancellationToken: tokenSource.Token);
                
                await task;

                if (!task.IsCompletedSuccessfully)
                    return null;
            
                var regions = task.Result;
                foreach (var regionInfo in availableRegions)
                {
                    var region = regions.FirstOrDefault(r => r.RegionCode == regionInfo.Token);
                    regionInfo.SetPing(region.RegionPing);
                }

                return task.Result;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to ping regions: {ex.Message}");
                return null;
            }
        }
    }
}