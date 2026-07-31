#if CMPSETUP_COMPLETE
using System;
using System.Collections.Generic;
using System.Linq;
using AvocadoShark;
using Fusion;
using UnityEngine;

public class FusionLobbyBridge : MonoBehaviour
{
    public static FusionLobbyBridge Instance { get; private set; }

    [SerializeField] private NetworkRunner runnerPrefab;
    [SerializeField] private string gameSceneName = "Rpvp";
    [SerializeField] private int maxPlayers = 4;
    [SerializeField] private int minPlayersToStart = 1;
    [SerializeField] private bool autoStartWhenAllReady = true;
    [SerializeField] private bool allowSoloHostStart = true;

    public string PendingRoomCode { get; private set; } = string.Empty;
    public string LocalDisplayName { get; private set; } = "Player";
    public bool HasDisplayName { get; private set; }
    public bool IsInSession { get; private set; }
    public bool IsHost { get; private set; }
    public bool LocalReady { get; private set; }
    public bool IsStartingGame { get; private set; }
    public bool IsConnecting { get; private set; }
    public int MinPlayersToStart => Mathf.Max(1, minPlayersToStart);
    public bool AllowSoloHostStart => allowSoloHostStart;

    public event Action<string> OnStatusChanged;
    public event Action<string> OnRoomError;
    public event Action OnLobbyEntered;
    public event Action OnLobbyRosterChanged;
    public event Action OnGameStarting;
    public event Action OnConnecting;
    public event Action OnLobbyLeft;

