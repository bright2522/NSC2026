using System;
using UnityEngine;

public class GameplayTimer : MonoBehaviour
{
    public static GameplayTimer Instance { get; private set; }

    [SerializeField] private float sessionDuration = 300f;

    public float RemainingTime { get; private set; }
    public float SessionDuration => sessionDuration;
    public bool IsRunning { get; private set; }
    public bool IsTimeUp { get; private set; }

    public event Action<float> OnTimerChanged;
    public event Action OnTimeUp;

    private bool hasTriggeredTimeUp;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (!IsRunning || IsTimeUp) return;

        RemainingTime -= Time.deltaTime;
        if (RemainingTime <= 0f)
        {
            RemainingTime = 0f;
            IsTimeUp = true;
            IsRunning = false;
            OnTimerChanged?.Invoke(RemainingTime);
            if (!hasTriggeredTimeUp)
            {
                hasTriggeredTimeUp = true;
                OnTimeUp?.Invoke();
            }
            return;
        }

        OnTimerChanged?.Invoke(RemainingTime);
    }

    public void StartCountdown(float seconds)
    {
        sessionDuration = Mathf.Max(1f, seconds);
        RemainingTime = sessionDuration;
        IsRunning = true;
        IsTimeUp = false;
        hasTriggeredTimeUp = false;
        OnTimerChanged?.Invoke(RemainingTime);
    }

    public void StopTimer()
    {
        IsRunning = false;
    }
}
