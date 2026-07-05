using System;
using System.Collections;
using System.Collections.Generic;
using CookingGame;
using Pep.Core;
using Pep.GameplayEvents;
using Pep.Minigames.Chopping;
using Pep.Minigames.Cooking;
using Pep.Minigames.Plating;
using Pep.Minigames.Preparation;
using Pep.Minigames.Presentation;
using Pep.Recipe;
using Pep.Scoring;
using UnityEngine;
using UnityEngine.Events;

// ─── ลำดับ Minigame ตามที่กำหนด ───────────────────────────────────────────────
public enum MinigameFlowStep
{
    None = 0,
    Cutting,            // หันไส้กรอก / มะเขือเทศ + Event แมลงสาป  ← ซีน cut
    PourOil,            // ใส่น้ำมัน
    AddOnion,           // ใส่ของ + ผัดหัวหอม
    AddSausage,         // ใส่ไส้กรอก
    CrackEgg,           // ตอกไข่
    AddTomato,          // ใส่มะเขือเทศ
    CookAndControlHeat, // ผัดไปเรื่อยๆ + ควบคุมไฟ  ← ซีน PAD
    PanFlick,           // ร่อนกระทะ
    ServeToPlate,       // ใส่จาน
    Plating,            // จัดจาน
    FinalScore          // ให้คะแนนต่างๆ
}

[Serializable]
public class MinigameStepConfig
{
    public MinigameFlowStep step = MinigameFlowStep.None;
    [TextArea(1, 2)] public string instructionThai;
    public GameObject[] sceneRoots = Array.Empty<GameObject>();
    [Tooltip("กล้องของ step นี้ — เปิดอัตโนมัติเมื่อเข้า step นี้")]
    public Camera stepCamera;
    [Tooltip("รอกด Next Button ก่อนไป step ถัดไป (ใช้สำหรับ folk step ที่ไม่มี completion event)")]
    public bool waitForNextButton = false;
    [Tooltip("auto advance เมื่อ script ภายในยิง completion event")]
    public bool autoAdvanceOnComplete = true;
    public float fallbackScore = 70f;
    [Tooltip("เรียกเมื่อ step นี้เริ่มแสดง (หลัง transition fade out)")]
    public UnityEvent onStart = new UnityEvent();
    [Tooltip("เรียกเมื่อออกจาก step นี้ (ก่อน transition fade in)")]
    public UnityEvent onEnd = new UnityEvent();
}

public class MinigameFlowManager : MonoBehaviour
{
    [Header("Flow")]
    [SerializeField] private bool autoStartOnLoad;
    [SerializeField] private List<MinigameStepConfig> steps = new List<MinigameStepConfig>();

    // ─── ซีน cut ─────────────────────────────────────────────────────────────
    [Header("Folk — Cutting Scene")]
    [SerializeField] private CuttingMinigameController cuttingController;
    [SerializeField] private KitchenDisturbanceManager disturbanceManager;

    // ─── pep Preparation ─────────────────────────────────────────────────────
    [Header("Pep — Pour Oil")]
    [SerializeField] private PreparationState preparationState;
    [SerializeField] private TiltPourMinigame tiltPourMinigame;

    // ─── ซีน PAD ──────────────────────────────────────────────────────────────
    [Header("Folk — PAD Scene (sub-steps ใช้ waitForNext)")]
    [SerializeField] private StirFryManager stirFryManager;
    [SerializeField] private CookingSystem3D folkCookingSystem;

    // ─── pep Cooking / PanFlick ───────────────────────────────────────────────
    [Header("Pep — Cook & PanFlick")]
    [SerializeField] private CookingState cookingState;
    [SerializeField] private PanFlickMinigame panFlickMinigame;

    // ─── pep Plating ──────────────────────────────────────────────────────────
    [Header("Pep — Plating")]
    [SerializeField] private PlatingMinigame platingMinigame;
    [SerializeField] private PresentationState presentationState;
    [SerializeField] private ChoppingMockState choppingState;

