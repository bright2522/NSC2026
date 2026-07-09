using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class LobbyUIAnimations
{
    public const float PanelInDuration = 0.55f;
    public const float PanelOutDuration = 0.32f;
    public const float ChildStagger = 0.07f;

    public static CanvasGroup EnsureCanvasGroup(GameObject go)
    {
        var group = go.GetComponent<CanvasGroup>();
        if (group == null)
        {
            group = go.AddComponent<CanvasGroup>();
        }

        return group;
    }

    public static void Cancel(GameObject go)
    {
        if (go != null)
        {
            LeanTween.cancel(go);
        }
    }

    public static void AnimatePanelIn(GameObject panel, float delay = 0f, Action onComplete = null)
    {
        if (panel == null)
        {
            onComplete?.Invoke();
            return;
        }

        Cancel(panel);
        panel.SetActive(true);

        var rect = panel.GetComponent<RectTransform>();
        var group = EnsureCanvasGroup(panel);

        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;

        if (rect != null)
        {
            var targetPos = rect.anchoredPosition;
            rect.localScale = Vector3.one * 0.9f;
            rect.anchoredPosition = new Vector2(targetPos.x, targetPos.y - 28f);
            LeanTween.move(rect, new Vector3(targetPos.x, targetPos.y, 0f), PanelInDuration)
                .setDelay(delay)
                .setEase(LeanTweenType.easeOutCubic);
            LeanTween.scale(rect, Vector3.one, PanelInDuration)
                .setDelay(delay)
                .setEase(LeanTweenType.easeOutBack);
        }

        LeanTween.alphaCanvas(group, 1f, PanelInDuration)
            .setDelay(delay)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnComplete(() =>
            {
                group.interactable = true;
                group.blocksRaycasts = true;
                onComplete?.Invoke();
            });
    }

    public static void AnimatePanelOut(GameObject panel, Action onComplete = null)
    {
        if (panel == null || !panel.activeSelf)
        {
            onComplete?.Invoke();
            return;
        }

        Cancel(panel);

        var rect = panel.GetComponent<RectTransform>();
        var group = EnsureCanvasGroup(panel);
        group.interactable = false;
        group.blocksRaycasts = false;

        Vector2 originalPos = rect != null ? rect.anchoredPosition : Vector2.zero;

        if (rect != null)
        {
            LeanTween.scale(rect, Vector3.one * 0.94f, PanelOutDuration)
                .setEase(LeanTweenType.easeInCubic);
            LeanTween.move(rect, new Vector3(originalPos.x, originalPos.y - 16f, 0f), PanelOutDuration)
                .setEase(LeanTweenType.easeInCubic);
        }

        LeanTween.alphaCanvas(group, 0f, PanelOutDuration)
            .setEase(LeanTweenType.easeInQuad)
            .setOnComplete(() =>
            {
                panel.SetActive(false);
                if (rect != null)
                {
                    rect.localScale = Vector3.one;
                    rect.anchoredPosition = originalPos;
                }

                group.alpha = 1f;
                onComplete?.Invoke();
            });
    }

    public static void TransitionPanels(GameObject from, GameObject to, Action onComplete = null)
    {
        if (from == to)
        {
            AnimatePanelIn(to, onComplete: onComplete);
            return;
        }

        if (from != null && from.activeSelf)
        {
            AnimatePanelOut(from, () => AnimatePanelIn(to, onComplete: onComplete));
            return;
        }

        AnimatePanelIn(to, onComplete: onComplete);
    }

    public static void StaggerChildrenIn(Transform parent, float baseDelay = 0.12f)
    {
        if (parent == null)
        {
            return;
        }

        int index = 0;
        for (int i = 0; i < parent.childCount; i++)
        {
            var child = parent.GetChild(i);
            if (!child.gameObject.activeInHierarchy)
            {
                continue;
            }

            AnimateElementIn(child.gameObject, baseDelay + ChildStagger * index);
            index++;
        }
    }

    public static void AnimateElementIn(GameObject element, float delay = 0f)
    {
        if (element == null)
        {
            return;
        }

        Cancel(element);

        var rect = element.GetComponent<RectTransform>();
        var group = EnsureCanvasGroup(element);
        group.alpha = 0f;

        if (rect != null)
        {
            rect.localScale = Vector3.one * 0.82f;
            LeanTween.scale(rect, Vector3.one, 0.42f)
                .setDelay(delay)
                .setEase(LeanTweenType.easeOutBack);
        }

        LeanTween.alphaCanvas(group, 1f, 0.38f)
            .setDelay(delay)
            .setEase(LeanTweenType.easeOutQuad);
    }

    public static void AnimatePopText(TMP_Text text)
    {
        if (text == null)
        {
            return;
        }

        Cancel(text.gameObject);
        var rect = text.rectTransform;
        rect.localScale = Vector3.one * 0.6f;
        var baseColor = text.color;

        LeanTween.scale(rect, Vector3.one * 1.12f, 0.28f)
            .setEase(LeanTweenType.easeOutBack)
            .setOnComplete(() =>
            {
                LeanTween.scale(rect, Vector3.one, 0.22f)
                    .setEase(LeanTweenType.easeOutQuad);
            });

        LeanTween.value(text.gameObject, 0f, 1f, 0.5f)
            .setEase(LeanTweenType.easeOutSine)
            .setOnUpdate((float t) =>
            {
                float wave = Mathf.Sin(t * Mathf.PI);
                text.color = Color.Lerp(baseColor, Color.white, wave * 0.45f);
            })
            .setOnComplete(() => text.color = baseColor);
    }

    public static void AnimateStatusPulse(TMP_Text text)
    {
        if (text == null)
        {
            return;
        }

        Cancel(text.gameObject);
        var rect = text.rectTransform;
        var baseColor = text.color;

        LeanTween.scale(rect, Vector3.one * 1.04f, 0.18f)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnComplete(() =>
            {
                LeanTween.scale(rect, Vector3.one, 0.24f)
                    .setEase(LeanTweenType.easeOutBack);
            });

        LeanTween.value(text.gameObject, 0f, 1f, 0.42f)
            .setOnUpdate((float t) =>
            {
                float wave = Mathf.Sin(t * Mathf.PI);
                text.color = Color.Lerp(baseColor, new Color(0.6f, 0.85f, 1f), wave * 0.35f);
            })
            .setOnComplete(() => text.color = baseColor);
    }

    public static void AnimateReveal(GameObject element, float delay = 0f)
    {
        if (element == null)
        {
            return;
        }

        Cancel(element);
        element.SetActive(true);
        AnimateElementIn(element, delay);
    }

    public static void AnimateBreathingPulse(GameObject target, float scale = 1.03f, float duration = 1.6f)
    {
        if (target == null)
        {
            return;
        }

        Cancel(target);
        var rect = target.GetComponent<RectTransform>();
        if (rect == null)
        {
            return;
        }

        LeanTween.scale(rect, Vector3.one * scale, duration * 0.5f)
            .setEase(LeanTweenType.easeInOutSine)
            .setLoopPingPong();
    }

    public static void AnimateFadeIn(GameObject target, float duration = 0.5f, float delay = 0f)
    {
        if (target == null)
        {
            return;
        }

        Cancel(target);
        target.SetActive(true);
        var group = EnsureCanvasGroup(target);
        group.alpha = 0f;
        LeanTween.alphaCanvas(group, 1f, duration)
            .setDelay(delay)
            .setEase(LeanTweenType.easeOutQuad);
    }

    public static void AnimateFadeOut(GameObject target, float duration = 0.35f, Action onComplete = null)
    {
        if (target == null)
        {
            onComplete?.Invoke();
            return;
        }

        Cancel(target);
        var group = EnsureCanvasGroup(target);
        LeanTween.alphaCanvas(group, 0f, duration)
            .setEase(LeanTweenType.easeInQuad)
            .setOnComplete(() =>
            {
                target.SetActive(false);
                group.alpha = 1f;
                onComplete?.Invoke();
            });
    }

    public static void SetupButtonFeedback(Button button)
    {
        if (button == null || button.GetComponent<LobbyUIButtonFeedback>() != null)
        {
            return;
        }

        button.gameObject.AddComponent<LobbyUIButtonFeedback>();
    }
}

