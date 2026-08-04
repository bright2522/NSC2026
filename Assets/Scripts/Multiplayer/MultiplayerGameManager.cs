#if CMPSETUP_COMPLETE
using Fusion;
using UnityEngine;
using System.Collections.Generic;
using StarterAssets;
using TMPro;
using AvocadoShark;

public class MultiplayerGameManager : NetworkBehaviour
{
    public static MultiplayerGameManager Instance;

    public static bool IsSpawnedReady =>
        Instance != null && Instance.Object != null && Instance.Object.IsValid;

    public enum GamePhase
    {
        WaitingRoom = 0,
        RoleSelection = 1,
        Countdown = 2,
        InGame = 3
    }

    [Header("Roles")]
    public RoleData[] roles;

    [Header("Timing")]
    public float selectDuration = 30f;
    public float startCountdownDuration = 5f;
    public float matchDuration = 300f;

    [Header("Cooking")]
    [SerializeField] private CompetitionMatchController competitionMatchController;

    public void SetCompetitionController(CompetitionMatchController controller)
    {
        competitionMatchController = controller;
    }

    [Header("UI")]
    public TMP_Text gameStateText;
    public TMP_Text playerCountText;
    public TMP_Text readyCountText;
    public TMP_Text timerText;
    public TMP_Text logText;

    [Networked] public bool GameStarted { get; set; }
    [Networked] public TickTimer MatchTimer { get; set; }
    [Networked] public int PlayerCount { get; set; }
    [Networked] public int ReadyCount { get; set; }
    [Networked] public GamePhase Phase { get; set; }
    [Networked] public TickTimer SelectTimer { get; set; }
    [Networked] public TickTimer StartCountdown { get; set; }

    [Networked, Capacity(4)]
    public NetworkArray<int> RoleTakenBy => default;

    private float cachedRemainingTime;

    private void Awake()
    {
        Instance = this;
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority)
            return;

        RecalculatePlayerCounts();
        UpdatePlayerWaitingIndices();
        PlayerData[] players = FindObjectsOfType<PlayerData>();

        switch (Phase)
        {
            case GamePhase.WaitingRoom:
                CheckReady(players);
                break;
            case GamePhase.RoleSelection:
                TickRoleSelection(players);
                break;
            case GamePhase.Countdown:
                TickStartCountdown();
                break;
            case GamePhase.InGame:
                if (MatchTimer.Expired(Runner))
                    EndGame();
                break;
        }

