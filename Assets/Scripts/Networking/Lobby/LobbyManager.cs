using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance { get; private set; }

    [Header("Gameplay")]
    [SerializeField] private string gameplaySceneName = "Gameplay"; // TODO: replace when final scene name is decided

    public event Action<IReadOnlyList<LobbyPlayerState>> OnPlayersChanged;
    public event Action OnLocalPlayerRegistered;

    private readonly List<LobbyPlayerState> _players = new List<LobbyPlayerState>();
    private bool _localNameSubmitted;

    public bool IsLobbyReady => IsSpawned;
    public bool HasSubmittedLocalName => _localNameSubmitted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
            RegisterPlayerServer(NetworkManager.ServerClientId);
            BroadcastPlayers();
        }

        if (IsClient && !IsServer)
        {
            RegisterLocalPlayerServerRpc();
        }
        else if (IsServer)
        {
            OnLocalPlayerRegistered?.Invoke();
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void SubmitDisplayName(string rawName)
    {
        if (!IsSpawned || _localNameSubmitted)
        {
            return;
        }

        string trimmed = rawName?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return;
        }

        if (trimmed.Length > 16)
        {
            trimmed = trimmed[..16];
        }

        _localNameSubmitted = true;
        SubmitDisplayNameServerRpc(trimmed, NetworkManager.Singleton.LocalClientId);
    }

    public void KickPlayer(ulong clientId)
    {
        if (!IsServer || clientId == NetworkManager.ServerClientId)
        {
            return;
        }

        NetworkManager.Singleton.DisconnectClient(clientId);
    }

    public void StartGame()
    {
        if (!IsServer)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(gameplaySceneName))
        {
            Debug.LogWarning("[LobbyManager] gameplaySceneName is empty.");
            return;
        }

        NetworkManager.Singleton.SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);
    }

    public IReadOnlyList<LobbyPlayerState> GetPlayers()
    {
        return _players;
    }

    [ServerRpc(RequireOwnership = false)]
    private void RegisterLocalPlayerServerRpc(ServerRpcParams rpcParams = default)
    {
        RegisterPlayerServer(rpcParams.Receive.SenderClientId);
        BroadcastPlayers();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitDisplayNameServerRpc(string displayName, ulong clientId)
    {
        for (int i = 0; i < _players.Count; i++)
        {
            if (_players[i].ClientId != clientId)
            {
                continue;
            }

            var state = _players[i];
            state.DisplayName = new FixedString64Bytes(displayName);
            state.HasSubmittedName = true;
            _players[i] = state;
            BroadcastPlayers();
            return;
        }
    }

    [ClientRpc]
    private void SyncPlayersClientRpc(LobbyPlayerState[] players)
    {
        _players.Clear();
        if (players != null)
        {
            _players.AddRange(players);
        }

        OnPlayersChanged?.Invoke(_players);
    }

    private void RegisterPlayerServer(ulong clientId)
    {
        for (int i = 0; i < _players.Count; i++)
        {
            if (_players[i].ClientId == clientId)
            {
                return;
            }
        }

        _players.Add(new LobbyPlayerState
        {
            ClientId = clientId,
            DisplayName = default,
            HasSubmittedName = false
        });
    }

    private void HandleClientConnected(ulong clientId)
    {
        RegisterPlayerServer(clientId);
        BroadcastPlayers();
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        for (int i = _players.Count - 1; i >= 0; i--)
        {
            if (_players[i].ClientId == clientId)
            {
                _players.RemoveAt(i);
            }
        }

        BroadcastPlayers();
    }

    private void BroadcastPlayers()
    {
        if (!IsServer)
        {
            return;
        }

        SyncPlayersClientRpc(_players.ToArray());
    }
}