    private readonly Dictionary<int, string> _playerNames = new Dictionary<int, string>();
    private readonly Dictionary<int, bool> _playerReady = new Dictionary<int, bool>();
    private FusionConnection _boundFusion;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        EnsureFusionConnection();
    }

    private void OnDestroy()
    {
        UnbindFusionEvents();
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void SetDisplayName(string rawName)
    {
        string trimmed = rawName?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return;
        }

        if (trimmed.Length > 16)
        {
            trimmed = trimmed[..16];
        }

        LocalDisplayName = trimmed;
        HasDisplayName = true;
        PlayerPrefs.SetString("MP_PLAYER_NAME", trimmed);
        PlayerPrefs.Save();

        EnsureFusionConnection();
        if (FusionConnection.Instance != null)
        {
            FusionConnection.Instance._playerName = trimmed;
        }

        SetStatus($"Hello {trimmed}");
    }

    public void PrepareHostRoom()
    {
        if (!HasDisplayName)
        {
            RaiseError("Enter your name first");
            return;
        }

        PendingRoomCode = RoomCodeGenerator.Generate();
        SetStatus($"Code {PendingRoomCode}");
    }

    public void EnterHostedRoom()
    {
        if (!HasDisplayName)
        {
            RaiseError("Enter your name first");
            return;
        }

        if (string.IsNullOrEmpty(PendingRoomCode))
        {
            PendingRoomCode = RoomCodeGenerator.Generate();
        }

        var fusion = EnsureFusionConnection();
        if (fusion == null)
        {
            RaiseError("FusionConnection missing");
            return;
        }

        if (!HasPhotonAppId())
        {
            RaiseError("Photon App Id empty — set AppIdFusion in PhotonAppSettings");
            return;
        }

        if (IsConnecting || IsInSession || IsStartingGame)
        {
            return;
        }

        IsHost = true;
        LocalReady = false;
        IsStartingGame = false;
        IsConnecting = true;
        fusion._playerName = LocalDisplayName;
        SetStatus($"Creating room {PendingRoomCode}...");
        OnConnecting?.Invoke();
        BindFusionEvents(fusion);
        fusion.JoinRoomStayInLobby(PendingRoomCode, maxPlayers, string.Empty);
    }

    public void JoinRoom(string rawCode)
    {
        if (!HasDisplayName)
        {
            RaiseError("Enter your name first");
            return;
        }

        if (IsConnecting || IsInSession || IsStartingGame)
        {
            return;
        }

        string roomCode = RoomCodeGenerator.Normalize(rawCode);
        if (!RoomCodeGenerator.IsValid(roomCode))
        {
            RaiseError("Room code must be 6 characters (a-z, A-Z, 0-9)");
            return;
        }

        if (!HasPhotonAppId())
        {
            RaiseError("Photon App Id empty — set AppIdFusion in PhotonAppSettings");
            return;
        }

        var fusion = EnsureFusionConnection();
        if (fusion == null)
        {
            RaiseError("FusionConnection missing");
            return;
        }

        PendingRoomCode = roomCode;
        IsHost = false;
        LocalReady = false;
        IsStartingGame = false;
        IsConnecting = true;
        fusion._playerName = LocalDisplayName;
        SetStatus($"Joining room {roomCode}...");
        OnConnecting?.Invoke();
        BindFusionEvents(fusion);
        fusion.JoinRoomStayInLobbyAsClient(roomCode);
    }

    public void LeaveLobby()
    {
        IsConnecting = false;
        IsInSession = false;
        IsStartingGame = false;
        LocalReady = false;
        IsHost = false;
        _playerNames.Clear();
        _playerReady.Clear();

        var fusion = FusionConnection.Instance;
        if (fusion?.Runner != null)
        {
            if (fusion.Runner.IsRunning)
            {
                fusion.Runner.Shutdown();
            }
        }

        SetStatus("Left room");
        OnLobbyLeft?.Invoke();
    }

    public void ToggleLocalReady()
    {
        if (!IsInSession || IsStartingGame)
        {
            return;
        }

        SetLocalReady(!LocalReady);
    }

    public void SetLocalReady(bool ready)
    {
        if (!IsInSession || IsStartingGame)
        {
            return;
        }

        LocalReady = ready;
        int localId = GetLocalPlayerId();
        if (localId >= 0)
        {
            _playerReady[localId] = ready;
            _playerNames[localId] = LocalDisplayName;
        }

        BroadcastLobbyState();
        OnLobbyRosterChanged?.Invoke();
        SetStatus(ready ? "You are ready" : "Ready cancelled");
        TryAutoStart();
    }

    public void HostStartGame()
    {
        if (!IsInSession || IsStartingGame)
        {
            return;
        }

        if (!IsLocalMasterClient())
        {
            RaiseError("Only Host can start the game");
            return;
        }

        if (GetPlayerCount() < MinPlayersToStart)
        {
            RaiseError($"Need at least {MinPlayersToStart} player(s) to start");
            return;
        }

        BeginGameStart();
    }

    public IReadOnlyList<LobbyRosterEntry> GetRoster()
    {
        var list = new List<LobbyRosterEntry>();
        var fusion = FusionConnection.Instance;
        if (fusion?.Runner == null || !fusion.Runner.IsRunning)
        {
            return list;
        }

        int localId = GetLocalPlayerId();
        foreach (var player in fusion.Runner.ActivePlayers.OrderBy(p => p.PlayerId))
        {
            int id = player.PlayerId;
            string name = _playerNames.TryGetValue(id, out var n) && !string.IsNullOrEmpty(n)
                ? n
                : (id == localId ? LocalDisplayName : $"Player {id + 1}");
            bool ready = _playerReady.TryGetValue(id, out var r) && r;
            list.Add(new LobbyRosterEntry(id, name, ready, id == localId));
        }

        return list;
    }

    public int GetPlayerCount()
    {
        var runner = FusionConnection.Instance?.Runner;
        if (runner == null || !runner.IsRunning)
        {
            return 0;
        }

        return runner.ActivePlayers.Count();
    }

    public bool AreAllPlayersReady()
    {
        var roster = GetRoster();
        if (roster.Count < MinPlayersToStart)
        {
            return false;
        }

        for (int i = 0; i < roster.Count; i++)
        {
            if (!roster[i].IsReady)
            {
                return false;
            }
        }

        return true;
    }

    private static bool HasPhotonAppId()
    {
        var settings = Fusion.Photon.Realtime.PhotonAppSettings.Global.AppSettings;
        return !string.IsNullOrWhiteSpace(settings.AppIdFusion)
               || !string.IsNullOrWhiteSpace(settings.AppIdRealtime);
    }

    public FusionConnection EnsureFusionConnection()
    {
        if (FusionConnection.Instance != null)
        {
            FusionConnection.Instance.Configure(runnerPrefab, gameSceneName);
            if (!string.IsNullOrEmpty(LocalDisplayName))
            {
                FusionConnection.Instance._playerName = LocalDisplayName;
            }

            return FusionConnection.Instance;
        }

        var existing = FindFirstObjectByType<FusionConnection>(FindObjectsInactive.Include);
        if (existing != null)
        {
            if (!existing.gameObject.activeSelf)
            {
                existing.gameObject.SetActive(true);
            }

            existing.Configure(runnerPrefab, gameSceneName);
            if (!string.IsNullOrEmpty(LocalDisplayName))
            {
                existing._playerName = LocalDisplayName;
            }

            return existing;
        }

        var go = new GameObject("FusionConnectionManager");
        var fusion = go.AddComponent<FusionConnection>();
        fusion.Configure(runnerPrefab, gameSceneName);
        if (!string.IsNullOrEmpty(LocalDisplayName))
        {
            fusion._playerName = LocalDisplayName;
        }

        return fusion;
    }

    private void BindFusionEvents(FusionConnection fusion)
    {
        if (_boundFusion == fusion)
        {
            return;
        }

        UnbindFusionEvents();
        _boundFusion = fusion;
        fusion.OnSharedSessionStarted += HandleSessionStarted;
        fusion.OnSharedSessionFailed += HandleSessionFailed;
        fusion.OnSessionPlayerJoined += HandlePlayerJoined;
        fusion.OnSessionPlayerLeft += HandlePlayerLeft;
        fusion.OnLobbyReliableMessage += HandleReliableMessage;
    }

    private void UnbindFusionEvents()
    {
        if (_boundFusion == null)
        {
            return;
        }

        _boundFusion.OnSharedSessionStarted -= HandleSessionStarted;
        _boundFusion.OnSharedSessionFailed -= HandleSessionFailed;
        _boundFusion.OnSessionPlayerJoined -= HandlePlayerJoined;
        _boundFusion.OnSessionPlayerLeft -= HandlePlayerLeft;
        _boundFusion.OnLobbyReliableMessage -= HandleReliableMessage;
        _boundFusion = null;
    }

    private void HandleSessionStarted()
    {
        IsConnecting = false;
        IsInSession = true;
        IsStartingGame = false;
        LocalReady = false;
        _playerNames.Clear();
        _playerReady.Clear();

        RefreshLocalRosterFromRunner();
        int localId = GetLocalPlayerId();
        if (localId >= 0)
        {
            _playerNames[localId] = LocalDisplayName;
            _playerReady[localId] = false;
        }

        IsHost = IsLocalMasterClient();
        SetStatus(IsHost
            ? $"Room ready — code {PendingRoomCode}"
            : $"Joined room {PendingRoomCode}");

        BroadcastLobbyState();
        OnLobbyEntered?.Invoke();
        OnLobbyRosterChanged?.Invoke();
    }

    private void HandleSessionFailed(string message)
    {
        IsConnecting = false;
        IsInSession = false;
        IsStartingGame = false;
        RaiseError(string.IsNullOrEmpty(message) ? "Failed to join room" : message);
        OnLobbyLeft?.Invoke();
    }

    private void HandlePlayerJoined(PlayerRef player)
    {
        if (!IsInSession)
        {
            return;
        }

        RefreshLocalRosterFromRunner();
        BroadcastLobbyState();
        OnLobbyRosterChanged?.Invoke();
        SetStatus($"Player joined ({GetPlayerCount()})");
        TryAutoStart();
    }

    private void HandlePlayerLeft(PlayerRef player)
    {
        _playerNames.Remove(player.PlayerId);
        _playerReady.Remove(player.PlayerId);
        OnLobbyRosterChanged?.Invoke();
        SetStatus($"Player left ({GetPlayerCount()})");
    }

    private void HandleReliableMessage(PlayerRef player, string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        if (message.StartsWith("STATE|", StringComparison.Ordinal))
        {
            ParseStateMessage(message);
            OnLobbyRosterChanged?.Invoke();
            TryAutoStart();
            return;
        }

        if (message == "START")
        {
            BeginGameStart();
        }
    }

    private void ParseStateMessage(string message)
    {
        var parts = message.Split('|');
        if (parts.Length < 4)
        {
            return;
        }

        if (!int.TryParse(parts[1], out int playerId))
        {
            return;
        }

        string name = parts[2];
        bool ready = parts[3] == "1";
        if (!string.IsNullOrEmpty(name))
        {
            _playerNames[playerId] = name;
        }

        _playerReady[playerId] = ready;
    }

    private void BroadcastLobbyState()
    {
        int localId = GetLocalPlayerId();
        if (localId < 0)
        {
            return;
        }

        string payload = $"STATE|{localId}|{LocalDisplayName}|{(LocalReady ? "1" : "0")}";
        FusionConnection.Instance?.SendLobbyReliableMessage(payload);
    }

    private void TryAutoStart()
    {
        if (!autoStartWhenAllReady || IsStartingGame || !IsInSession)
        {
            return;
        }

        if (!IsLocalMasterClient())
        {
            return;
        }

        if (!AreAllPlayersReady())
        {
            return;
        }

        BeginGameStart();
    }

    private void BeginGameStart()
    {
        if (IsStartingGame)
        {
            return;
        }

        IsStartingGame = true;
        SetStatus("Starting game...");
        OnGameStarting?.Invoke();

        if (IsLocalMasterClient())
        {
            FusionConnection.Instance?.SendLobbyReliableMessage("START");
        }

        var fusion = FusionConnection.Instance;
        if (fusion == null)
        {
            RaiseError("FusionConnection missing");
            IsStartingGame = false;
            return;
        }

        fusion.LoadConfiguredGameScene();
    }

    private void RefreshLocalRosterFromRunner()
    {
        var runner = FusionConnection.Instance?.Runner;
        if (runner == null)
        {
            return;
        }

        foreach (var player in runner.ActivePlayers)
        {
            int id = player.PlayerId;
            if (!_playerNames.ContainsKey(id))
            {
                _playerNames[id] = id == GetLocalPlayerId() ? LocalDisplayName : $"Player {id + 1}";
            }

            if (!_playerReady.ContainsKey(id))
            {
                _playerReady[id] = false;
            }
        }
    }

    private int GetLocalPlayerId()
    {
        var runner = FusionConnection.Instance?.Runner;
        if (runner == null || !runner.IsRunning)
        {
            return -1;
        }

        return runner.LocalPlayer.PlayerId;
    }

    private bool IsLocalMasterClient()
    {
        var runner = FusionConnection.Instance?.Runner;
        if (runner == null || !runner.IsRunning)
        {
            return IsHost;
        }

        return runner.IsSharedModeMasterClient || runner.IsServer;
    }

    private void SetStatus(string message)
    {
        Debug.Log($"[FusionLobbyBridge] {message}");
        OnStatusChanged?.Invoke(message);
    }

    private void RaiseError(string message)
    {
        Debug.LogError($"[FusionLobbyBridge] {message}");
        OnRoomError?.Invoke(message);
        OnStatusChanged?.Invoke(message);
    }
}

public readonly struct LobbyRosterEntry
{
    public readonly int PlayerId;
    public readonly string DisplayName;
    public readonly bool IsReady;
    public readonly bool IsLocal;

    public LobbyRosterEntry(int playerId, string displayName, bool isReady, bool isLocal)
    {
        PlayerId = playerId;
        DisplayName = displayName;
        IsReady = isReady;
        IsLocal = isLocal;
    }
}
#endif
