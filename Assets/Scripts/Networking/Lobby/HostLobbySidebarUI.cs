using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HostLobbySidebarUI : MonoBehaviour
{
    [Header("Layout")]
    [SerializeField] private RectTransform centerPanel;
    [SerializeField] private RectTransform sidebarPanel;
    [SerializeField] private RectTransform playerListRoot;
    [SerializeField] private GameObject playerRowPrefab;
    [SerializeField] private TMP_Text overflowText;
    [SerializeField] private Button startButton;
    [SerializeField] private TMP_Text sidebarTitleText;

    [Header("Animation")]
    [SerializeField] private float sidebarHiddenX = 420f;
    [SerializeField] private float sidebarVisibleX = 0f;
    [SerializeField] private float sidebarSlideDuration = 0.65f;
    [SerializeField] private LeanTweenType sidebarEase = LeanTweenType.easeOutBack;

    [Header("Display")]
    [SerializeField] private int maxVisibleRows = 4;

    private readonly List<GameObject> _spawnedRows = new List<GameObject>();
    private bool _sidebarVisible;
    private bool _isBound;

    private void Awake()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(HandleStartClicked);
            LobbyUIAnimations.SetupButtonFeedback(startButton);
        }

        ResetSidebar();
    }

    private void OnDisable()
    {
        UnbindLobbyManager();
    }

    public void PrepareCenterPhase()
    {
        ResetSidebar();

        if (sidebarPanel != null)
        {
            sidebarPanel.gameObject.SetActive(true);
            var pos = sidebarPanel.anchoredPosition;
            pos.x = sidebarHiddenX;
            sidebarPanel.anchoredPosition = pos;
            sidebarPanel.localScale = Vector3.one * 0.96f;
        }

        if (centerPanel != null)
        {
            LobbyUIAnimations.CancelAndReset(centerPanel.gameObject);
            centerPanel.anchoredPosition = Vector2.zero;
            centerPanel.localScale = Vector3.one;
        }

        if (startButton != null)
        {
            startButton.gameObject.SetActive(true);
            startButton.interactable = false;
        }
    }

    public void BindLobbyManager()
    {
        if (_isBound || LobbyManager.Instance == null)
        {
            return;
        }

        _isBound = true;
        LobbyManager.Instance.OnPlayersChanged += HandlePlayersChanged;
        HandlePlayersChanged(LobbyManager.Instance.GetPlayers());
    }

    public void ForceShowForHost()
    {
        if (startButton != null)
        {
            startButton.gameObject.SetActive(true);
            startButton.interactable = true;
        }

        if (!_sidebarVisible)
        {
            ShowSidebarAnimated();
        }

        if (LobbyManager.Instance != null)
        {
            HandlePlayersChanged(LobbyManager.Instance.GetPlayers());
        }
    }

    public void ResetSidebar()
    {
        UnbindLobbyManager();
        _sidebarVisible = false;
        ClearRows();

        if (sidebarPanel != null)
        {
            LobbyUIAnimations.Cancel(sidebarPanel.gameObject);
            sidebarPanel.gameObject.SetActive(false);
        }

        if (overflowText != null)
        {
            overflowText.gameObject.SetActive(false);
        }
    }

    private void UnbindLobbyManager()
    {
        if (!_isBound || LobbyManager.Instance == null)
        {
            _isBound = false;
            return;
        }

        LobbyManager.Instance.OnPlayersChanged -= HandlePlayersChanged;
        _isBound = false;
    }

    private void HandlePlayersChanged(IReadOnlyList<LobbyPlayerState> players)
    {
        RefreshPlayerRows(players);

        int namedPlayers = CountNamedPlayers(players);
        if (!_sidebarVisible && namedPlayers > 0)
        {
            ShowSidebarAnimated();
        }

        if (startButton != null)
        {
            startButton.interactable = namedPlayers > 0;
        }
    }

    private int CountNamedPlayers(IReadOnlyList<LobbyPlayerState> players)
    {
        int count = 0;
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].HasSubmittedName)
            {
                count++;
            }
        }

        return count;
    }

    private void RefreshPlayerRows(IReadOnlyList<LobbyPlayerState> players)
    {
        ClearRows();

        int visibleCount = Mathf.Min(players.Count, maxVisibleRows);
        int overflowCount = Mathf.Max(0, players.Count - maxVisibleRows);

        for (int i = 0; i < visibleCount; i++)
        {
            GameObject row = playerRowPrefab != null
                ? Instantiate(playerRowPrefab, playerListRoot)
                : CreateRuntimePlayerRow(playerListRoot);
            _spawnedRows.Add(row);

            var label = row.GetComponentInChildren<TMP_Text>();
            if (label != null)
            {
                var player = players[i];
                string prefix = player.ClientId == NetworkManager.ServerClientId ? "[Host] " : string.Empty;
                label.text = player.HasSubmittedName
                    ? $"{prefix}{player.DisplayName}"
                    : $"{prefix}Connecting...";
            }

            var kickButton = row.GetComponentInChildren<Button>();
            ulong clientId = players[i].ClientId;
            bool isHostRow = clientId == NetworkManager.ServerClientId;

            if (kickButton != null)
            {
                kickButton.gameObject.SetActive(!isHostRow && players[i].HasSubmittedName);
                kickButton.onClick.RemoveAllListeners();
                kickButton.onClick.AddListener(() => LobbyManager.Instance?.KickPlayer(clientId));
            }

            AnimateRowIn(row, i);
        }

        if (overflowText != null)
        {
            overflowText.gameObject.SetActive(overflowCount > 0);
            overflowText.text = overflowCount > 0 ? $"... +{overflowCount}" : string.Empty;
        }
    }

    private void ShowSidebarAnimated()
    {
        if (sidebarPanel == null)
        {
            return;
        }

        _sidebarVisible = true;
        sidebarPanel.gameObject.SetActive(true);
        LobbyUIAnimations.Cancel(sidebarPanel.gameObject);

        if (centerPanel != null)
        {
            LobbyUIAnimations.Cancel(centerPanel.gameObject);
            LeanTween.move(centerPanel, new Vector3(-120f, 0f, 0f), sidebarSlideDuration)
                .setEase(sidebarEase);
            LeanTween.scale(centerPanel, Vector3.one * 0.94f, sidebarSlideDuration)
                .setEase(LeanTweenType.easeOutCubic);
        }

        LeanTween.moveX(sidebarPanel, sidebarVisibleX, sidebarSlideDuration)
            .setEase(sidebarEase);
        LeanTween.scale(sidebarPanel, Vector3.one, sidebarSlideDuration)
            .setEase(LeanTweenType.easeOutCubic);

        if (sidebarTitleText != null)
        {
            sidebarTitleText.text = "Players";
        }
    }

    private static void AnimateRowIn(GameObject row, int index)
    {
        var rect = row.GetComponent<RectTransform>();
        if (rect == null)
        {
            return;
        }

        LobbyUIAnimations.AnimateElementIn(row, 0.08f * index);
    }

    private void ClearRows()
    {
        for (int i = 0; i < _spawnedRows.Count; i++)
        {
            if (_spawnedRows[i] != null)
            {
                LobbyUIAnimations.Cancel(_spawnedRows[i]);
                Destroy(_spawnedRows[i]);
            }
        }

        _spawnedRows.Clear();
    }

    private static GameObject CreateRuntimePlayerRow(Transform parent)
    {
        var row = new GameObject("PlayerRow", typeof(RectTransform), typeof(CanvasGroup), typeof(HorizontalLayoutGroup));
        row.transform.SetParent(parent, false);

        var layout = row.GetComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.spacing = 8f;
        layout.padding = new RectOffset(8, 8, 6, 6);
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        var rect = row.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0f, 48f);

        var labelGo = new GameObject("Name", typeof(RectTransform));
        labelGo.transform.SetParent(row.transform, false);
        var label = labelGo.AddComponent<TextMeshProUGUI>();
        label.fontSize = 20f;
        label.color = new Color(0.04f, 0.04f, 0.04f);

        var kickGo = new GameObject("KickButton", typeof(RectTransform), typeof(Image), typeof(Button));
        kickGo.transform.SetParent(row.transform, false);
        var kickRect = kickGo.GetComponent<RectTransform>();
        kickRect.sizeDelta = new Vector2(72f, 36f);
        kickGo.GetComponent<Image>().color = new Color(0.93f, 0.42f, 0.35f);
        var kickLabelGo = new GameObject("Text", typeof(RectTransform));
        kickLabelGo.transform.SetParent(kickGo.transform, false);
        var kickLabel = kickLabelGo.AddComponent<TextMeshProUGUI>();
        kickLabel.text = "KICK";
        kickLabel.fontSize = 14f;
        kickLabel.alignment = TextAlignmentOptions.Center;
        kickLabel.color = Color.white;
        var kickLabelRect = kickLabel.GetComponent<RectTransform>();
        kickLabelRect.anchorMin = Vector2.zero;
        kickLabelRect.anchorMax = Vector2.one;
        kickLabelRect.offsetMin = Vector2.zero;
        kickLabelRect.offsetMax = Vector2.zero;

        return row;
    }

    private void HandleStartClicked()
    {
        LobbyManager.Instance?.StartGame();
    }
}
