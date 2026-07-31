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
    [SerializeField] private Button enterHostRoomButton;

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
    private bool _nameReady;
    private string _pendingHostCode = string.Empty;

    private void Awake()
    {
        CreateRoomSceneBootstrap.EnsureSceneReady();
        HideLegacyCreateRoomPanel();
        EnsurePanelRefs();
    }

    private void HideLegacyCreateRoomPanel()
    {
        var legacy = transform.Find("CreateRoomPanel");
        if (legacy == null && transform.parent != null)
        {
            legacy = transform.parent.Find("CreateRoomPanel");
        }

        if (legacy == null)
        {
            var found = GameObject.Find("CreateRoomPanel");
            if (found != null)
            {
                legacy = found.transform;
            }
        }

        if (legacy != null)
        {
            legacy.gameObject.SetActive(false);
        }
    }

    private void EnsurePanelRefs()
    {
        if (background == null)
        {
            var found = transform.Find("Background");
            if (found == null && transform.parent != null)
            {
                found = transform.parent.Find("Background");
            }

            if (found != null)
            {
                background = found.gameObject;
            }
        }

        if (joinCodeInput == null)
        {
            joinCodeInput = GetComponentInChildren<TMP_InputField>(true);
        }

        if (joinCodeInput != null)
        {
            joinCodeInput.characterLimit = RoomCodeGenerator.CodeLength;
            joinCodeInput.contentType = TMP_InputField.ContentType.Alphanumeric;
            joinCodeInput.lineType = TMP_InputField.LineType.SingleLine;

            if (joinCodeInput.textViewport == null)
            {
                var selfRect = joinCodeInput.GetComponent<RectTransform>();
                joinCodeInput.textViewport = selfRect;
            }

            if (joinCodeInput.textComponent == null)
            {
                var texts = joinCodeInput.GetComponentsInChildren<TMP_Text>(true);
                for (int i = 0; i < texts.Length; i++)
                {
                    if (texts[i].gameObject.name == "Placeholder")
                    {
                        continue;
                    }

                    joinCodeInput.textComponent = texts[i];
                    break;
                }
            }

            if (joinCodeInput.placeholder == null)
            {
                var placeholderTransform = joinCodeInput.transform.Find("Placeholder");
                if (placeholderTransform != null)
                {
                    joinCodeInput.placeholder = placeholderTransform.GetComponent<TMP_Text>();
                }
            }

            if (joinCodeInput.placeholder is TMP_Text placeholder)
            {
                placeholder.text = "Room Code (6)";
            }
        }
    }

    private void OnEnable()
    {
        SubscribeBridgeEvents();
        SubscribeRoomEvents();
    }

    private void OnDisable()
    {
        UnsubscribeBridgeEvents();
        UnsubscribeRoomEvents();
        ResetAllPanels();
    }

    private void Start()
    {
        CreateRoomSceneBootstrap.EnsureSceneReady();
        SubscribeBridgeEvents();
        SubscribeRoomEvents();
        SetupButtonAnimations();
        WireEnterHostButton();
        PlayEntrance();
        HandleStatusChanged("ใส่ชื่อเพื่อเริ่มเล่น");
    }

    private void WireEnterHostButton()
    {
        if (enterHostRoomButton == null && leaveButton != null)
        {
            enterHostRoomButton = leaveButton;
        }

        if (enterHostRoomButton == null)
        {
            return;
        }

        enterHostRoomButton.onClick.RemoveListener(OnClickEnterHostRoom);
        enterHostRoomButton.onClick.AddListener(OnClickEnterHostRoom);
        enterHostRoomButton.gameObject.SetActive(false);

        var label = enterHostRoomButton.GetComponentInChildren<TMP_Text>();
        if (label != null)
        {
            label.text = "ENTER ROOM";
        }
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
        SetPanelActive(mainPanel, false);
        SetPanelActive(hostPanel, false);
        SetPanelActive(joinPanel, false);
        _activePanel = null;
        _initialized = true;

        if (background == null)
        {
            var found = transform.Find("Background");
            if (found != null)
            {
                background = found.gameObject;
            }
        }

        if (background != null)
        {
            background.SetActive(true);
            LobbyUIAnimations.AnimateFadeIn(background, 0.35f);
        }
    }

    public void ShowMainPanelAfterName(string displayName)
    {
        _nameReady = true;
        HandleStatusChanged($"สวัสดี {displayName} — เลือก Host หรือ Join");
        ShowMainPanel();
    }

    private void SubscribeBridgeEvents()
    {
#if CMPSETUP_COMPLETE
        CreateRoomSceneBootstrap.EnsureFusionLobby();
        if (FusionLobbyBridge.Instance == null)
        {
            return;
        }

        FusionLobbyBridge.Instance.OnStatusChanged -= HandleStatusChanged;
        FusionLobbyBridge.Instance.OnRoomError -= HandleRoomError;
        FusionLobbyBridge.Instance.OnStatusChanged += HandleStatusChanged;
        FusionLobbyBridge.Instance.OnRoomError += HandleRoomError;
#endif
    }

    private void UnsubscribeBridgeEvents()
    {
#if CMPSETUP_COMPLETE
        if (FusionLobbyBridge.Instance == null)
        {
            return;
        }

        FusionLobbyBridge.Instance.OnStatusChanged -= HandleStatusChanged;
        FusionLobbyBridge.Instance.OnRoomError -= HandleRoomError;
#endif
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
        if (!_nameReady)
        {
            HandleRoomError("กรุณาใส่ชื่อก่อน");
            return;
        }

#if CMPSETUP_COMPLETE
        CreateRoomSceneBootstrap.EnsureFusionLobby();
        var bridge = FusionLobbyBridge.Instance;
        if (bridge != null)
        {
            bridge.PrepareHostRoom();
            _pendingHostCode = bridge.PendingRoomCode;
            HandleRoomCodeChanged(_pendingHostCode);
            ShowHostPanel();
            if (enterHostRoomButton != null)
            {
                enterHostRoomButton.gameObject.SetActive(true);
            }

            HandleStatusChanged("แชร์รหัสนี้ → กด ENTER ROOM ก่อน → ให้เพื่อน Join ด้วยรหัสเดียวกัน");
            return;
        }

        HandleRoomError("ไม่พบ Fusion lobby — ตรวจ FusionLobbyBridge ในฉาก");
        return;
#else
        CreateRoomSceneBootstrap.EnsureSceneReady();
        var service = CreateRoomSceneBootstrap.EnsureRoomService();
        if (service == null)
        {
            HandleRoomError("ไม่พบระบบสร้างห้อง");
            return;
        }

        ShowHostPanel();
        service.HostRoom();
#endif
    }

    public void OnClickEnterHostRoom()
    {
        if (!_nameReady)
        {
            HandleRoomError("กรุณาใส่ชื่อก่อน");
            return;
        }

#if CMPSETUP_COMPLETE
        CreateRoomSceneBootstrap.EnsureFusionLobby();
        if (FusionLobbyBridge.Instance != null)
        {
            HandleStatusChanged("กำลังเข้าห้อง...");
            FusionLobbyBridge.Instance.EnterHostedRoom();
            return;
        }
#endif

        HandleRoomError("ไม่พบ Fusion lobby");
    }

    public void OnClickJoin()
    {
        if (!_nameReady)
        {
            HandleRoomError("กรุณาใส่ชื่อก่อน");
            return;
        }

        ShowJoinPanel();
    }

    public void OnClickConfirmJoin()
    {
        if (!_nameReady)
        {
            HandleRoomError("กรุณาใส่ชื่อก่อน");
            return;
        }

        if (joinCodeInput == null)
        {
            EnsurePanelRefs();
        }

        string code = joinCodeInput != null ? joinCodeInput.text : string.Empty;
        if (string.IsNullOrWhiteSpace(code))
        {
            HandleRoomError("กรุณาใส่รหัสห้อง 6 ตัว");
            return;
        }

#if CMPSETUP_COMPLETE
        CreateRoomSceneBootstrap.EnsureFusionLobby();
        if (FusionLobbyBridge.Instance != null)
        {
            FusionLobbyBridge.Instance.JoinRoom(code);
            return;
        }

        HandleRoomError("ไม่พบ Fusion lobby — ตรวจ FusionLobbyBridge ในฉาก");
        return;
#else
        CreateRoomSceneBootstrap.EnsureSceneReady();
        var service = CreateRoomSceneBootstrap.EnsureRoomService();
        if (service == null)
        {
            HandleRoomError("ไม่พบระบบเข้าห้อง");
            return;
        }

        service.JoinRoom(code);
#endif
    }

    public void OnClickLeave()
    {
        MultiplayerRoomService.Instance?.LeaveRoom();
        LobbyFlowController.Instance?.ResetFlow();
        if (_nameReady)
        {
            ShowMainPanel();
        }
    }

    public void OnClickBack()
    {
        if (_nameReady)
        {
            ShowMainPanel();
            return;
        }

        HideAllPanels();
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
        if (enterHostRoomButton != null)
        {
            enterHostRoomButton.gameObject.SetActive(inRoom && _nameReady);
        }

        if (!string.IsNullOrEmpty(roomCode) && roomCode != _lastRoomCode && hostRoomCodeText != null)
        {
            LobbyUIAnimations.AnimatePopText(hostRoomCodeText);
        }

        _lastRoomCode = roomCode ?? string.Empty;
        _pendingHostCode = _lastRoomCode;
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
        SetPanelActive(mainPanel, false);
        SetPanelActive(hostPanel, false);
        SetPanelActive(joinPanel, false);
        _activePanel = null;
    }

    public void ShowMainPanel()
    {
        TransitionToPanel(mainPanel);
    }

    private void ShowHostPanel()
    {
        TransitionToPanel(hostPanel);

        if (hostStatusText != null && string.IsNullOrEmpty(hostStatusText.text))
        {
            hostStatusText.text = "แชร์รหัสนี้ให้เพื่อน";
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
            if (targetPanel != null && !targetPanel.activeSelf)
            {
                targetPanel.SetActive(true);
                _activePanel = targetPanel;
            }

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
