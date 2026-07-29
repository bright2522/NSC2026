#if CMPSETUP_COMPLETE
using UnityEngine;
using Fusion;
using TMPro;
using System;
using UnityEngine.SceneManagement;
using System.Collections;

namespace AvocadoShark
{
    public enum PlayerStatus { Healthy, Injured, Downed }

    public class PlayerStats : NetworkBehaviour
    {
        private ChangeDetector _changeDetector;
        [Networked] public bool IsDisconnecting { get; set; }
        [Networked] public TickTimer VoteTime { get; set; }
        [Networked] public NetworkString<_32> PlayerName { get; set; }
        [Networked] public NetworkString<_32> VoteInitiatorPlayerName { get; set; }
        [Networked] public NetworkBool VoteKick { get; set; }
        [Networked] public int PositiveVotes { get; set; }
        [Networked] public int NegativeVotes { get; set; }
        [Networked] public PlayerStatus CurrentStatus { get; set; }
        [Networked] public TickTimer DamageCooldownTimer { get; set; }
        [Networked] public NetworkBool IsSkillImmobilized { get; set; }
        [SerializeField] private float damageCooldownDuration = 2.0f;
        [Networked] public float HealProgress { get; set; }
        [SerializeField] private float healSpeed = 0.2f;
        [Networked] public bool MatchStarted { get; set; }
        [Networked] private TickTimer SpawnProtectionTimer { get; set; }
        [SerializeField] private float spawnProtectionDuration = 15f;

        public Action<PlayerStatus> OnStatusChanged;

        public bool HasSpawnProtection =>
            Object != null && Object.IsValid && Runner != null &&
            !SpawnProtectionTimer.ExpiredOrNotRunning(Runner);

        [SerializeField] private Animator animator;
        [SerializeField] private string healthyLayerName = "Base Layer";
        [SerializeField] private string injuredLayerName = "Injured Layer";
        [SerializeField] private string downedLayerName = "Downed Layer";

        public int maxVoteTime = 15;
        public bool isVoteInitiator = false;
        public Action<int> OnPositiveVotesChanged, OnNegativeVotesChanged, OnVoteTimeUpdated;
        public Action<bool> OnSpeaking;

        [SerializeField] TextMeshPro playerNameLabel;
        public static PlayerStats instance;
        public Action<string> OnPlayerStatsReady;

        public bool CanMove => MatchStarted && CurrentStatus != PlayerStatus.Downed && !IsSkillImmobilized;

        public void SetSkillImmobilized(bool immobilized)
        {
            if (!Object.HasStateAuthority) return;
            IsSkillImmobilized = immobilized;
        }

        public void BeginMatch()
        {
            if (!Object.HasStateAuthority) return;
            MatchStarted = true;
            CurrentStatus = PlayerStatus.Healthy;
            HealProgress = 0f;
            IsSkillImmobilized = false;
            SpawnProtectionTimer = TickTimer.CreateFromSeconds(Runner, spawnProtectionDuration);
        }

        public override void Spawned()
        {
            _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
            GetComponent<PlayerWorldUIManager>().OnSpeaking += Speaking;

            if (animator == null)
                animator = GetComponentInChildren<Animator>();

            if (HasStateAuthority)
            {
                string name = FusionConnection.Instance != null
                    ? FusionConnection.Instance._playerName
                    : PlayerPrefs.GetString("MP_PLAYER_NAME", "Player");
                if (string.IsNullOrWhiteSpace(name))
                    name = "Player";

                PlayerName = name;
                CurrentStatus = PlayerStatus.Healthy;
                MatchStarted = false;
                OnPlayerStatsReady?.Invoke(PlayerName.ToString());
                if (playerNameLabel != null)
                    playerNameLabel.text = "";

                if (instance == null)
                    instance = this;

                if (MultiplayerGameManager.IsSpawnedReady && MultiplayerGameManager.Instance.GameStarted)
                    BeginMatch();
            }
            else if (playerNameLabel != null)
            {
                playerNameLabel.text = RichTextSafety.Escape(PlayerName.ToString());
            }

            if (SessionPlayers.instance != null)
                SessionPlayers.instance.AddPlayer(this);
            else
                StartCoroutine(WaitAndAddPlayer());

            UpdateAnimatorLayerWeights(CurrentStatus);
            PlayerHUDManager.Instance?.RefreshHUD();
        }

        private IEnumerator WaitAndAddPlayer()
        {
            yield return new WaitUntil(() => SessionPlayers.instance != null);
            if (this == null || Object == null || !Object.IsValid)
                yield break;
            SessionPlayers.instance.AddPlayer(this);
        }