    // ─── Pep internals ────────────────────────────────────────────────────────
    [Header("Pep — Core")]
    [SerializeField] private GameStateMachine gameStateMachine;

    // ─── Scoring ──────────────────────────────────────────────────────────────
    [Header("Scoring")]
    [SerializeField] private ScoringManager scoringManager;
    [SerializeField] private RecipeCatalogManager recipeCatalog;

    // ─── Camera ───────────────────────────────────────────────────────────────
    [Header("Camera")]
    [Tooltip("กล้อง fallback ใช้เมื่อ step ไม่มีกล้องของตัวเอง")]
    [SerializeField] private Camera defaultCamera;

    // ─── Step Transition ──────────────────────────────────────────────────────
    [Header("Step Transition")]
    [Tooltip("CanvasGroup สำหรับ fade ระหว่าง step — เปิด → เพิ่ม alpha → ลด alpha → ปิด")]
    [SerializeField] private CanvasGroup stepTransitionGroup;
    [SerializeField] private float transitionFadeInDuration = 0.35f;
    [SerializeField] private float transitionFadeOutDuration = 0.35f;

    // ─── Events (ต่อใน Inspector) ────────────────────────────────────────────
    [Header("Events")]
    public UnityEvent<MinigameFlowStep> onStepEntered;
    public UnityEvent<MinigameFlowStep, float> onStepCompleted;
    public UnityEvent<float> onFlowCompleted;

    // ─── Public State ─────────────────────────────────────────────────────────
    public MinigameFlowStep CurrentStep { get; private set; } = MinigameFlowStep.None;
    public int CurrentStepIndex { get; private set; } = -1;
    public bool IsRunning { get; private set; }
    public bool WaitingForNextButton { get; private set; }
    public string CurrentInstruction => GetCurrentConfig() is { } c
        && !string.IsNullOrWhiteSpace(c.instructionThai)
            ? c.instructionThai
            : DefaultInstruction(CurrentStep);

    // ─── Private ──────────────────────────────────────────────────────────────
    private bool stepDone;
    private bool isTransitioning;
    private float pendingScore = -1f;
    private RecipeSO activeRecipe;
    private Coroutine pollRoutine;
    private Coroutine transitionRoutine;

    // ═════════════════════════════════════════════════════════════════════════
    #region Unity Lifecycle

    private void Start()
    {
        BuildDefaultStepsIfEmpty();
        HideAllStepRoots();
        DisableAllStepCameras();
        PrepareTransitionOverlay();
        if (defaultCamera != null) defaultCamera.gameObject.SetActive(true);
        if (autoStartOnLoad) StartFlow();
    }

    private void OnDestroy()
    {
        StopTransitionRoutine();
        UnsubscribeAll();
    }

    #endregion

    // ═════════════════════════════════════════════════════════════════════════
    #region Public API

    public void StartFlow()
    {
        scoringManager?.ResetScores();
        if (disturbanceManager != null) disturbanceManager.enabled = true;
        LoadFirstRecipe();

        IsRunning = true;
        CurrentStepIndex = -1;
        GoToNextStep();
    }

    public void RestartFlow()
    {
        StopTransitionRoutine();
        StopCurrentStep();
        HideAllStepRoots();
        StartFlow();
    }

    /// <summary>เรียกจากปุ่ม Next Button</summary>
    public void GoToNextStep()
    {
        if (!IsRunning) IsRunning = true;
        if (isTransitioning) return;

        // บันทึกคะแนน step ปัจจุบันก่อน (ถ้ายังไม่ได้บันทึก)
        if (CurrentStepIndex >= 0 && !stepDone)
        {
            float s = pendingScore >= 0f ? pendingScore : GetCurrentConfig()?.fallbackScore ?? 70f;
            pendingScore = -1f;
            RecordStepScore(s);
        }

        int next = CurrentStepIndex + 1;
        if (next >= steps.Count) { FinishFlow(); return; }

        StopTransitionRoutine();
        transitionRoutine = StartCoroutine(TransitionToStep(next));
    }

