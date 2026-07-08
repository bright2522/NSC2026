using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameplayHUD : MonoBehaviour
{
    [SerializeField] private bool createUiOnStart = true;
    [Header("Score Animation")]
    [SerializeField] private float scoreCountDuration = 0.35f;
    [SerializeField] private float scorePopScale = 1.1f;
    [SerializeField] private float scorePopDuration = 0.3f;

    private bool uiBuilt;
    private Canvas rootCanvas;
    private RectTransform panelRoot;
    private TextMeshProUGUI scoreText;
    private RectTransform scoreRect;
    private TextMeshProUGUI timerText;
    private Image timerFill;
    private int displayedScore;
    private static readonly Color ScoreBaseColor = Color.white;
    private static readonly Color ScorePopColor = new Color(1f, 0.94f, 0.55f);

    void Start()
    {
        if (createUiOnStart && !uiBuilt)
            BuildRuntimeUi();
    }

    void OnEnable()
    {
        if (GameplayScore.Instance != null)
            GameplayScore.Instance.OnScoreChanged += HandleScoreChanged;
        if (GameplayTimer.Instance != null)
            GameplayTimer.Instance.OnTimerChanged += HandleTimerChanged;

        if (uiBuilt)
        {
            HandleScoreChanged(GameplayScore.Instance != null ? GameplayScore.Instance.CurrentScore : 0);
            HandleTimerChanged(GameplayTimer.Instance != null ? GameplayTimer.Instance.RemainingTime : 0f);
        }
    }

    void OnDisable()
    {
        if (GameplayScore.Instance != null)
            GameplayScore.Instance.OnScoreChanged -= HandleScoreChanged;
        if (GameplayTimer.Instance != null)
            GameplayTimer.Instance.OnTimerChanged -= HandleTimerChanged;

        if (scoreText != null)
            LeanTween.cancel(scoreText.gameObject);
    }

    public void BuildRuntimeUi()
    {
        if (uiBuilt) return;

        rootCanvas = FindFirstObjectByType<Canvas>();
        if (rootCanvas == null)
        {
            var canvasGo = new GameObject("GameplayHUDCanvas",
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            rootCanvas = canvasGo.GetComponent<Canvas>();
            rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            rootCanvas.sortingOrder = 100;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
        }

        var panelGo = new GameObject("GameplayHUDPanel", typeof(RectTransform), typeof(Image));
        panelRoot = panelGo.GetComponent<RectTransform>();
        panelRoot.SetParent(rootCanvas.transform, false);
        panelRoot.anchorMin = new Vector2(0.5f, 1f);
        panelRoot.anchorMax = new Vector2(0.5f, 1f);
        panelRoot.pivot = new Vector2(0.5f, 1f);
        panelRoot.sizeDelta = new Vector2(520f, 88f);
        panelRoot.anchoredPosition = new Vector2(0f, -16f);
        panelGo.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.62f);

        scoreText = CreateText("ScoreText", panelRoot, "คะแนน: 0", 26,
            new Vector2(-120f, -28f), 220f, TextAlignmentOptions.MidlineLeft);
        scoreRect = scoreText.rectTransform;
        timerText = CreateText("TimerText", panelRoot, "05:00", 26,
            new Vector2(120f, -28f), 160f, TextAlignmentOptions.MidlineRight);

        timerFill = CreateBar("TimerBar", panelRoot, new Vector2(0f, -62f), new Vector2(480f, 10f),
            new Color(1f, 1f, 1f, 0.12f), new Color(0.95f, 0.75f, 0.2f, 0.95f));

        uiBuilt = true;
        HandleScoreChanged(0);
        HandleTimerChanged(GameplayTimer.Instance != null ? GameplayTimer.Instance.RemainingTime : 0f);
    }

    void HandleScoreChanged(int score)
    {
        if (!uiBuilt || scoreText == null) return;

        if (score <= displayedScore)
        {
            LeanTween.cancel(scoreText.gameObject);
            displayedScore = score;
            scoreText.text = $"คะแนน: {score:N0}";
            if (scoreRect != null) scoreRect.localScale = Vector3.one;
            scoreText.color = ScoreBaseColor;
            return;
        }

        int fromScore = displayedScore;
        LeanTween.cancel(scoreText.gameObject);
        PlayScorePop();

        LeanTween.value(scoreText.gameObject, (float)fromScore, score, scoreCountDuration)
            .setEase(LeanTweenType.easeOutCubic)
            .setOnUpdate((float v) =>
            {
                displayedScore = Mathf.RoundToInt(v);
                scoreText.text = $"คะแนน: {displayedScore:N0}";
            })
            .setOnComplete(() =>
            {
                displayedScore = score;
                scoreText.text = $"คะแนน: {score:N0}";
            });
    }

    void PlayScorePop()
    {
        if (scoreRect == null) return;

        scoreRect.localScale = Vector3.one;
        LeanTween.scale(scoreRect, Vector3.one * scorePopScale, scorePopDuration * 0.42f)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnComplete(() =>
            {
                LeanTween.scale(scoreRect, Vector3.one, scorePopDuration * 0.58f)
                    .setEase(LeanTweenType.easeOutBack);
            });

        LeanTween.value(scoreText.gameObject, 0f, 1f, scorePopDuration)
            .setEase(LeanTweenType.easeOutSine)
            .setOnUpdate((float t) =>
            {
                float wave = Mathf.Sin(t * Mathf.PI);
                scoreText.color = Color.Lerp(ScoreBaseColor, ScorePopColor, wave * 0.5f);
            })
            .setOnComplete(() => scoreText.color = ScoreBaseColor);
    }

    void HandleTimerChanged(float remaining)
    {
        if (!uiBuilt) return;

        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(Mathf.Max(0f, remaining) / 60f);
            int seconds = Mathf.FloorToInt(Mathf.Max(0f, remaining) % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }

        if (timerFill != null && GameplayTimer.Instance != null)
        {
            float max = Mathf.Max(1f, GameplayTimer.Instance.SessionDuration);
            timerFill.fillAmount = Mathf.Clamp01(remaining / max);
        }
    }

    static TextMeshProUGUI CreateText(string name, RectTransform parent, string text, int fontSize,
        Vector2 anchoredPos, float width, TextAlignmentOptions alignment)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        var rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = new Vector2(width, 36f);
        rect.anchoredPosition = anchoredPos;

        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = Color.white;
        return tmp;
    }

    static Image CreateBar(string name, RectTransform parent, Vector2 anchoredPos, Vector2 size,
        Color bgColor, Color fillColor)
    {
        var bgGo = new GameObject(name, typeof(RectTransform), typeof(Image));
        var bgRect = bgGo.GetComponent<RectTransform>();
        bgRect.SetParent(parent, false);
        bgRect.anchorMin = new Vector2(0.5f, 1f);
        bgRect.anchorMax = new Vector2(0.5f, 1f);
        bgRect.pivot = new Vector2(0.5f, 1f);
        bgRect.sizeDelta = size;
        bgRect.anchoredPosition = anchoredPos;
        bgGo.GetComponent<Image>().color = bgColor;

        var fillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        var fillRect = fillGo.GetComponent<RectTransform>();
        fillRect.SetParent(bgRect, false);
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        var fill = fillGo.GetComponent<Image>();
        fill.color = fillColor;
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = (int)Image.OriginHorizontal.Left;
        fill.fillAmount = 1f;
        return fill;
    }
}