public class LobbyUIButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private float hoverScale = 1.06f;
    [SerializeField] private float pressScale = 0.94f;
    [SerializeField] private float tweenDuration = 0.18f;

    private RectTransform _rect;
    private Vector3 _baseScale;
    private bool _pressed;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _baseScale = _rect != null ? _rect.localScale : Vector3.one;
    }

    private void OnEnable()
    {
        if (_rect != null)
        {
            _rect.localScale = _baseScale;
        }
    }

    private void OnDisable()
    {
        LobbyUIAnimations.Cancel(gameObject);
        if (_rect != null)
        {
            _rect.localScale = _baseScale;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsInteractable() || _pressed)
        {
            return;
        }

        TweenScale(_baseScale * hoverScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!IsInteractable())
        {
            return;
        }

        _pressed = false;
        TweenScale(_baseScale);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!IsInteractable())
        {
            return;
        }

        _pressed = true;
        TweenScale(_baseScale * pressScale);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!IsInteractable())
        {
            return;
        }

        _pressed = false;
        TweenScale(_baseScale * hoverScale);
    }

    private bool IsInteractable()
    {
        var button = GetComponent<Button>();
        return button == null || button.interactable;
    }

    private void TweenScale(Vector3 target)
    {
        if (_rect == null)
        {
            return;
        }

        LobbyUIAnimations.Cancel(gameObject);
        LeanTween.scale(_rect, target, tweenDuration)
            .setEase(LeanTweenType.easeOutBack);
    }
}