        UpdateLobbyUI();
        UpdateTimerUI();
    }

    void UpdatePlayerWaitingIndices()
    {
        PlayerData[] players = FindObjectsOfType<PlayerData>();
        List<PlayerData> sortedPlayers = new List<PlayerData>(players);
        sortedPlayers.Sort((a, b) => a.Object.Id.CompareTo(b.Object.Id));

        for (int i = 0; i < sortedPlayers.Count; i++)
            sortedPlayers[i].WaitingIndex = i + 1;
    }

    void RecalculatePlayerCounts()
    {
        PlayerData[] players = FindObjectsOfType<PlayerData>();
        PlayerCount = players.Length;

        int ready = 0;
        foreach (var p in players)
            if (p.IsReady) ready++;

        ReadyCount = ready;
    }

    void UpdateLobbyUI()
    {
        if (gameStateText != null)
            gameStateText.text = GameStarted ? "IN GAME" : Phase.ToString();

        if (playerCountText != null)
            playerCountText.text = $"Players : {PlayerCount}";

        if (readyCountText != null)
            readyCountText.text = $"Ready : {ReadyCount}/{PlayerCount}";
    }

    void CheckReady(PlayerData[] players)
    {
        if (players.Length < 1)
            return;

        foreach (var p in players)
            if (!p.IsReady)
                return;

        BeginRoleSelection(players);
    }

    void BeginRoleSelection(PlayerData[] players)
    {
        if (roles == null || roles.Length == 0)
        {
            Phase = GamePhase.Countdown;
            StartCountdown = TickTimer.CreateFromSeconds(Runner, startCountdownDuration);
            Log("Skip role selection — start countdown");
            return;
        }

        Phase = GamePhase.RoleSelection;
        SelectTimer = TickTimer.CreateFromSeconds(Runner, selectDuration);
        Log("All ready — role selection");

        foreach (var p in players)
            RPC_ResetRole(p.Object.InputAuthority);

        for (int i = 0; i < RoleTakenBy.Length; i++)
            RoleTakenBy.Set(i, -1);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestSelectRole(PlayerRef requester, int roleID)
    {
        if (Phase != GamePhase.RoleSelection)
            return;
        if (roleID < 0 || roleID >= roles.Length)
            return;

        foreach (var p in FindObjectsOfType<PlayerData>())
        {
            if (p.Object.InputAuthority != requester)
                continue;
            if (p.RoleID != -1)
                return;
            break;
        }

        if (roleID < RoleTakenBy.Length)
            RoleTakenBy.Set(roleID, requester.PlayerId);

        RPC_AssignRole(requester, roleID);
    }

    void TickRoleSelection(PlayerData[] players)
    {
        bool allPicked = true;
        foreach (var p in players)
        {
            if (p.RoleID == -1)
            {
                allPicked = false;
                break;
            }
        }

        if (allPicked)
        {
            Phase = GamePhase.Countdown;
            StartCountdown = TickTimer.CreateFromSeconds(Runner, startCountdownDuration);
            return;
        }

        if (SelectTimer.Expired(Runner))
        {
            AutoAssignRemainingRoles(players);
            Phase = GamePhase.Countdown;
            StartCountdown = TickTimer.CreateFromSeconds(Runner, startCountdownDuration);
        }
    }

    void AutoAssignRemainingRoles(PlayerData[] players)
    {
        if (roles == null || roles.Length == 0)
            return;

        foreach (var p in players)
        {
            if (p.RoleID != -1)
                continue;

            int roleID = Random.Range(0, roles.Length);
            if (roleID < RoleTakenBy.Length)
                RoleTakenBy.Set(roleID, p.Object.InputAuthority.PlayerId);

            RPC_AssignRole(p.Object.InputAuthority, roleID);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_AssignRole(PlayerRef targetPlayer, int roleID)
    {
        foreach (var p in FindObjectsOfType<PlayerData>())
        {
            if (p.Object.InputAuthority != targetPlayer)
                continue;
            if (!p.Object.HasStateAuthority)
                continue;
            p.RoleID = roleID;
            break;
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_ResetRole(PlayerRef targetPlayer)
    {
        foreach (var p in FindObjectsOfType<PlayerData>())
        {
            if (p.Object.InputAuthority != targetPlayer)
                continue;
            if (!p.Object.HasStateAuthority)
                continue;
            p.RoleID = -1;
            break;
        }
    }

    public float GetStartCountdownRemaining()
    {
        return StartCountdown.RemainingTime(Runner) ?? 0f;
    }

    void TickStartCountdown()
    {
        if (!StartCountdown.Expired(Runner))
            return;

        GameStarted = true;
        MatchTimer = TickTimer.CreateFromSeconds(Runner, matchDuration);
        Phase = GamePhase.InGame;
        Log("GAME STARTED");
        RPC_GameStarted();
    }

    void UpdateTimerUI()
    {
        float remain = MatchTimer.RemainingTime(Runner) ?? 0;

        if (Mathf.Abs(remain - cachedRemainingTime) < 0.5f)
            return;

        cachedRemainingTime = remain;

        int min = Mathf.FloorToInt(remain / 60);
        int sec = Mathf.FloorToInt(remain % 60);

        if (timerText != null)
            timerText.text = $"{min:00}:{sec:00}";
    }

    void EndGame()
    {
        GameStarted = false;
        Log("GAME END");
        RPC_EndGame();
        Phase = GamePhase.WaitingRoom;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_GameStarted()
    {
        foreach (var player in FindObjectsOfType<PlayerStats>())
        {
            if (!player.HasInputAuthority || !player.Object.HasStateAuthority)
                continue;
            player.BeginMatch();
        }

        if (MatchManager.Instance != null)
            MatchManager.Instance.StartMatch();

        var cooking = competitionMatchController != null
            ? competitionMatchController
            : FindFirstObjectByType<CompetitionMatchController>();
        if (cooking != null)
            cooking.BeginMatch();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_EndGame()
    {
        foreach (var player in FindObjectsOfType<PlayerData>())
        {
            ThirdPersonController controller = player.GetComponent<ThirdPersonController>();
            if (controller != null)
                controller.enabled = false;

            if (player.HasInputAuthority && player.endGameUI != null)
                player.endGameUI.SetActive(true);
        }

        var cooking = competitionMatchController != null
            ? competitionMatchController
            : FindFirstObjectByType<CompetitionMatchController>();
        if (cooking != null)
            cooking.EndMatch(loadResultScene: false);
    }

    void Log(string msg)
    {
        Debug.Log($"[MultiplayerGameManager] {msg}");
        if (logText != null)
            logText.text += msg + "\n";
    }
}
#endif
