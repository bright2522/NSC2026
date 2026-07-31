#if CMPSETUP_COMPLETE
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FusionLobbyWaitingUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject waitingPanel;
    [SerializeField] private GameObject loadingOverlay;
    [SerializeField] private GameObject connectingOverlay;
    [SerializeField] private RectTransform spinner;

    [Header("Texts")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text roomCodeText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text playerListText;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private TMP_Text connectingText;

    [Header("Buttons")]
    [SerializeField] private Button readyButton;
    [SerializeField] private Button startButton;
    [SerializeField] private Button backButton;
    [SerializeField] private TMP_Text readyButtonLabel;
    [SerializeField] private TMP_Text startButtonLabel;

    [Header("Font")]
    [SerializeField] private TMP_FontAsset uiFont;

    private MultiplayerRoomUI _roomUI;
    private bool _wired;
    private bool _spinnerRunning;

    private void Awake()
    {
        EnsureRefs();
        ApplyUiFont();
        WireButtons();
        HideAll();
    }

    private void OnEnable()
    {
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
        StopSpinner();
    }

    private void Update()
    {
        if (_spinnerRunning && spinner != null)
        {
            spinner.Rotate(0f, 0f, -360f * Time.unscaledDeltaTime);
        }
    }

    public void Configure(MultiplayerRoomUI roomUI)
    {
        _roomUI = roomUI;
    }

    private void EnsureRefs()
    {
        if (waitingPanel == null)
        {
            waitingPanel = gameObject;
        }

        if (readyButtonLabel == null && readyButton != null)
        {
            readyButtonLabel = readyButton.GetComponentInChildren<TMP_Text>(true);
        }

        if (startButtonLabel == null && startButton != null)
        {
            startButtonLabel = startButton.GetComponentInChildren<TMP_Text>(true);
        }

        if (uiFont == null)
        {
            uiFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        }
    }

    private void ApplyUiFont()
    {
        if (uiFont == null)
        {
            return;
        }

        foreach (var tmp in GetComponentsInChildren<TMP_Text>(true))
        {
            tmp.font = uiFont;
        }
    }

    private void WireButtons()
    {
        if (_wired)
        {
            return;
        }

        _wired = true;

        if (readyButton != null)
        {
            readyButton.onClick.RemoveListener(OnClickReady);
            readyButton.onClick.AddListener(OnClickReady);
            LobbyUIAnimations.SetupButtonFeedback(readyButton);
        }

        if (startButton != null)
        {
            startButton.onClick.RemoveListener(OnClickStart);
            startButton.onClick.AddListener(OnClickStart);
            LobbyUIAnimations.SetupButtonFeedback(startButton);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveListener(OnClickBack);
            backButton.onClick.AddListener(OnClickBack);
            LobbyUIAnimations.SetupButtonFeedback(backButton);
        }
    }

    private void Subscribe()
    {
        CreateRoomSceneBootstrap.EnsureFusionLobby();
        if (FusionLobbyBridge.Instance == null)
        {
            return;
        }

        var bridge = FusionLobbyBridge.Instance;
        bridge.OnLobbyEntered -= HandleLobbyEntered;
        bridge.OnLobbyRosterChanged -= Refresh;
        bridge.OnStatusChanged -= HandleStatus;
        bridge.OnRoomError -= HandleRoomError;
        bridge.OnGameStarting -= HandleGameStarting;
        bridge.OnConnecting -= HandleConnecting;
        bridge.OnLobbyLeft -= HandleLobbyLeft;

        bridge.OnLobbyEntered += HandleLobbyEntered;
        bridge.OnLobbyRosterChanged += Refresh;
        bridge.OnStatusChanged += HandleStatus;
        bridge.OnRoomError += HandleRoomError;
        bridge.OnGameStarting += HandleGameStarting;
        bridge.OnConnecting += HandleConnecting;
        bridge.OnLobbyLeft += HandleLobbyLeft;
    }

    private void Unsubscribe()
    {
        if (FusionLobbyBridge.Instance == null)
        {
            return;
        }

        var bridge = FusionLobbyBridge.Instance;
        bridge.OnLobbyEntered -= HandleLobbyEntered;
        bridge.OnLobbyRosterChanged -= Refresh;
        bridge.OnStatusChanged -= HandleStatus;
        bridge.OnRoomError -= HandleRoomError;
        bridge.OnGameStarting -= HandleGameStarting;
        bridge.OnConnecting -= HandleConnecting;
        bridge.OnLobbyLeft -= HandleLobbyLeft;
    }

    private void HandleConnecting()
    {
        _roomUI?.HideAllPanels();
        ShowConnecting("Creating room...");
    }

    private void HandleLobbyEntered()
    {
        _roomUI?.HideAllPanels();
        ShowWaiting();
        Refresh();
    }

    private void HandleLobbyLeft()
    {
        HideAll();
        if (_roomUI != null)
        {
            _roomUI.ShowMainPanel();
        }
    }

    private void HandleGameStarting()
    {
        ShowLoading("Loading game...");
    }

    private void HandleStatus(string message)
    {
        if (statusText != null && (connectingOverlay == null || !connectingOverlay.activeSelf))
        {
            statusText.text = message;
        }

        if (connectingText != null && connectingOverlay != null && connectingOverlay.activeSelf)
        {
            connectingText.text = message;
        }
    }

    private void HandleRoomError(string message)
    {
        StopSpinner();
        if (connectingOverlay != null)
        {
            connectingOverlay.SetActive(false);
        }

        if (statusText != null)
        {
            statusText.text = message;
        }

        if (FusionLobbyBridge.Instance != null && !FusionLobbyBridge.Instance.IsInSession)
        {
            HideAll();
            _roomUI?.ShowMainPanel();
            _roomUI?.HandleExternalStatus(message);
        }
    }

    public void ShowConnecting(string message)
    {
        ApplyUiFont();

        if (waitingPanel != null)
        {
            waitingPanel.SetActive(true);
        }

        if (loadingOverlay != null)
        {
            loadingOverlay.SetActive(false);
        }

        if (connectingOverlay != null)
        {
            connectingOverlay.SetActive(true);
        }

        if (connectingText != null)
        {
            connectingText.text = message;
        }

        if (backButton != null)
        {
            backButton.gameObject.SetActive(false);
        }

        StartSpinner();
    }

    public void ShowWaiting()
    {
        ApplyUiFont();
        StopSpinner();

        if (waitingPanel != null)
        {
            waitingPanel.SetActive(true);
            LobbyUIAnimations.AnimatePanelIn(waitingPanel, 0.05f);
        }

        if (loadingOverlay != null)
        {
            loadingOverlay.SetActive(false);
        }

        if (connectingOverlay != null)
        {
            connectingOverlay.SetActive(false);
        }

        if (backButton != null)
        {
            backButton.gameObject.SetActive(true);
        }
    }

    public void HideAll()
    {
        StopSpinner();

        if (waitingPanel != null)
        {
            LobbyUIAnimations.ResetPanelTree(waitingPanel);
            waitingPanel.SetActive(false);
        }

        if (loadingOverlay != null)
        {
            loadingOverlay.SetActive(false);
        }

        if (connectingOverlay != null)
        {
            connectingOverlay.SetActive(false);
        }
    }

    private void ShowLoading(string message)
    {
        if (connectingOverlay != null)
        {
            connectingOverlay.SetActive(false);
        }

        if (loadingOverlay != null)
        {
            loadingOverlay.SetActive(true);
            LobbyUIAnimations.AnimateFadeIn(loadingOverlay, 0.2f);
        }

        if (loadingText != null)
        {
            loadingText.text = message;
            LobbyUIAnimations.AnimateBreathingPulse(loadingText.gameObject, 1.04f, 1.6f);
        }

        if (statusText != null)
        {
            statusText.text = message;
        }

        if (backButton != null)
        {
            backButton.gameObject.SetActive(false);
        }
    }

    private void StartSpinner()
    {
        _spinnerRunning = spinner != null;
        if (spinner != null)
        {
            spinner.gameObject.SetActive(true);
        }
    }

    private void StopSpinner()
    {
        _spinnerRunning = false;
    }

    private void Refresh()
    {
        var bridge = FusionLobbyBridge.Instance;
        if (bridge == null || !bridge.IsInSession)
        {
            return;
        }

        var roster = bridge.GetRoster();
        int count = roster.Count;
        int need = bridge.MinPlayersToStart;

        if (titleText != null)
        {
            titleText.text = bridge.IsHost ? "You are Host" : "You joined the room";
        }

        if (roomCodeText != null)
        {
            string code = bridge.PendingRoomCode;
            roomCodeText.text = string.IsNullOrEmpty(code)
                ? "------"
                : (code.Length == 6 ? $"{code[..3]} {code[3..]}" : code);
        }

        if (playerListText != null)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Players in room ({count})");
            for (int i = 0; i < roster.Count; i++)
            {
                var entry = roster[i];
                string ready = entry.IsReady ? "Ready" : "Waiting";
                string you = entry.IsLocal ? " (You)" : string.Empty;
                sb.AppendLine($"- {entry.DisplayName}{you} - {ready}");
            }

            playerListText.text = sb.ToString().TrimEnd();
        }

        if (statusText != null)
        {
            if (bridge.IsStartingGame)
            {
                statusText.text = "Starting game...";
            }
            else if (count < need)
            {
                statusText.text = bridge.IsHost
                    ? $"Share this code. Waiting for players ({count}/{need})"
                    : $"Joined. Waiting for players ({count}/{need})";
            }
            else if (bridge.IsHost)
            {
                statusText.text = "Share code with friends, or press Start Game";
            }
            else if (!bridge.AreAllPlayersReady())
            {
                statusText.text = "Press Ready when you are ready";
            }
            else
            {
                statusText.text = "Everyone is ready!";
            }
        }

        if (readyButtonLabel != null)
        {
            readyButtonLabel.text = bridge.LocalReady ? "Cancel Ready" : "Ready";
        }

        if (readyButton != null)
        {
            readyButton.interactable = !bridge.IsStartingGame;
        }

        bool canHostStart = bridge.IsHost
                            && !bridge.IsStartingGame
                            && count >= need;

        if (startButton != null)
        {
            startButton.gameObject.SetActive(bridge.IsHost);
            startButton.interactable = canHostStart;
        }

        if (startButtonLabel != null)
        {
            startButtonLabel.text = "Start Game";
        }

        if (backButton != null)
        {
            backButton.gameObject.SetActive(!bridge.IsStartingGame);
            backButton.interactable = !bridge.IsStartingGame;
        }
    }

    private void OnClickReady()
    {
        FusionLobbyBridge.Instance?.ToggleLocalReady();
        Refresh();
    }

    private void OnClickStart()
    {
        FusionLobbyBridge.Instance?.HostStartGame();
        Refresh();
    }

    private void OnClickBack()
    {
        FusionLobbyBridge.Instance?.LeaveLobby();
    }
}
#endif