    /// <summary>เรียกจาก folk script ที่ไม่มี event เมื่อ step นั้นเสร็จ</summary>
    public void NotifyStepCompleted(float score)
    {
        var cfg = GetCurrentConfig();
        if (cfg == null || !IsRunning) return;

        if (cfg.waitForNextButton)
        {
            // รอกดปุ่ม — แค่เก็บคะแนนไว้ก่อน
            WaitingForNextButton = true;
            pendingScore = score;
            onStepCompleted?.Invoke(CurrentStep, score);
            return;
        }

        RecordStepScore(score);
        if (cfg.autoAdvanceOnComplete) GoToNextStep();
    }

    #endregion

    // ═════════════════════════════════════════════════════════════════════════
    #region Step Lifecycle

    private void EnterStep(int index)
    {
        StopCurrentStep();
        HideAllStepRoots();

        CurrentStepIndex = index;
        CurrentStep = steps[index].step;
        stepDone = false;
        WaitingForNextButton = false;
        pendingScore = -1f;

        var cfg = steps[index];
        SetActiveRoots(cfg.sceneRoots, true);
        SwitchCamera(cfg.stepCamera);
        Subscribe(cfg);
        LaunchStepLogic(cfg);

        Debug.Log($"[Flow] Step {index + 1}/{steps.Count} ▶ {CurrentStep} — {CurrentInstruction}");
        onStepEntered?.Invoke(CurrentStep);
    }

    private void LaunchStepLogic(MinigameStepConfig cfg)
    {
        switch (cfg.step)
        {
            // ── ซีน cut ──────────────────────────────────────────────────────
            case MinigameFlowStep.Cutting:
                if (disturbanceManager != null) disturbanceManager.enabled = true;
                // CuttingMinigameController เปิด/ปิดเองผ่าน sceneRoots
                break;

            // ── ใส่น้ำมัน ─────────────────────────────────────────────────────
            case MinigameFlowStep.PourOil:
                if (preparationState != null)
                    preparationState.Begin();
                else if (tiltPourMinigame != null)
                {
                    tiltPourMinigame.enabled = true;
                    tiltPourMinigame.Begin();
                }
                break;

            // ── folk sub-steps (waitForNext) ──────────────────────────────────
            case MinigameFlowStep.AddOnion:
            case MinigameFlowStep.AddSausage:
            case MinigameFlowStep.CrackEgg:
            case MinigameFlowStep.AddTomato:
            case MinigameFlowStep.ServeToPlate:
                stirFryManager?.ResetProgress();
                break;

            // ── ผัดไปเรื่อยๆ + ควบคุมไฟ ──────────────────────────────────────
            case MinigameFlowStep.CookAndControlHeat:
                if (cookingState != null)
                {
                    cookingState.enabled = true;
                    cookingState.BeginCooking();
                }
                else if (folkCookingSystem != null)
                {
                    folkCookingSystem.enabled = true;
                    folkCookingSystem.StartFunction();
                    pollRoutine = StartCoroutine(PollFolkCooking());
                }
                break;

            // ── ร่อนกระทะ ─────────────────────────────────────────────────────
            case MinigameFlowStep.PanFlick:
                if (panFlickMinigame != null)
                {
                    panFlickMinigame.enabled = true;
                    panFlickMinigame.Begin();
                }
                break;

            // ── จัดจาน ────────────────────────────────────────────────────────
            case MinigameFlowStep.Plating:
                if (platingMinigame != null)
                    platingMinigame.Begin(activeRecipe);
                else if (presentationState != null)
                    presentationState.Begin();
                break;

            case MinigameFlowStep.FinalScore:
                break;
        }
    }

