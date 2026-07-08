using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Serializable]
    public class TextContent
    {
        public PropertyGetString playersMessageFormat = new("[{0}]: {1}");
        public bool enableNotificationMessages = true;
        public PropertyGetString joinedRoomMessage = new("Joined room {0}.");
        public PropertyGetString playerJoinedMessage = new("{0} Joined!");
        public PropertyGetString playerLeftMessage = new("{0} Left!");
    }
    [Serializable]
    public class ChatEvents
    {
        public InstructionList onOpen = new();
        public InstructionList onClose = new();
        public InstructionList onSendMessage = new();
        public InstructionList onReceiveMessage = new();
    }

    [Serializable]
    public class ChatColors
    {
        public PropertyGetColor playerColor = GetColorColorsCyan.Create;
        public PropertyGetColor othersColor = GetColorColorsWhite.Create;
        public PropertyGetColor serverColor = GetColorColorsYellow.Create;
    }

    /// <summary>
    /// Networked chat logic. Takes care of sending and receiving of chat messages.
    /// </summary>

    [AddComponentMenu("Game Creator/Fusion/Room Chat")]
    [HelpURL("https://docs.ninjutsugames.com/game-creator-2/fusion-module/user-interface#room-chat")]
    public class RoomChat : Chat, INetworkRunnerCallbacks
    {
        public static RoomChat Instance { get; private set; }

        [SerializeField] private ChatColors colors = new();
        [SerializeField] private TextContent notifications = new();
        [SerializeField] private ChatEvents events = new();
        
        public Dictionary<PlayerRef, string> LastMessages { get; private set; } = new();
        public NetworkCharacter LastPlayer { get; private set; }
        
        public static event Action<Args> EventChatMessage;

        private const string S_SPLIT = "|s|";
        private bool _initialized;

        protected override void Awake()
        {
            Instance = this;
            _initialized = false;
            input.Interactable = false;
            
            // Call base.Awake() after setting Instance to ensure it's available
            base.Awake();
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if(NetworkManager.Runner) NetworkManager.Runner.RemoveCallbacks(this); 
        }

        // Cache these event handlers to avoid GC allocations
        private readonly Action _onGameStartedHandler;
        private readonly Action _onGameCanceledHandler;
        private readonly Action _onGameFailedHandler;
        private readonly Action<NetworkPlayer> _onPlayerSpawnedHandler;
        private readonly Action<NetworkPlayer> _onPlayerDespawnedHandler;

        // Constructor to initialize the event handlers
        public RoomChat()
        {
            _onGameStartedHandler = OnGameStarted;
            _onGameCanceledHandler = OnGameCanceledOrFailed;
            _onGameFailedHandler = OnGameCanceledOrFailed;
            _onPlayerSpawnedHandler = OnPlayerSpawned;
            _onPlayerDespawnedHandler = OnPlayerDespawned;
        }

        private void OnEnable()
        {
            NetworkManager.EventGameStarted += _onGameStartedHandler;
            NetworkManager.EventGameCanceled += _onGameCanceledHandler;
            NetworkManager.EventGameFailed += _onGameFailedHandler;
            NetworkPlayer.EventPlayerSpawned += _onPlayerSpawnedHandler;
            NetworkPlayer.EventPlayerDespawned += _onPlayerDespawnedHandler;
            if(NetworkManager.IsConnected && !_initialized) OnGameStarted();
        }
        
        private void OnDisable()
        {
            NetworkManager.EventGameStarted -= _onGameStartedHandler;
            NetworkManager.EventGameCanceled -= _onGameCanceledHandler;
            NetworkManager.EventGameFailed -= _onGameFailedHandler;
            NetworkPlayer.EventPlayerSpawned -= _onPlayerSpawnedHandler;
            NetworkPlayer.EventPlayerDespawned -= _onPlayerDespawnedHandler;
        }
        

        private void OnPlayerSpawned(NetworkPlayer player)
        {
            if(notifications.enableNotificationMessages)
            {
                Add(string.Format(notifications.playerJoinedMessage.Get(Args.EMPTY), player.Username),
                    colors.serverColor.Get(Args.EMPTY), false, PlayerRef.None);
            }
        }

        private void OnPlayerDespawned(NetworkPlayer player)
        {
            if (notifications.enableNotificationMessages)
            {
                Add(string.Format(notifications.playerLeftMessage.Get(Args.EMPTY), player.Username),
                    colors.serverColor.Get(Args.EMPTY), false, PlayerRef.None);
            }
        }

        private void OnGameStarted()
        {
            if (_initialized) return;
            
            ClearHistory();
            ClearChatData();
            _initialized = true;
            NetworkManager.Runner.AddGlobal(this);

            if (notifications.enableNotificationMessages)
            {
                Add(string.Format(notifications.joinedRoomMessage.Get(Args.EMPTY), NetworkManager.Runner.SessionInfo.Name), colors.serverColor.Get(Args.EMPTY), false, PlayerRef.None);
            }

            input.Interactable = true;
            // input.GameObject.SetActive(true);
        }
        
        private void OnGameCanceledOrFailed()
        {
            ClearHistory();
            ClearChatData();
            _initialized = false;
            input.Interactable = false;
        }
        
        private void ClearChatData()
        {
            LastMessages.Clear();
            LastPlayer = null;
        }

        /// <summary>
        /// Send the chat message to everyone else.
        /// </summary>

        protected override void OnSubmit(string text)
        {
            Send(text);
        }

        /// <summary>
        /// True when input field is focused.
        /// </summary>
        public static bool IsOpen()
        {
            return Instance && Instance._selected; //mInst.input.isFocused;
        }

        public static void Send(string text)
        {
            _ = Instance.events.onSendMessage.Run(new Args(Instance.gameObject));
            RPC_ChatMessage(NetworkManager.Runner, text);
        }

        // Cached Args to reduce GC allocations
        private static readonly Args EmptyArgs = Args.EMPTY;
        
        [Rpc]
        private static void RPC_ChatMessage(NetworkRunner runner, string message, RpcInfo info = default)
        {
            var player = PlayerManager.Instance.GetPlayer(info.Source);
            if (player == null) return;
            
            Color color;
            string formattedMessage;
            bool isSystemMessage = message.Contains(S_SPLIT);
            
            if (isSystemMessage)
            {
                // System message
                formattedMessage = message.Replace(S_SPLIT, string.Empty);
                color = Instance.colors.serverColor.Get(EmptyArgs);
            }
            else
            {
                // Player message
                var avatar = PlayerManager.Instance.GetAvatar(info.Source);
                
                // Store original message for reference
                Instance.LastMessages[info.Source] = message;
                Instance.LastPlayer = avatar;
                
                // Format the message with player name
                formattedMessage = string.Format(
                    Instance.notifications.playersMessageFormat.Get(EmptyArgs), 
                    player.Username, 
                    message
                );
                
                // Determine target for args
                GameObject target = avatar ? avatar.gameObject : Instance.gameObject;
                
                // Create args only once
                var args = new Args(Instance.gameObject, target);
                
                // Get color based on player
                color = GetColorFromTarget(args, info.Source);
                
                // Trigger events
                EventChatMessage?.Invoke(args);
                _ = Instance.events.onReceiveMessage.Run(args);
            }
            
            // Add the message to the chat
            Instance.Add(formattedMessage, color, false, info.Source);
        }
        
        public static Color GetColorFromTarget(Args args, PlayerRef target)
        {
            return target == NetworkManager.Runner.LocalPlayer ? Instance.colors.playerColor.Get(args) : Instance.colors.othersColor.Get(args);
        }

        /// <summary>
        /// Add a new chat entry.
        /// </summary>
        /// <param name="text"></param>
        public static void Add(string text)
        {
            if(Instance == null)
            {
                Debug.LogWarning("Can't add chat messages there is no RoomChat instance found.");
                return;
            }
            Add(text, Instance.colors.serverColor.Get(Args.EMPTY));
        }

        /// <summary>
        /// Add a new chat entry.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="color"></param>
        public static void Add(string text, Color color)
        {
            if (Instance) Instance.Add(text, color, false, PlayerRef.None);
        }

        protected override void OnOpen()
        {
            _ = events.onOpen.Run(new Args(gameObject));
        }
        
        protected override void OnClose()
        {
            _ = events.onClose.Run(new Args(gameObject));
        }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) {}
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) {}
        public void OnInput(NetworkRunner runner, NetworkInput input) {}
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            if(runner && runner.IsRunning) runner.RemoveGlobal(this);
            if(this && input != null)
            {
                input.Interactable = false;
                // if(input.GameObject) input?.GameObject?.SetActive(false);
            }
            _initialized = false;
            ClearHistory();
            ClearChatData();
        }
        public void OnConnectedToServer() { OnGameStarted(); }
        public void OnConnectedToServer(NetworkRunner runner) { OnGameStarted(); }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) {}
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnSceneLoadDone(NetworkRunner runner) { OnGameStarted(); }
        public void OnSceneLoadStart(NetworkRunner runner) { }
        public void Spawned() {}
    }
}