        public override void Render()
        {
            foreach (var change in _changeDetector.DetectChanges(this, out var previousBuffer, out var currentBuffer))
            {
                switch (change)
                {
                    case nameof(PlayerName):
                        HandleChangeDetection<NetworkString<_32>>(nameof(PlayerName), previousBuffer, currentBuffer, UpdatePlayerName);
                        break;
                    case nameof(VoteKick):
                        HandleChangeDetection<NetworkBool>(nameof(VoteKick), previousBuffer, currentBuffer, OnVoteKickStateChanged);
                        break;
                    case nameof(PositiveVotes):
                        HandleChangeDetection<int>(nameof(PositiveVotes), previousBuffer, currentBuffer, OnPositiveVote);
                        break;
                    case nameof(NegativeVotes):
                        HandleChangeDetection<int>(nameof(NegativeVotes), previousBuffer, currentBuffer, OnNegativeVote);
                        break;
                    case nameof(CurrentStatus):
                        HandleChangeDetection<PlayerStatus>(nameof(CurrentStatus), previousBuffer, currentBuffer, OnStatusUpdate);
                        break;
                }
            }
        }

        private void HandleChangeDetection<T>(string propertyName, NetworkBehaviourBuffer previousBuffer,
            NetworkBehaviourBuffer currentBuffer, Action<T, T> callback) where T : unmanaged
        {
            var reader = GetPropertyReader<T>(propertyName);
            var (previous, current) = reader.Read(previousBuffer, currentBuffer);
            callback(previous, current);
        }

        private void Update()
        {
            if (Object == null || !Object.IsValid || !VoteKick)
                return;
            OnVoteTimeUpdated?.Invoke(Mathf.RoundToInt(VoteTime.RemainingTime(Runner).GetValueOrDefault()));
        }

        public override void FixedUpdateNetwork()
        {
            if (VoteTime.Expired(Runner) && VoteKick)
                VoteKick = false;
        }

        protected void UpdatePlayerName(NetworkString<_32> previous, NetworkString<_32> current)
        {
            if (SessionPlayers.instance != null)
                SessionPlayers.instance.AddPlayer(this);
            else
                StartCoroutine(WaitAndAddPlayer());

            if (playerNameLabel != null)
                playerNameLabel.text = !HasStateAuthority ? RichTextSafety.Escape(current.ToString()) : "";
            PlayerHUDManager.Instance?.RefreshHUD();
        }

        public void ChangeStatus(PlayerStatus newStatus)
        {
            if (Object.HasStateAuthority && HasSpawnProtection &&
                (newStatus == PlayerStatus.Injured || newStatus == PlayerStatus.Downed))
                return;

            if (animator != null)
                animator.Play("Damage");

            if (Object.HasStateAuthority)
            {
                CurrentStatus = newStatus;
                HealProgress = 0f;
            }
        }

        private void OnStatusUpdate(PlayerStatus previous, PlayerStatus current)
        {
            OnStatusChanged?.Invoke(current);
            UpdateAnimatorLayerWeights(current);
        }

        private void UpdateAnimatorLayerWeights(PlayerStatus status)
        {
            if (animator == null) return;

            int healthyIdx = animator.GetLayerIndex(healthyLayerName);
            int injuredIdx = animator.GetLayerIndex(injuredLayerName);
            int downedIdx = animator.GetLayerIndex(downedLayerName);

            if (injuredIdx != -1) animator.SetLayerWeight(injuredIdx, 0f);
            if (downedIdx != -1) animator.SetLayerWeight(downedIdx, 0f);

            switch (status)
            {
                case PlayerStatus.Healthy:
                    if (healthyIdx != -1 && healthyIdx != 0) animator.SetLayerWeight(healthyIdx, 1f);
                    break;
                case PlayerStatus.Injured:
                    if (injuredIdx != -1) animator.SetLayerWeight(injuredIdx, 1f);
                    break;
                case PlayerStatus.Downed:
                    if (downedIdx != -1) animator.SetLayerWeight(downedIdx, 1f);
                    break;
            }
        }

        public void RequestHeal()
        {
            RPC_ProgressHeal(Runner.DeltaTime * healSpeed);
        }