    private void StopCurrentStep()
    {
        UnsubscribeAll();
        StopPollRoutine();

        preparationState?.Stop();
        if (tiltPourMinigame != null) tiltPourMinigame.enabled = false;
        choppingState?.Stop();
        cookingState?.StopCooking();
        panFlickMinigame?.Stop();
        platingMinigame?.Stop();
        presentationState?.Stop();
        if (folkCookingSystem != null)
        {
            folkCookingSystem.EndFunction();
            folkCookingSystem.enabled = false;
        }
        if (disturbanceManager != null) disturbanceManager.enabled = false;

        EnterPepState(PepGameState.None, force: true);
    }

    private void FinishFlow()
    {
        GetCurrentConfig()?.onEnd?.Invoke();
        StopTransitionRoutine();
        StopCurrentStep();
        HideAllStepRoots();
        SwitchCamera(null);

        float final = scoringManager != null
            ? scoringManager.CompleteRecipeAndGetFinalAverage()
            : 0f;

        IsRunning = false;
        CurrentStep = MinigameFlowStep.FinalScore;
        Debug.Log($"[Flow] เสร็จสิ้น — คะแนนเฉลี่ย {final:0.##}");
        onFlowCompleted?.Invoke(final);
    }

    private IEnumerator TransitionToStep(int nextIndex)
    {
        isTransitioning = true;
        bool hasPreviousStep = CurrentStepIndex >= 0;

        if (hasPreviousStep)
            GetCurrentConfig()?.onEnd?.Invoke();

        if (hasPreviousStep && stepTransitionGroup != null)
        {
            stepTransitionGroup.gameObject.SetActive(true);
            stepTransitionGroup.alpha = 0f;
            yield return FadeCanvasGroup(stepTransitionGroup, 0f, 1f, transitionFadeInDuration);
        }

        EnterStep(nextIndex);

        if (hasPreviousStep && stepTransitionGroup != null)
        {
            yield return FadeCanvasGroup(stepTransitionGroup, 1f, 0f, transitionFadeOutDuration);
            stepTransitionGroup.gameObject.SetActive(false);
        }

        GetCurrentConfig()?.onStart?.Invoke();

        isTransitioning = false;
        transitionRoutine = null;
    }

