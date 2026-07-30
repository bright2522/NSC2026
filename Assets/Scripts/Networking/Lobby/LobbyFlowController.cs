using System.Collections;
using TMPro;
using UnityEngine;

public class LobbyFlowController : MonoBehaviour
{
    public static LobbyFlowController Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private MultiplayerRoomUI roomUI;
    [SerializeField] private LobbyNameEntryUI nameEntryUI;
    [SerializeField] private HostLobbySidebarUI hostSidebarUI;
    [SerializeField] private GameObject clientWaitingPanel;
    [SerializeField] private TMP_Text clientWaitingText;

    [Header("Timing")]
    [SerializeField] private float lobbyManagerWaitSeconds = 5f;

    private bool _nameSubmitted;
    private bool _lobbyEntered;

    private void Awake()
    {
        Instance = this;

        if (clientWaitingText == null && clientWaitingPanel != null)
        {
            clientWaitingText = clientWaitingPanel.GetComponentInChildren<TMP_Text>();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnEnable()
    {
        SubscribeRoomEvents();
    }

    private void OnDisable()
    {
        UnsubscribeRoomEvents();
    }

    private void Start()
    {
        SubscribeRoomEvents();
        BeginNameEntry();
    }

    private void SubscribeRoomEvents()
    {
        if (MultiplayerRoomService.Instance == null)
        {
            return;
        }

        MultiplayerRoomService.Instance.OnRoomJoined -= HandleRoomJoined;
        MultiplayerRoomService.Instance.OnRoomJoined += HandleRoomJoined;
    }

    private void UnsubscribeRoomEvents()
    {
        if (MultiplayerRoomService.Instance == null)
        {
            return;
        }

        MultiplayerRoomService.Instance.OnRoomJoined -= HandleRoomJoined;
    }

    private void BeginNameEntry()
    {
        _nameSubmitted = false;
        _lobbyEntered = false;

        roomUI?.HideAllPanels();
        hostSidebarUI?.ResetSidebar();
        HideClientWaitingPanel();

        if (nameEntryUI == null)
        {
            nameEntryUI = FindFirstObjectByType<LobbyNameEntryUI>(FindObjectsInactive.Include);
        }

        if (nameEntryUI != null)
        {
            nameEntryUI.Show(OnNameSubmitted);
            return;
        }

        Debug.LogError("[LobbyFlow] NameEntryUI missing — showing MainPanel fallback.");
        roomUI?.ShowMainPanelAfterName("Player");
    }

    private void OnNameSubmitted(string displayName)
    {
        _nameSubmitted = true;

#if CMPSETUP_COMPLETE
        if (FusionLobbyBridge.Instance != null)
        {
            FusionLobbyBridge.Instance.SetDisplayName(displayName);
        }
        else
        {
            PlayerPrefs.SetString("MP_PLAYER_NAME", displayName);
            PlayerPrefs.Save();
        }
#else
        PlayerPrefs.SetString("MP_PLAYER_NAME", displayName);
        PlayerPrefs.Save();
#endif

        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.SubmitDisplayName(displayName);
        }

        roomUI?.ShowMainPanelAfterName(displayName);
    }

    private void HandleRoomJoined()
    {
        if (_lobbyEntered)
        {
            return;
        }

        _lobbyEntered = true;
        StartCoroutine(EnterLobbyPhase());
    }

    private IEnumerator EnterLobbyPhase()
    {
        float elapsed = 0f;
        while (LobbyManager.Instance == null && elapsed < lobbyManagerWaitSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!_nameSubmitted)
        {
            nameEntryUI?.Show(OnNameSubmitted);
            yield break;
        }

        bool isHost = MultiplayerRoomService.Instance != null && MultiplayerRoomService.Instance.IsHost;

        if (isHost)
        {
            hostSidebarUI?.PrepareCenterPhase();
            hostSidebarUI?.BindLobbyManager();
            hostSidebarUI?.ForceShowForHost();
            HideClientWaitingPanel();
            yield break;
        }

        roomUI?.HideAllPanels();
        ShowClientWaitingPanel();
    }

    public void ResetFlow()
    {
        _lobbyEntered = false;
        HideClientWaitingPanel();
        hostSidebarUI?.ResetSidebar();

        if (_nameSubmitted)
        {
            roomUI?.ShowMainPanel();
            return;
        }

        BeginNameEntry();
    }

    private void ShowClientWaitingPanel()
    {
        if (clientWaitingPanel == null)
        {
            return;
        }

        if (clientWaitingText != null)
        {
            clientWaitingText.text = "Waiting for host to start...";
        }

        LobbyUIAnimations.Cancel(clientWaitingPanel);
        LobbyUIAnimations.AnimatePanelIn(clientWaitingPanel, 0.12f, () =>
        {
            if (clientWaitingText != null)
            {
                LobbyUIAnimations.AnimateBreathingPulse(clientWaitingText.gameObject, 1.04f, 1.8f);
            }
        });
    }

    private void HideClientWaitingPanel()
    {
        if (clientWaitingPanel == null)
        {
            return;
        }

        if (clientWaitingText != null)
        {
            LobbyUIAnimations.Cancel(clientWaitingText.gameObject);
        }

        LobbyUIAnimations.ResetPanelTree(clientWaitingPanel);
        clientWaitingPanel.SetActive(false);
    }
}