        [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
        private void RPC_ProgressHeal(float amount)
        {
            if (CurrentStatus != PlayerStatus.Downed) return;
            HealProgress += amount;
            if (HealProgress >= 1f)
            {
                HealProgress = 0f;
                CurrentStatus = PlayerStatus.Injured;
            }
        }

        public void InitializeVoteKick()
        {
            if (Object.HasStateAuthority) PositiveVotes += 1;
            if (NotEnoughPlayers() || IsDisconnecting || VoteKick) return;

            if (Runner.GameMode == GameMode.Shared)
            {
                if (Object.HasStateAuthority)
                {
                    VoteKick = true;
                    VoteInitiatorPlayerName = PlayerName;
                    VoteTime = TickTimer.CreateFromSeconds(Runner, maxVoteTime);
                }
                else
                {
                    RPC_BeginVoteKick();
                    isVoteInitiator = true;
                }
            }
        }

        public void OnVoteKickStateChanged(NetworkBool previous, NetworkBool current)
        {
            if (current)
            {
                SessionPlayers.instance.AddVoteKick(this);
                if (HasStateAuthority)
                {
                    PositiveVotes = 0;
                    NegativeVotes = 0;
                }
            }
            else
            {
                if (HasStateAuthority)
                {
                    if (PositiveVotes > NegativeVotes)
                    {
                        IsDisconnecting = true;
                        RemovePlayer();
                        RPC_PlayerVoteResultMessage($"Vote kick for {RichTextSafety.Escape(PlayerName.ToString())} has passed");
                    }
                    else
                    {
                        RPC_PlayerVoteResultMessage($"Vote kick for {RichTextSafety.Escape(PlayerName.ToString())} has failed");
                    }
                    PositiveVotes = 0;
                    NegativeVotes = 0;
                }
                isVoteInitiator = false;
                SessionPlayers.instance.RemoveVoteKick(this);
            }
        }

        public int GetNegativeVotes() => SessionPlayers.instance.activePlayers.Count - PositiveVotes - 1;
        public bool NotEnoughPlayers() => SessionPlayers.instance.activePlayers.Count <= 2;

        public void AddPositiveVote()
        {
            if (!VoteKick) return;
            if (Object.HasStateAuthority) PositiveVotes += 1;
            else RPC_AddPositiveVote();
        }

        public void AddNegativeVote()
        {
            if (!VoteKick) return;
            if (Object.HasStateAuthority) NegativeVotes += 1;
            else RPC_AddNegativeVote();
        }

        public void RemovePlayer()
        {
            if (Object.HasStateAuthority)
                StartCoroutine(RemovePlayerAfterDelay(3f));
        }

        private IEnumerator RemovePlayerAfterDelay(float time)
        {
            yield return new WaitForSeconds(time);
            Runner.Shutdown();
            SceneManager.LoadScene("CreateRoom");
        }

        public void OnPositiveVote(int previous, int current) => OnPositiveVotesChanged?.Invoke(current);
        public void OnNegativeVote(int previous, int current) => OnNegativeVotesChanged?.Invoke(current);

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (SessionPlayers.instance != null)
                SessionPlayers.instance.RemovePlayer(this);

            PlayerHUDManager.Instance?.RefreshHUD();

            if (VoteKick && SessionPlayers.instance != null)
            {
                SessionPlayers.instance.RemoveVoteKick(this);
                RPC_PlayerVoteResultMessage("Vote kick failed");
            }
        }

        [Rpc(sources: RpcSources.Proxies, targets: RpcTargets.StateAuthority)]
        public void RPC_BeginVoteKick()
        {
            VoteKick = true;
            VoteTime = TickTimer.CreateFromSeconds(Runner, maxVoteTime);
        }

        [Rpc(sources: RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_PlayerVoteResultMessage(string message) => InfoPanel.instance.AddMessage(message);

        [Rpc(sources: RpcSources.Proxies, targets: RpcTargets.StateAuthority)]
        public void RPC_AddPositiveVote() => PositiveVotes += 1;

        [Rpc(sources: RpcSources.Proxies, targets: RpcTargets.StateAuthority)]
        public void RPC_AddNegativeVote() => NegativeVotes += 1;

        private void Speaking(bool value) => OnSpeaking?.Invoke(value);

        private void OnTriggerEnter(Collider other)
        {
            if (!Object.HasStateAuthority || HasSpawnProtection) return;
            if (!other.CompareTag("DamageToPlayer")) return;
            if (!DamageCooldownTimer.ExpiredOrNotRunning(Runner)) return;

            switch (CurrentStatus)
            {
                case PlayerStatus.Healthy:
                    ChangeStatus(PlayerStatus.Injured);
                    DamageCooldownTimer = TickTimer.CreateFromSeconds(Runner, damageCooldownDuration);
                    break;
                case PlayerStatus.Injured:
                    ChangeStatus(PlayerStatus.Downed);
                    DamageCooldownTimer = TickTimer.CreateFromSeconds(Runner, damageCooldownDuration);
                    break;
            }
        }
    }
}
#endif