    private static IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to, float duration)
    {
        if (group == null) yield break;

        if (duration <= 0f)
        {
            group.alpha = to;
            yield break;
        }

        group.alpha = from;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        group.alpha = to;
    }

    private void PrepareTransitionOverlay()
    {
        if (stepTransitionGroup == null) return;
        stepTransitionGroup.alpha = 0f;
        stepTransitionGroup.gameObject.SetActive(false);
    }

    private void StopTransitionRoutine()
    {
        if (transitionRoutine == null) return;
        StopCoroutine(transitionRoutine);
        transitionRoutine = null;
        isTransitioning = false;
        PrepareTransitionOverlay();
    }

    #endregion

    // ═════════════════════════════════════════════════════════════════════════
    #region Subscribe / Unsubscribe

    private void Subscribe(MinigameStepConfig cfg)
    {
        if (!cfg.autoAdvanceOnComplete) return;

        switch (cfg.step)
        {
            case MinigameFlowStep.Cutting:
                if (cuttingController != null)
                    cuttingController.OnAllFoodSliced += HandleCuttingDone;
                break;

            case MinigameFlowStep.PourOil:
                if (preparationState != null)
                    preparationState.OnPreparationCompleted += HandlePreparationDone;
                else if (tiltPourMinigame != null)
                    tiltPourMinigame.OnPourCompleted += HandlePourDone;
                break;

            case MinigameFlowStep.CookAndControlHeat:
                if (cookingState != null)
                    cookingState.OnCookingCompleted += HandleCookingDone;
                // folkCookingSystem → ใช้ pollRoutine แทน
                break;

            case MinigameFlowStep.PanFlick:
                if (panFlickMinigame != null)
                    panFlickMinigame.OnPanFlickCompleted += HandlePanFlickDone;
                break;

            case MinigameFlowStep.Plating:
                if (platingMinigame != null)
                    platingMinigame.OnMinigameCompleted += HandlePlatingDone;
                else if (presentationState != null)
                    presentationState.OnPresentationCompleted += HandlePresentationDone;
                break;
        }
    }

    private void UnsubscribeAll()
    {
        if (cuttingController != null)
            cuttingController.OnAllFoodSliced -= HandleCuttingDone;
        if (preparationState != null)
            preparationState.OnPreparationCompleted -= HandlePreparationDone;
        if (tiltPourMinigame != null)
            tiltPourMinigame.OnPourCompleted -= HandlePourDone;
        if (cookingState != null)
            cookingState.OnCookingCompleted -= HandleCookingDone;
        if (panFlickMinigame != null)
            panFlickMinigame.OnPanFlickCompleted -= HandlePanFlickDone;
        if (platingMinigame != null)
            platingMinigame.OnMinigameCompleted -= HandlePlatingDone;
        if (presentationState != null)
            presentationState.OnPresentationCompleted -= HandlePresentationDone;
    }

    #endregion

    // ═════════════════════════════════════════════════════════════════════════
    #region Event Handlers

    private void HandleCuttingDone()                            => AutoFinish(GetCurrentConfig()?.fallbackScore ?? 80f);
    private void HandlePreparationDone(float s, bool _)        => AutoFinish(s);
    private void HandlePourDone(float s, bool _)               => AutoFinish(s);
    private void HandleCookingDone(float s, bool _)            => AutoFinish(s);
    private void HandlePanFlickDone(float s, bool _)           => AutoFinish(s);
    private void HandlePlatingDone(float s, bool _)            => AutoFinish(s);
    private void HandlePresentationDone(float s, bool _)       => AutoFinish(s);

    private void AutoFinish(float score)
    {
        if (!IsRunning || stepDone) return;
        RecordStepScore(score);
        GoToNextStep();
    }

    #endregion

    // ═════════════════════════════════════════════════════════════════════════
    #region Polling (folk CookingSystem3D ไม่มี event)

    private IEnumerator PollFolkCooking()
    {
        if (folkCookingSystem == null) yield break;

        while (IsRunning && CurrentStep == MinigameFlowStep.CookAndControlHeat)
        {
            if (folkCookingSystem.isCooked || folkCookingSystem.isBurnt || folkCookingSystem.isTimeOut)
            {
                float score = folkCookingSystem.isCooked ? 90f
                    : folkCookingSystem.isBurnt ? 30f
                    : 50f;
                AutoFinish(score);
                yield break;
            }
            yield return null;
        }
    }

    private void StopPollRoutine()
    {
        if (pollRoutine != null) { StopCoroutine(pollRoutine); pollRoutine = null; }
    }

    #endregion

    // ═════════════════════════════════════════════════════════════════════════
    #region Score

    private void RecordStepScore(float score)
    {
        if (stepDone) return;
        stepDone = true;
        WaitingForNextButton = false;

        var cfg = GetCurrentConfig();
        if (cfg != null && scoringManager != null)
            scoringManager.ReportStepScore("MinigameFlow", cfg.step.ToString(), score);

        onStepCompleted?.Invoke(CurrentStep, score);
    }

    #endregion

    // ═════════════════════════════════════════════════════════════════════════
    #region Camera

    private void SwitchCamera(Camera target)
    {
        DisableAllStepCameras();
        if (target != null)
        {
            target.gameObject.SetActive(true);
            if (defaultCamera != null) defaultCamera.gameObject.SetActive(false);
        }
        else
        {
            if (defaultCamera != null) defaultCamera.gameObject.SetActive(true);
        }
    }

    private void DisableAllStepCameras()
    {
        foreach (var cfg in steps)
            if (cfg.stepCamera != null) cfg.stepCamera.gameObject.SetActive(false);
    }

    #endregion

    // ═════════════════════════════════════════════════════════════════════════
    #region Helpers

    private MinigameStepConfig GetCurrentConfig()
    {
        if (CurrentStepIndex < 0 || CurrentStepIndex >= steps.Count) return null;
        return steps[CurrentStepIndex];
    }

    private void HideAllStepRoots()
    {
        foreach (var cfg in steps) SetActiveRoots(cfg.sceneRoots, false);
    }

    private static void SetActiveRoots(GameObject[] roots, bool active)
    {
        if (roots == null) return;
        foreach (var r in roots) if (r != null) r.SetActive(active);
    }

    private void EnterPepState(PepGameState state, bool force = false)
        => gameStateMachine?.TryChangeState(state, "MinigameFlowManager", force);

    private void LoadFirstRecipe()
    {
        if (recipeCatalog == null) return;
        var ids = recipeCatalog.GetRecipeIdList();
        if (ids.Count > 0) activeRecipe = recipeCatalog.GetRecipeById(ids[0]);
    }

    #endregion

    // ═════════════════════════════════════════════════════════════════════════
    #region Default Steps

    private void BuildDefaultStepsIfEmpty()
    {
        if (steps.Count > 0) return;
        steps = new List<MinigameStepConfig>
        {
            S(MinigameFlowStep.Cutting,            "หันไส้กรอก / มะเขือเทศ  —  ระวังแมลงสาป!",  auto: true,  wait: false),
            S(MinigameFlowStep.PourOil,            "เอียงขวดเทน้ำมันลงกระทะ",                    auto: true,  wait: false),
            S(MinigameFlowStep.AddOnion,           "ใส่ของลงกระทะ — ผัดหัวหอมให้หอม",           auto: false, wait: true),
            S(MinigameFlowStep.AddSausage,         "ใส่ไส้กรอกลงกระทะ",                         auto: false, wait: true),
            S(MinigameFlowStep.CrackEgg,           "ตอกไข่ลงกระทะ",                             auto: false, wait: true),
            S(MinigameFlowStep.AddTomato,          "ใส่มะเขือเทศ",                               auto: false, wait: true),
            S(MinigameFlowStep.CookAndControlHeat, "ผัดไปเรื่อยๆ — ควบคุมไฟให้พอดี",            auto: true,  wait: false),
            S(MinigameFlowStep.PanFlick,           "ร่อนกระทะ!",                                 auto: true,  wait: false),
            S(MinigameFlowStep.ServeToPlate,       "ตักใส่จาน",                                  auto: false, wait: true),
            S(MinigameFlowStep.Plating,            "จัดจานให้สวยงาม",                            auto: true,  wait: false),
            S(MinigameFlowStep.FinalScore,         "สรุปคะแนน",                                  auto: false, wait: true),
        };
    }

    private static MinigameStepConfig S(MinigameFlowStep step, string th, bool auto, bool wait) =>
        new MinigameStepConfig
        {
            step = step,
            instructionThai = th,
            autoAdvanceOnComplete = auto,
            waitForNextButton = wait,
            fallbackScore = 70f,
        };

    private static string DefaultInstruction(MinigameFlowStep s) => s switch
    {
        MinigameFlowStep.Cutting             => "หันไส้กรอก / มะเขือเทศ",
        MinigameFlowStep.PourOil             => "ใส่น้ำมัน",
        MinigameFlowStep.AddOnion            => "ใส่ของ + ผัดหัวหอม",
        MinigameFlowStep.AddSausage          => "ใส่ไส้กรอก",
        MinigameFlowStep.CrackEgg            => "ตอกไข่",
        MinigameFlowStep.AddTomato           => "ใส่มะเขือเทศ",
        MinigameFlowStep.CookAndControlHeat  => "ผัดไปเรื่อยๆ + ควบคุมไฟ",
        MinigameFlowStep.PanFlick            => "ร่อนกระทะ",
        MinigameFlowStep.ServeToPlate        => "ใส่จาน",
        MinigameFlowStep.Plating             => "จัดจาน",
        MinigameFlowStep.FinalScore          => "สรุปคะแนน",
        _                                    => string.Empty,
    };

    #endregion
}
