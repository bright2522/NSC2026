using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class LobbyUIAnimations
{
    public const float PanelInDuration = 0.38f;
    public const float PanelOutDuration = 0.22f;
    public const float ChildStagger = 0.05f;

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

    public static void CancelAndReset(GameObject go)
    {
        if (go == null)
        {
            return;
        }

        Cancel(go);
        ResetVisualState(go, includeChildren: false);
    }

    public static void ResetVisualState(GameObject go, bool includeChildren)
    {
        if (go == null)
        {
            return;
        }

        var rect = go.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.localScale = Vector3.one;
        }

        var group = go.GetComponent<CanvasGroup>();
        if (group != null)
        {
            group.alpha = 1f;
            group.interactable = true;
            group.blocksRaycasts = true;
        }

        if (!includeChildren)
        {
            return;
        }

        for (int i = 0; i < go.transform.childCount; i++)
        {
            ResetVisualState(go.transform.GetChild(i).gameObject, true);
        }
    }

    public static void ResetPanelTree(GameObject panel)
    {
        if (panel == null)
        {
            return;
        }

        Cancel(panel);
        ResetVisualState(panel, includeChildren: true);
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
        group.alpha = 1f;
        group.interactable = true;
        group.blocksRaycasts = true;

        if (rect != null)
        {
            rect.localScale = Vector3.one * 0.96f;
            try
            {
                LeanTween.scale(rect, Vector3.one, PanelInDuration)
                    .setDelay(delay)
                    .setEase(LeanTweenType.easeOutCubic)
                    .setOnComplete(() => onComplete?.Invoke());
                return;
            }
            catch (Exception)
            {
                rect.localScale = Vector3.one;
            }
        }

        onComplete?.Invoke();
    }

    public static void AnimatePanelOut(GameObject panel, Action onComplete = null)
    {
        if (panel == null || !panel.activeSelf)
        {
            onComplete?.Invoke();
            return;
        }

        Cancel(panel);
        panel.SetActive(false);
        ResetPanelTree(panel);
        onComplete?.Invoke();
    }

    public static void TransitionPanels(GameObject from, GameObject to, Action onComplete = null)
    {
        if (to == null)
        {
            onComplete?.Invoke();
            return;
        }

        if (from != null && from != to && from.activeSelf)
        {
            CancelAndReset(from);
            from.SetActive(false);
        }

        AnimatePanelIn(to, onComplete: () =>
        {
            StaggerChildrenIn(to.transform, 0.04f);
            onComplete?.Invoke();
        });
    }

    public static void StaggerChildrenIn(Transform parent, float baseDelay = 0.04f)
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
        if (rect == null)
        {
            return;
        }

        rect.localScale = Vector3.one * 0.9f;
        LeanTween.scale(rect, Vector3.one, 0.28f)
            .setDelay(delay)
            .setEase(LeanTweenType.easeOutBack);
    }

    public static void AnimatePopText(TMP_Text text)
    {
        if (text == null)
        {
            return;
        }

        Cancel(text.gameObject);
        var rect = text.rectTransform;
        rect.localScale = Vector3.one;

        LeanTween.scale(rect, Vector3.one * 1.08f, 0.2f)
            .setEase(LeanTweenType.easeOutBack)
            .setOnComplete(() =>
            {
                if (rect != null)
                {
                    LeanTween.scale(rect, Vector3.one, 0.16f)
                        .setEase(LeanTweenType.easeOutQuad);
                }
            });
    }

    public static void AnimateStatusPulse(TMP_Text text)
    {
        if (text == null)
        {
            return;
        }

        Cancel(text.gameObject);
        var rect = text.rectTransform;
        rect.localScale = Vector3.one;

        LeanTween.scale(rect, Vector3.one * 1.03f, 0.14f)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnComplete(() =>
            {
                if (rect != null)
                {
                    LeanTween.scale(rect, Vector3.one, 0.18f)
                        .setEase(LeanTweenType.easeOutQuad);
                }
            });
    }

    public static void AnimateReveal(GameObject element, float delay = 0f)
    {
        if (element == null)
        {
            return;
        }

        CancelAndReset(element);
        element.SetActive(true);
        AnimateElementIn(element, delay);
    }

    public static void AnimateBreathingPulse(GameObject target, float scale = 1.02f, float duration = 1.8f)
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

        rect.localScale = Vector3.one;
        LeanTween.scale(rect, Vector3.one * scale, duration * 0.5f)
            .setEase(LeanTweenType.easeInOutSine)
            .setLoopPingPong();
    }

    public static void AnimateFadeIn(GameObject target, float duration = 0.4f, float delay = 0f)
    {
        if (target == null)
        {
            return;
        }

        Cancel(target);
        target.SetActive(true);

        var group = EnsureCanvasGroup(target);
        group.alpha = 1f;
        group.interactable = true;
        group.blocksRaycasts = true;
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
    [SerializeField] private float hoverScale = 1.04f;
    [SerializeField] private float pressScale = 0.96f;
    [SerializeField] private float tweenDuration = 0.12f;

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
        _pressed = false;
        if (_rect != null)
        {
            _rect.localScale = _baseScale;
        }
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
        TweenScale(_baseScale);
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
            .setEase(LeanTweenType.easeOutQuad);
    }
}
