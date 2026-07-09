using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerRoomUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject hostPanel;
    [SerializeField] private GameObject joinPanel;
    [SerializeField] private GameObject background;

    [Header("Host")]
    [SerializeField] private TMP_Text hostRoomCodeText;
    [SerializeField] private TMP_Text hostStatusText;
    [SerializeField] private TMP_Text statusText;

    [Header("Join")]
    [SerializeField] private TMP_InputField joinCodeInput;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button leaveButton;

    private GameObject _activePanel;
    private string _lastRoomCode = string.Empty;
    private string _lastStatusMessage = string.Empty;
    private bool _initialized;
    private bool _isTransitioning;

    private void Awake()
    {
        if (background == null)
        {
            var found = transform.parent != null
                ? transform.parent.Find("Background")
                : transform.Find("Background");
            if (found != null)
            {
                background = found.gameObject;
            }
        }
    }

    private void OnEnable()
    {
        SubscribeRoomEvents();
    }

    private void OnDisable()
    {
        UnsubscribeRoomEvents();
        ResetAllPanels();
    }

    private void Start()
    {
        SubscribeRoomEvents();
        SetupButtonAnimations();
        PlayEntrance();
        HandleRoomCodeChanged(MultiplayerRoomService.Instance?.CurrentRoomCode ?? string.Empty);
        HandleStatusChanged("พร้อมเล่น multiplayer");
    }

    private void SetupButtonAnimations()
    {
        foreach (var button in GetComponentsInChildren<Button>(true))
        {
            LobbyUIAnimations.SetupButtonFeedback(button);
        }
    }

    private void PlayEntrance()
    {
        ResetAllPanels();
        SetPanelActive(hostPanel, false);
        SetPanelActive(joinPanel, false);

        if (background != null)
        {
            LobbyUIAnimations.AnimateFadeIn(background, 0.35f);
        }

        if (mainPanel != null)
        {
            mainPanel.SetActive(true);
            LobbyUIAnimations.AnimatePanelIn(mainPanel, 0.05f, () =>
            {
                _activePanel = mainPanel;
                _initialized = true;
            });
        }
    }

    private void SubscribeRoomEvents()
    {
        if (MultiplayerRoomService.Instance == null)
        {
            return;
        }

        MultiplayerRoomService.Instance.OnRoomCodeChanged -= HandleRoomCodeChanged;
        MultiplayerRoomService.Instance.OnStatusChanged -= HandleStatusChanged;
        MultiplayerRoomService.Instance.OnRoomError -= HandleRoomError;
        MultiplayerRoomService.Instance.OnRoomCodeChanged += HandleRoomCodeChanged;
        MultiplayerRoomService.Instance.OnStatusChanged += HandleStatusChanged;
        MultiplayerRoomService.Instance.OnRoomError += HandleRoomError;
    }

    private void UnsubscribeRoomEvents()
    {
        if (MultiplayerRoomService.Instance == null)
        {
            return;
        }

        MultiplayerRoomService.Instance.OnRoomCodeChanged -= HandleRoomCodeChanged;
        MultiplayerRoomService.Instance.OnStatusChanged -= HandleStatusChanged;
        MultiplayerRoomService.Instance.OnRoomError -= HandleRoomError;
    }

    public void OnClickHost()
    {
        ShowHostPanel();
        MultiplayerRoomService.Instance?.HostRoom();
    }

    public void OnClickJoin()
    {
        ShowJoinPanel();
    }

    public void OnClickConfirmJoin()
    {
        MultiplayerRoomService.Instance?.JoinRoom(joinCodeInput.text);
    }

    public void OnClickLeave()
    {
        MultiplayerRoomService.Instance?.LeaveRoom();
        LobbyFlowController.Instance?.ResetFlow();
        ShowMainPanel();
    }

    public void OnClickBack()
    {
        ShowMainPanel();
    }

    private void HandleRoomCodeChanged(string roomCode)
    {
        if (hostRoomCodeText != null)
        {
            hostRoomCodeText.text = string.IsNullOrEmpty(roomCode)
                ? "------"
                : FormatRoomCode(roomCode);
        }

        bool inRoom = !string.IsNullOrEmpty(roomCode);
        if (leaveButton != null)
        {
            leaveButton.gameObject.SetActive(inRoom);
        }

        if (!string.IsNullOrEmpty(roomCode) && roomCode != _lastRoomCode && hostRoomCodeText != null)
        {
            LobbyUIAnimations.AnimatePopText(hostRoomCodeText);
        }

        _lastRoomCode = roomCode ?? string.Empty;
    }

    private void HandleStatusChanged(string message)
    {
        bool shouldPulse = message != _lastStatusMessage;

        if (statusText != null)
        {
            statusText.text = message;
            if (shouldPulse)
            {
                LobbyUIAnimations.AnimateStatusPulse(statusText);
            }
        }

        if (hostStatusText != null)
        {
            hostStatusText.text = message;
        }

        _lastStatusMessage = message;
    }

    private void HandleRoomError(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
            LobbyUIAnimations.AnimateStatusPulse(statusText);
        }

        if (hostStatusText != null)
        {
            hostStatusText.text = message;
        }

        _lastStatusMessage = message;
    }

    public void HideAllPanels()
    {
        _isTransitioning = false;
        ResetAllPanels();
        _activePanel = null;
    }

    public void ShowMainPanel()
    {
        TransitionToPanel(mainPanel);
    }

    private void ShowHostPanel()
    {
        TransitionToPanel(hostPanel);

        if (hostStatusText != null)
        {
            hostStatusText.text = "กำลังสร้างห้อง...";
        }
    }

    private void ShowJoinPanel()
    {
        TransitionToPanel(joinPanel);

        if (joinCodeInput != null)
        {
            joinCodeInput.characterLimit = RoomCodeGenerator.CodeLength;
            joinCodeInput.contentType = TMP_InputField.ContentType.Alphanumeric;
        }
    }

    private void TransitionToPanel(GameObject targetPanel)
    {
        if (!_initialized || targetPanel == null)
        {
            SetPanelActive(mainPanel, targetPanel == mainPanel);
            SetPanelActive(hostPanel, targetPanel == hostPanel);
            SetPanelActive(joinPanel, targetPanel == joinPanel);
            _activePanel = targetPanel;
            return;
        }

        if (_isTransitioning || _activePanel == targetPanel)
        {
            return;
        }

        _isTransitioning = true;
        var previous = _activePanel;
        _activePanel = targetPanel;

        LobbyUIAnimations.TransitionPanels(previous, targetPanel, () => _isTransitioning = false);
    }

    private void ResetAllPanels()
    {
        LobbyUIAnimations.ResetPanelTree(mainPanel);
        LobbyUIAnimations.ResetPanelTree(hostPanel);
        LobbyUIAnimations.ResetPanelTree(joinPanel);
        LobbyUIAnimations.CancelAndReset(background);
    }

    private static void SetPanelActive(GameObject panel, bool isActive)
    {
        if (panel != null)
        {
            panel.SetActive(isActive);
        }
    }

    private static string FormatRoomCode(string roomCode)
    {
        if (roomCode.Length != RoomCodeGenerator.CodeLength)
        {
            return roomCode;
        }

        return $"{roomCode[..3]} {roomCode[3..]}";
    }
}
