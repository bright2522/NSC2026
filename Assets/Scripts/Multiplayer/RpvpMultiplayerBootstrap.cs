#if CMPSETUP_COMPLETE
using AvocadoShark;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RpvpMultiplayerBootstrap : MonoBehaviour
{
    [Header("Optional refs (auto-created if null)")]
    [SerializeField] private MultiplayerGameManager gameManager;
    [SerializeField] private CompetitionMatchController competitionMatchController;

    private void Awake()
    {
        EnsureSessionPlayers();
        EnsurePlayerHudManager();
        EnsureLobbyUi();
        WireMultiplayerGameManager();
    }

    private void EnsureSessionPlayers()
    {
        if (SessionPlayers.instance != null)
            return;

        var go = new GameObject("SessionPlayers");
        go.AddComponent<SessionPlayers>();
    }

    private void EnsurePlayerHudManager()
    {
        if (PlayerHUDManager.Instance != null)
            return;

        var go = new GameObject("PlayerHUDManager");
        var hud = go.AddComponent<AvocadoShark.PlayerHUDManager>();

        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
            return;

        var container = new GameObject("PlayerHUDContainer");
        container.transform.SetParent(canvas.transform, false);
        var rt = container.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(20f, -20f);
        rt.sizeDelta = new Vector2(280f, 400f);
        var layout = container.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 8;
        layout.childControlHeight = false;
        layout.childControlWidth = true;

        var prefab = CreatePlayerUiPrefab();
        prefab.transform.SetParent(go.transform, false);
        prefab.SetActive(false);
        hud.Configure(prefab, container.transform);
    }

    private static GameObject CreatePlayerUiPrefab()
    {
        var root = new GameObject("PlayerUIItemPrefab");
        var rt = root.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(260f, 36f);
        root.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.35f);

        var nameGo = new GameObject("Name");
        nameGo.transform.SetParent(root.transform, false);
        var nameRt = nameGo.AddComponent<RectTransform>();
        nameRt.anchorMin = Vector2.zero;
        nameRt.anchorMax = Vector2.one;
        nameRt.offsetMin = new Vector2(10f, 0f);
        nameRt.offsetMax = new Vector2(-10f, 0f);
        var nameText = nameGo.AddComponent<TextMeshProUGUI>();
        nameText.fontSize = 16;
        nameText.color = Color.white;

        var roleGo = new GameObject("Role");
        roleGo.transform.SetParent(root.transform, false);
        var roleRt = roleGo.AddComponent<RectTransform>();
        roleRt.anchorMin = new Vector2(1f, 0.5f);
        roleRt.anchorMax = new Vector2(1f, 0.5f);
        roleRt.sizeDelta = new Vector2(80f, 24f);
        roleRt.anchoredPosition = new Vector2(-50f, 0f);
        var roleText = roleGo.AddComponent<TextMeshProUGUI>();
        roleText.fontSize = 14;
        roleText.alignment = TextAlignmentOptions.Right;
        roleText.color = new Color(0.8f, 0.9f, 1f);

        var item = root.AddComponent<AvocadoShark.PlayerUIItem>();
        item.Configure(nameText, roleText);

        return root;
    }

    private void EnsureLobbyUi()
    {
        if (FindFirstObjectByType<ReadyButtonUI>() != null)
            return;

        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
            return;

        var panel = new GameObject("MultiplayerLobbyUI");
        panel.transform.SetParent(canvas.transform, false);
        var panelRt = panel.AddComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0f);
        panelRt.anchorMax = new Vector2(0.5f, 0f);
        panelRt.pivot = new Vector2(0.5f, 0f);
        panelRt.anchoredPosition = new Vector2(0f, 40f);
        panelRt.sizeDelta = new Vector2(420f, 180f);

        var gameState = CreateLabel(panel.transform, "GameStateText", new Vector2(0f, 140f), 20);
        var playerCount = CreateLabel(panel.transform, "PlayerCountText", new Vector2(0f, 110f), 18);
        var readyCount = CreateLabel(panel.transform, "ReadyCountText", new Vector2(0f, 80f), 18);
        var timer = CreateLabel(panel.transform, "TimerText", new Vector2(0f, 50f), 18);

        var readyBtnGo = new GameObject("ReadyButton");
        readyBtnGo.transform.SetParent(panel.transform, false);
        var btnRt = readyBtnGo.AddComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.5f, 0f);
        btnRt.anchorMax = new Vector2(0.5f, 0f);
        btnRt.sizeDelta = new Vector2(200f, 44f);
        btnRt.anchoredPosition = new Vector2(0f, 10f);
        readyBtnGo.AddComponent<Image>().color = new Color(0.15f, 0.55f, 0.25f, 1f);
        var btn = readyBtnGo.AddComponent<Button>();
        var btnLabelGo = new GameObject("Text");
        btnLabelGo.transform.SetParent(readyBtnGo.transform, false);
        var btnLabelRt = btnLabelGo.AddComponent<RectTransform>();
        btnLabelRt.anchorMin = Vector2.zero;
        btnLabelRt.anchorMax = Vector2.one;
        btnLabelRt.offsetMin = Vector2.zero;
        btnLabelRt.offsetMax = Vector2.zero;
        var btnLabel = btnLabelGo.AddComponent<TextMeshProUGUI>();
        btnLabel.text = "Ready";
        btnLabel.alignment = TextAlignmentOptions.Center;
        btnLabel.color = Color.white;
        btnLabel.fontSize = 20;

        var readyUi = readyBtnGo.AddComponent<ReadyButtonUI>();
        readyUi.Configure(btn);

        if (gameManager == null)
            gameManager = FindFirstObjectByType<MultiplayerGameManager>();

        if (gameManager != null)
        {
            gameManager.gameStateText = gameState;
            gameManager.playerCountText = playerCount;
            gameManager.readyCountText = readyCount;
            gameManager.timerText = timer;
        }
    }

    private static TMP_Text CreateLabel(Transform parent, string name, Vector2 pos, float fontSize)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.sizeDelta = new Vector2(400f, 28f);
        rt.anchoredPosition = pos;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        return tmp;
    }

    private void WireMultiplayerGameManager()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<MultiplayerGameManager>();

        if (competitionMatchController == null)
            competitionMatchController = FindFirstObjectByType<CompetitionMatchController>();

        if (gameManager != null && competitionMatchController != null)
            gameManager.SetCompetitionController(competitionMatchController);
    }
}
#endif
