using System;
using TMPro;
using UnityEngine;

public class MatchSessionTimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private float sessionDuration = 180f;

    public float RemainingTime { get; private set; }
    public float SessionDuration => sessionDuration;
    public bool IsRunning { get; private set; }
    public bool IsTimeUp { get; private set; }

    public event Action<float> OnTimerChanged;
    public event Action OnTimeUp;

    private bool hasTriggeredTimeUp;

    void Update()
    {
        if (!IsRunning || IsTimeUp) return;

        RemainingTime -= Time.deltaTime;
        if (RemainingTime <= 0f)
        {
            RemainingTime = 0f;
            IsTimeUp = true;
            IsRunning = false;
            PushUi(RemainingTime);
            OnTimerChanged?.Invoke(RemainingTime);
            if (!hasTriggeredTimeUp)
            {
                hasTriggeredTimeUp = true;
                OnTimeUp?.Invoke();
            }
            return;
        }

        PushUi(RemainingTime);
        OnTimerChanged?.Invoke(RemainingTime);
    }

    public void StartCountdown(float seconds)
    {
        sessionDuration = Mathf.Max(1f, seconds);
        RemainingTime = sessionDuration;
        IsRunning = true;
        IsTimeUp = false;
        hasTriggeredTimeUp = false;
        PushUi(RemainingTime);
        OnTimerChanged?.Invoke(RemainingTime);
    }

    public void StopTimer()
    {
        IsRunning = false;
    }

    public void BindTimerText(TextMeshProUGUI text)
    {
        timerText = text;
        PushUi(RemainingTime);
    }

    void PushUi(float seconds)
    {
        if (timerText == null) return;
        int total = Mathf.CeilToInt(Mathf.Max(0f, seconds));
        int m = total / 60;
        int s = total % 60;
        timerText.text = $"{m:00}:{s:00}";
    }
}
