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

    private bool _initialized;

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

        CancelAllTweens();

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

        LobbyUIAnimations.SetupButtonFeedback(hostButton);

        LobbyUIAnimations.SetupButtonFeedback(joinButton);

        LobbyUIAnimations.SetupButtonFeedback(leaveButton);



        foreach (var button in GetComponentsInChildren<Button>(true))

        {

            LobbyUIAnimations.SetupButtonFeedback(button);

        }

    }



    private void PlayEntrance()

    {

        if (background != null)

        {

            LobbyUIAnimations.AnimateFadeIn(background, 0.65f);

        }



        SetPanelActive(hostPanel, false);

        SetPanelActive(joinPanel, false);



        if (mainPanel != null)

        {

            mainPanel.SetActive(true);

            LobbyUIAnimations.AnimatePanelIn(mainPanel, 0.08f, () =>

            {

                LobbyUIAnimations.StaggerChildrenIn(mainPanel.transform, 0.15f);

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

            bool wasHidden = !leaveButton.gameObject.activeSelf;

            leaveButton.gameObject.SetActive(inRoom);



            if (inRoom && wasHidden)

            {

                LobbyUIAnimations.AnimateReveal(leaveButton.gameObject, 0.1f);

            }

        }



        if (!string.IsNullOrEmpty(roomCode) && roomCode != _lastRoomCode && hostRoomCodeText != null)

        {

            LobbyUIAnimations.AnimatePopText(hostRoomCodeText);

        }



        _lastRoomCode = roomCode ?? string.Empty;

    }



    private void HandleStatusChanged(string message)

    {

        if (statusText != null)

        {

            statusText.text = message;

            LobbyUIAnimations.AnimateStatusPulse(statusText);

        }



        if (hostStatusText != null)

        {

            hostStatusText.text = message;

            LobbyUIAnimations.AnimateStatusPulse(hostStatusText);

        }

    }



    private void HandleRoomError(string message)

    {

        HandleStatusChanged(message);

    }



    public void HideAllPanels()

    {

        CancelAllTweens();

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



        if (hostStatusText != null)

        {

            hostStatusText.text = "กำลังสร้างห้อง...";

            LobbyUIAnimations.AnimateStatusPulse(hostStatusText);

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

        if (!_initialized)

        {

            SetPanelActive(mainPanel, targetPanel == mainPanel);

            SetPanelActive(hostPanel, targetPanel == hostPanel);

            SetPanelActive(joinPanel, targetPanel == joinPanel);

            _activePanel = targetPanel;

            return;

        }



        if (_activePanel == targetPanel)

        {

            return;

        }



        var previous = _activePanel;

        _activePanel = targetPanel;



        LobbyUIAnimations.TransitionPanels(previous, targetPanel, () =>

        {

            if (targetPanel != null)

            {

                LobbyUIAnimations.StaggerChildrenIn(targetPanel.transform, 0.08f);

            }

        });

    }



    private void CancelAllTweens()

    {

        LobbyUIAnimations.Cancel(mainPanel);

        LobbyUIAnimations.Cancel(hostPanel);

        LobbyUIAnimations.Cancel(joinPanel);

        LobbyUIAnimations.Cancel(background);

        LobbyUIAnimations.Cancel(gameObject);

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

