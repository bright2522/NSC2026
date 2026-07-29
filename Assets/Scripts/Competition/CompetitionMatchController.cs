using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum CompetitionMatchState
{
    Idle,
    Countdown,
    Playing,
    Ended
}

public class CompetitionMatchController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private MatchCountdownUI countdownUi;
    [SerializeField] private MatchSessionTimer sessionTimer;
    [SerializeField] private StationSliderController stationSlider;
    [SerializeField] private OrderQuestUI orderQuestUi;

    [Header("Match")]
    [SerializeField] private float matchDurationSeconds = 180f;
    [SerializeField] private bool beginOnStart = true;
    [SerializeField] private OrderQuest[] mockOrders =
    {
        new OrderQuest("pad_krapow", "Pad Krapow"),
        new OrderQuest("omelette", "Omelette"),
        new OrderQuest("fried_rice", "Fried Rice"),
    };

    [Header("Result")]
    [SerializeField] private string resultSceneName;

    public CompetitionMatchState State { get; private set; } = CompetitionMatchState.Idle;

    public event Action OnMatchStarted;
    public event Action OnMatchEnded;

    void Start()
    {
        if (!beginOnStart)
            return;

#if CMPSETUP_COMPLETE
        if (FindFirstObjectByType<MultiplayerGameManager>() != null)
            return;
#endif

        BeginMatch();
    }

    void OnEnable()
    {
        if (countdownUi != null)
            countdownUi.OnCountdownFinished += HandleCountdownFinished;

        if (sessionTimer != null)
            sessionTimer.OnTimeUp += HandleTimeUp;
    }

    void OnDisable()
    {
        if (countdownUi != null)
            countdownUi.OnCountdownFinished -= HandleCountdownFinished;

        if (sessionTimer != null)
            sessionTimer.OnTimeUp -= HandleTimeUp;
    }

    public void BeginMatch()
    {
        if (State == CompetitionMatchState.Countdown || State == CompetitionMatchState.Playing)
            return;

        State = CompetitionMatchState.Countdown;

        stationSlider?.SetEnabled(false);
        orderQuestUi?.SetVisible(false);
        sessionTimer?.StopTimer();

        if (countdownUi != null)
            countdownUi.Play();
        else
            HandleCountdownFinished();
    }

    void HandleCountdownFinished()
    {
        if (State != CompetitionMatchState.Countdown && State != CompetitionMatchState.Idle)
            return;

        State = CompetitionMatchState.Playing;

        if (orderQuestUi != null)
            orderQuestUi.SetOrders(mockOrders);

        stationSlider?.SetEnabled(true);

        float duration = matchDurationSeconds;
        if (sessionTimer != null)
        {
            duration = matchDurationSeconds > 0f ? matchDurationSeconds : sessionTimer.SessionDuration;
            sessionTimer.StartCountdown(duration);
        }

        OnMatchStarted?.Invoke();
    }

    void HandleTimeUp()
    {
        EndMatch(loadResultScene: true);
    }

    public void EndMatch(bool loadResultScene = true)
    {
        if (State == CompetitionMatchState.Ended) return;

        State = CompetitionMatchState.Ended;
        sessionTimer?.StopTimer();
        stationSlider?.SetEnabled(false);
        countdownUi?.Stop();

        OnMatchEnded?.Invoke();

        if (!loadResultScene) return;

        if (string.IsNullOrWhiteSpace(resultSceneName))
        {
            Debug.LogWarning("[CompetitionMatchController] resultSceneName is empty — skip scene load.");
            return;
        }

        SceneManager.LoadScene(resultSceneName);
    }
}
