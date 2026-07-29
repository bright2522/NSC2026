using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class MatchCountdownUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private float numberDuration = 0.85f;
    [SerializeField] private float startScale = 2.2f;
    [SerializeField] private float endScale = 1f;
    [SerializeField] private bool showGo = true;
    [SerializeField] private string goLabel = "GO!";
    [SerializeField] private int[] numbers = { 3, 2, 1 };

    public event Action OnCountdownFinished;

    private Coroutine playRoutine;
    private int activeTweenId = -1;

    public void Play()
    {
        if (playRoutine != null)
            StopCoroutine(playRoutine);

        CancelActiveTween();
        playRoutine = StartCoroutine(PlayRoutine());
    }

    public void Stop()
    {
        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
            playRoutine = null;
        }

        CancelActiveTween();
        SetVisible(false);
    }

    IEnumerator PlayRoutine()
    {
        SetVisible(true);

        if (numbers != null)
        {
            for (int i = 0; i < numbers.Length; i++)
            {
                yield return AnimateLabel(numbers[i].ToString());
            }
        }

        if (showGo)
            yield return AnimateLabel(goLabel);

        SetVisible(false);
        playRoutine = null;
        OnCountdownFinished?.Invoke();
    }

    IEnumerator AnimateLabel(string label)
    {
        if (countdownText == null)
        {
            yield return new WaitForSeconds(numberDuration);
            yield break;
        }

        CancelActiveTween();

        countdownText.text = label;
        countdownText.alpha = 1f;
        countdownText.rectTransform.localScale = Vector3.one * startScale;

        bool done = false;
        activeTweenId = LeanTween.scale(countdownText.rectTransform, Vector3.one * endScale, numberDuration)
            .setEase(LeanTweenType.easeOutBack)
            .setOnComplete(() => done = true)
            .id;

        LeanTween.value(countdownText.gameObject, 1f, 0f, numberDuration * 0.35f)
            .setDelay(numberDuration * 0.65f)
            .setOnUpdate((float a) =>
            {
                if (countdownText != null)
                    countdownText.alpha = a;
            });

        while (!done)
            yield return null;
    }

    void SetVisible(bool visible)
    {
        if (countdownText == null) return;
        countdownText.gameObject.SetActive(visible);
        if (!visible)
            countdownText.alpha = 0f;
    }

    void CancelActiveTween()
    {
        if (countdownText != null)
        {
            LeanTween.cancel(countdownText.gameObject);
            LeanTween.cancel(countdownText.rectTransform);
        }

        if (activeTweenId >= 0)
        {
            LeanTween.cancel(activeTweenId);
            activeTweenId = -1;
        }
    }

    void OnDisable()
    {
        Stop();
    }
}
