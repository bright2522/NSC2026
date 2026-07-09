using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

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
        if (MultiplayerRoomService.Instance != null)
        {
            MultiplayerRoomService.Instance.OnRoomJoined += HandleRoomJoined;
        }
    }

    private void OnDisable()
    {
        if (MultiplayerRoomService.Instance != null)
        {
            MultiplayerRoomService.Instance.OnRoomJoined -= HandleRoomJoined;
        }
    }

    private void Start()
    {
        if (MultiplayerRoomService.Instance != null)
        {
            MultiplayerRoomService.Instance.OnRoomJoined += HandleRoomJoined;
        }

        HideLobbyPhase();
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

        bool isHost = MultiplayerRoomService.Instance != null && MultiplayerRoomService.Instance.IsHost;

        if (isHost)
        {
            hostSidebarUI?.PrepareCenterPhase();
        }
        else
        {
            roomUI?.HideAllPanels();
        }

        nameEntryUI?.Show(OnNameSubmitted);
    }

    private void OnNameSubmitted(string displayName)
    {
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.SubmitDisplayName(displayName);
        }

        bool isHost = MultiplayerRoomService.Instance != null && MultiplayerRoomService.Instance.IsHost;
        if (isHost)
        {
            hostSidebarUI?.BindLobbyManager();
            HideClientWaitingPanel();
            return;
        }

        roomUI?.HideAllPanels();
        ShowClientWaitingPanel();
    }

    public void ResetFlow()
    {
        _lobbyEntered = false;
        HideLobbyPhase();
        roomUI?.ShowMainPanel();
    }

    private void HideLobbyPhase()
    {
        nameEntryUI?.Hide();
        hostSidebarUI?.ResetSidebar();
        HideClientWaitingPanel();
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

        LobbyUIAnimations.Cancel(clientWaitingPanel);
        clientWaitingPanel.SetActive(false);
    }
}
