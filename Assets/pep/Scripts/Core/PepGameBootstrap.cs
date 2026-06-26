using System.Collections;
using System.Collections.Generic;
using Pep.GameplayEvents;
using Pep.Input;
using Pep.Integration;
using Pep.Minigames.Chopping;
using Pep.Minigames.Cooking;
using Pep.Minigames.Preparation;
using Pep.Minigames.Presentation;
using Pep.Recipe;
using Pep.Scoring;
using Pep.SmartFridge;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Pep.Core
{
    public class PepGameBootstrap : MonoBehaviour
    {
        [Header("Flow")]
        [SerializeField] private bool autoStartOnSceneLoad = true;
        [SerializeField] private float bootDelay = 1f;
        [SerializeField] private float recipeSelectionDelay = 1f;
        [SerializeField] private float fridgeDelay = 1f;
        [SerializeField] private bool keepAliveOnLoad = false;

        [Header("Managers")]
        [SerializeField] private GameStateMachine gameStateMachine;
        [SerializeField] private PlayerDataManager playerDataManager;
        [SerializeField] private InventoryManager inventoryManager;
        [SerializeField] private RecipeCatalogManager recipeCatalogManager;
        [SerializeField] private ScoringManager scoringManager;
        [SerializeField] private KitchenDisturbanceManager disturbanceManager;

        [Header("Input")]
        [SerializeField] private AccelerometerGestureDetector accelerometer;
        [SerializeField] private FlickDetector flickDetector;
        [SerializeField] private TiltPourReader tiltPourReader;

        [Header("Steps")]
        [SerializeField] private PreparationState preparationState;
        [SerializeField] private TiltPourMinigame tiltPourMinigame;
        [SerializeField] private ChoppingMockState choppingState;
        [SerializeField] private CookingState cookingState;
        [SerializeField] private PanFlickMinigame panFlickMinigame;
        [SerializeField] private PresentationState presentationState;

        [Header("Runtime UI")]
        [SerializeField] private bool createDebugUi = true;

        [Header("Integration")]
        [SerializeField] private bool useExternalBridge;

        public bool IsRunning => isRunning;
        public string CurrentFlowStep => currentFlowStep;
        public string SelectedRecipeId => selectedRecipeId;

        private readonly IPepExternalFlowBridge bridge = new PepExternalFlowBridge();
        private Coroutine flowRoutine;
        private bool isRunning;
        private bool skipRequested;
        private string currentFlowStep = "Idle";
        private string selectedRecipeId = "stew";
        private bool hasCompletedRun;
        private PepGameBootstrapDebugView debugView;

        private void Awake()
        {
            if (keepAliveOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }

            EnsureReferences();
            ConfigureDependencies();
        }

        private void Start()
        {
            if (createDebugUi)
            {
                CreateDebugUiIfNeeded();
            }

            if (autoStartOnSceneLoad)
            {
                StartFlow();
            }
        }

        private void Update()
        {
            if (createDebugUi && debugView != null)
            {
                UpdateDebugView();
            }
        }

        public void StartFlow()
        {
            if (flowRoutine != null)
            {
                StopCoroutine(flowRoutine);
            }

            StopAllMinigames();

            isRunning = true;
            skipRequested = false;
            hasCompletedRun = false;
            flowRoutine = StartCoroutine(RunFlow());
        }

        private void StopAllMinigames()
        {
            preparationState?.Stop();
            tiltPourMinigame?.Stop();
            choppingState?.Stop();
            cookingState?.StopCooking();
            panFlickMinigame?.Stop();
            presentationState?.Stop();
        }

        public void RestartFlow()
        {
            if (scoringManager != null)
            {
                scoringManager.ResetScores();
            }

            if (disturbanceManager != null)
            {
                disturbanceManager.enabled = true;
            }

            StartFlow();
        }

        public void RequestSkipCurrentStep()
        {
            skipRequested = true;
        }

        public void ForceState(PepGameState targetState)
        {
            gameStateMachine?.TryChangeState(targetState, "Forced by debug", true);
        }

        private IEnumerator RunFlow()
        {
            EnterState(PepGameState.Boot, "Bootstrap");
            currentFlowStep = "Boot";
            UpdateDebugView();
            yield return WaitOrSkip(bootDelay);

            currentFlowStep = "RecipeSelection";
            EnterState(PepGameState.RecipeSelection, "Auto recipe selection");
            SetupRecipeAndInventory();
            UpdateDebugView();
            yield return WaitOrSkip(recipeSelectionDelay);

            currentFlowStep = "SmartFridge";
            EnterState(PepGameState.SmartFridge, "Auto smart fridge");
            UpdateDebugView();
            yield return WaitOrSkip(fridgeDelay);

            currentFlowStep = "Preparation";
            EnterState(PepGameState.Preparation, "Preparation state");
            yield return RunPreparationStep();
            UpdateDebugView();

            currentFlowStep = "Chopping";
            EnterState(PepGameState.Chopping, "Chopping state");
            yield return RunChoppingStep();
            UpdateDebugView();

            currentFlowStep = "Cooking";
            EnterState(PepGameState.Cooking, "Cooking state");
            yield return RunCookingStep();
            UpdateDebugView();

            currentFlowStep = "PanFlick";
            EnterState(PepGameState.Cooking, "Pan flick step", true);
            yield return RunPanFlickStep();
            UpdateDebugView();

            currentFlowStep = "Presentation";
            EnterState(PepGameState.Presentation, "Presentation state");
            yield return RunPresentationStep();
            UpdateDebugView();

            currentFlowStep = "Result";
            EnterState(PepGameState.Result, "Result state");
            FinalizeRun();
            UpdateDebugView();
        }

        private void FinalizeRun()
        {
            if (hasCompletedRun) return;
            hasCompletedRun = true;
            isRunning = false;

            float finalAverage = scoringManager != null ? scoringManager.CompleteRecipeAndGetFinalAverage() : 0f;
            bridge.NotifyPepFlowCompleted(finalAverage);

            if (playerDataManager != null)
            {
                playerDataManager.AddScore(Mathf.RoundToInt(finalAverage));
                playerDataManager.Save();
            }
        }

        private IEnumerator RunPreparationStep()
        {
            if (preparationState == null)
            {
                if (scoringManager != null) scoringManager.ReportStepScore("pep/Preparation", "Preparation", 55f);
                yield break;
            }

            bool done = false;
            preparationState.OnPreparationCompleted += OnCompleted;
            preparationState.Begin();

            while (!done)
            {
                if (skipRequested)
                {
                    skipRequested = false;
                    preparationState.ForceComplete(70f);
                }
                yield return null;
            }

            preparationState.OnPreparationCompleted -= OnCompleted;

            void OnCompleted(float score, bool success)
            {
                bridge.NotifyPepStepCompleted("Preparation", score);
                done = true;
            }
        }

        private IEnumerator RunChoppingStep()
        {
            if (choppingState == null)
            {
                if (scoringManager != null) scoringManager.ReportStepScore("pep/Chopping", "Chopping", 60f);
                yield break;
            }

            bool done = false;
            choppingState.OnChoppingCompleted += OnCompleted;
            choppingState.Begin();

            while (!done)
            {
                if (skipRequested)
                {
                    skipRequested = false;
                    choppingState.ForceComplete(72f);
                }
                yield return null;
            }

            choppingState.OnChoppingCompleted -= OnCompleted;

            void OnCompleted(float score, bool success)
            {
                bridge.NotifyPepStepCompleted("Chopping", score);
                done = true;
            }
        }

        private IEnumerator RunCookingStep()
        {
            if (cookingState == null)
            {
                if (scoringManager != null) scoringManager.ReportStepScore("pep/Cooking", "Cooking", 58f);
                yield break;
            }

            bool done = false;
            cookingState.OnCookingCompleted += OnCompleted;
            cookingState.enabled = true;
            cookingState.BeginCooking();

            while (!done)
            {
                if (skipRequested)
                {
                    skipRequested = false;
                    cookingState.StopCooking();
                    OnCompleted(75f, true);
                }
                yield return null;
            }

            cookingState.OnCookingCompleted -= OnCompleted;

            void OnCompleted(float score, bool success)
            {
                if (scoringManager != null)
                {
                    scoringManager.ReportStepScore("pep/CookingState", "Cooking", score);
                }
                bridge.NotifyPepStepCompleted("Cooking", score);
                done = true;
            }
        }

        private IEnumerator RunPanFlickStep()
        {
            if (panFlickMinigame == null)
            {
                if (scoringManager != null) scoringManager.ReportStepScore("pep/PanFlick", "Pan Flick", 62f);
                yield break;
            }

            bool done = false;
            panFlickMinigame.OnPanFlickCompleted += OnCompleted;
            panFlickMinigame.enabled = true;
            panFlickMinigame.Begin();

            while (!done)
            {
                if (skipRequested)
                {
                    skipRequested = false;
                    panFlickMinigame.Stop();
                    OnCompleted(78f, true);
                }
                yield return null;
            }

            panFlickMinigame.OnPanFlickCompleted -= OnCompleted;

            void OnCompleted(float score, bool success)
            {
                bridge.NotifyPepStepCompleted("PanFlick", score);
                done = true;
            }
        }

        private IEnumerator RunPresentationStep()
        {
            if (presentationState == null)
            {
                if (scoringManager != null) scoringManager.ReportStepScore("pep/Presentation", "Presentation", 66f);
                yield break;
            }

            bool done = false;
            presentationState.OnPresentationCompleted += OnCompleted;
            presentationState.Begin();

            while (!done)
            {
                if (skipRequested)
                {
                    skipRequested = false;
                    presentationState.ForceComplete(80f);
                }
                yield return null;
            }

            presentationState.OnPresentationCompleted -= OnCompleted;

            void OnCompleted(float score, bool success)
            {
                bridge.NotifyPepStepCompleted("Presentation", score);
                done = true;
            }
        }

        private void EnterState(PepGameState state, string reason, bool force = false)
        {
            if (gameStateMachine == null) return;
            gameStateMachine.TryChangeState(state, reason, force);
        }

        private IEnumerator WaitOrSkip(float duration)
        {
            float timer = 0f;
            while (timer < duration)
            {
                if (skipRequested)
                {
                    skipRequested = false;
                    yield break;
                }
                timer += Time.deltaTime;
                yield return null;
            }
        }

        private void SetupRecipeAndInventory()
        {
            if (bridge.IsAvailable && useExternalBridge)
            {
                if (bridge.TryReadSelectedRecipe(out string externalRecipe) && !string.IsNullOrWhiteSpace(externalRecipe))
                {
                    selectedRecipeId = externalRecipe;
                }
            }

            if (recipeCatalogManager != null)
            {
                recipeCatalogManager.RebuildIndex();
                List<string> recipeIds = recipeCatalogManager.GetRecipeIdList();
                if (recipeIds.Count > 0)
                {
                    selectedRecipeId = recipeIds[0];
                }

                RecipeSO recipe = recipeCatalogManager.GetRecipeById(selectedRecipeId);
                if (recipe != null && inventoryManager != null)
                {
                    inventoryManager.ApplySelectionFromSmartFridge(new List<string>(recipe.RequiredIngredientIds));
                }
            }
        }

        private void EnsureReferences()
        {
            gameStateMachine = gameStateMachine ?? FindObjectOfType<GameStateMachine>() ?? CreateComponent<GameStateMachine>("PepCore/GameStateMachine");
            playerDataManager = playerDataManager ?? FindObjectOfType<PlayerDataManager>() ?? CreateComponent<PlayerDataManager>("PepCore/PlayerDataManager");
            inventoryManager = inventoryManager ?? FindObjectOfType<InventoryManager>() ?? CreateComponent<InventoryManager>("PepCore/InventoryManager");
            scoringManager = scoringManager ?? FindObjectOfType<ScoringManager>() ?? CreateComponent<ScoringManager>("PepCore/ScoringManager");
            recipeCatalogManager = recipeCatalogManager ?? FindObjectOfType<RecipeCatalogManager>() ?? CreateComponent<RecipeCatalogManager>("PepCore/RecipeCatalogManager");

            accelerometer = accelerometer ?? FindObjectOfType<AccelerometerGestureDetector>() ?? CreateComponent<AccelerometerGestureDetector>("PepMobileInput");
            flickDetector = flickDetector ?? FindObjectOfType<FlickDetector>() ?? AddOrGetComponent<FlickDetector>(accelerometer.gameObject);
            tiltPourReader = tiltPourReader ?? FindObjectOfType<TiltPourReader>() ?? AddOrGetComponent<TiltPourReader>(accelerometer.gameObject);

            tiltPourMinigame = tiltPourMinigame ?? FindObjectOfType<TiltPourMinigame>() ?? CreateComponent<TiltPourMinigame>("PepMinigames/TiltPour");
            preparationState = preparationState ?? FindObjectOfType<PreparationState>() ?? AddOrGetComponent<PreparationState>(tiltPourMinigame.gameObject);
            choppingState = choppingState ?? FindObjectOfType<ChoppingMockState>() ?? CreateComponent<ChoppingMockState>("PepMinigames/ChoppingMock");
            cookingState = cookingState ?? FindObjectOfType<CookingState>() ?? CreateComponent<CookingState>("PepMinigames/CookingState");
            panFlickMinigame = panFlickMinigame ?? FindObjectOfType<PanFlickMinigame>() ?? CreateComponent<PanFlickMinigame>("PepMinigames/PanFlick");
            presentationState = presentationState ?? FindObjectOfType<PresentationState>() ?? CreateComponent<PresentationState>("PepMinigames/Presentation");

            disturbanceManager = disturbanceManager ?? FindObjectOfType<KitchenDisturbanceManager>() ?? CreateComponent<KitchenDisturbanceManager>("PepGameplayEvents/KitchenDisturbanceManager");

            EnsureDisturbancePath();
        }

        private void ConfigureDependencies()
        {
            if (flickDetector != null && accelerometer != null) flickDetector.Configure(accelerometer);
            if (tiltPourReader != null && accelerometer != null) tiltPourReader.Configure(accelerometer);
            if (tiltPourMinigame != null) tiltPourMinigame.Configure(tiltPourReader, scoringManager);
            if (preparationState != null) preparationState.Configure(tiltPourMinigame, scoringManager);
            if (choppingState != null) choppingState.Configure(scoringManager);
            if (panFlickMinigame != null) panFlickMinigame.Configure(flickDetector, scoringManager);
            if (presentationState != null) presentationState.Configure(scoringManager);
            if (disturbanceManager != null) disturbanceManager.Configure(gameStateMachine, scoringManager);

            if (tiltPourMinigame != null) tiltPourMinigame.enabled = false;
            if (panFlickMinigame != null) panFlickMinigame.enabled = false;
            if (cookingState != null) cookingState.enabled = false;
            if (preparationState != null) preparationState.enabled = true;
            if (choppingState != null) choppingState.enabled = true;
            if (presentationState != null) presentationState.enabled = true;
        }

        private void EnsureDisturbancePath()
        {
            if (disturbanceManager == null) return;

            var waypoints = new List<Transform>();
            Transform root = GetOrCreateTransform("PepGameplayEvents/RunPath");

            for (int i = 0; i < 3; i++)
            {
                var point = new GameObject($"Point{i + 1}").transform;
                point.SetParent(root, false);
                point.position = new Vector3(-2.5f + i * 2.5f, 0.5f, 0f);
                waypoints.Add(point);
            }

            disturbanceManager.SetRunPath(waypoints);
        }

        private void CreateDebugUiIfNeeded()
        {
            if (debugView != null) return;
            EnsureEventSystem();

            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                var canvasObject = new GameObject("PepBootstrapCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
            }

            var rootObject = new GameObject("PepBootstrapDebug", typeof(RectTransform), typeof(Image));
            var rect = rootObject.GetComponent<RectTransform>();
            rect.SetParent(canvas.transform, false);
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.sizeDelta = new Vector2(460f, 215f);
            rect.anchoredPosition = new Vector2(24f, 20f);
            rootObject.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);

            debugView = rootObject.AddComponent<PepGameBootstrapDebugView>();
            debugView.Initialize(this);
            UpdateDebugView();
        }

        private void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null) return;

            var eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        private void UpdateDebugView()
        {
            if (debugView == null) return;
            debugView.Refresh();
        }

        private T CreateComponent<T>(string path) where T : Component
        {
            Transform parent = GetOrCreateTransform(path);
            return parent.gameObject.AddComponent<T>();
        }

        private T AddOrGetComponent<T>(GameObject target) where T : Component
        {
            T component = target.GetComponent<T>();
            if (component == null)
            {
                component = target.AddComponent<T>();
            }
            return component;
        }

        private Transform GetOrCreateTransform(string path)
        {
            string[] parts = path.Split('/');
            Transform current = null;

            foreach (string rawName in parts)
            {
                string name = rawName.Trim();
                if (string.IsNullOrEmpty(name)) continue;

                Transform next = current == null ? GameObject.Find(name)?.transform : current.Find(name);
                if (next == null)
                {
                    var go = new GameObject(name);
                    next = go.transform;
                    if (current != null)
                    {
                        next.SetParent(current, false);
                    }
                }
                current = next;
            }

            return current;
        }

        private sealed class PepGameBootstrapDebugView : MonoBehaviour
        {
            private PepGameBootstrap bootstrap;
            private Text infoText;

            public void Initialize(PepGameBootstrap owner)
            {
                bootstrap = owner;
                infoText = CreateText("Info", transform as RectTransform, string.Empty, 20, new Vector2(10f, 68f));
                infoText.alignment = TextAnchor.UpperLeft;

                CreateButton("Start", new Vector2(10f, 10f), new Vector2(136f, 46f), bootstrap.StartFlow);
                CreateButton("Restart", new Vector2(154f, 10f), new Vector2(136f, 46f), bootstrap.RestartFlow);
                CreateButton("Skip", new Vector2(298f, 10f), new Vector2(136f, 46f), bootstrap.RequestSkipCurrentStep);
            }

            public void Refresh()
            {
                if (infoText == null || bootstrap == null) return;

                string stateName = bootstrap.gameStateMachine != null ? bootstrap.gameStateMachine.CurrentState.ToString() : "None";
                float total = bootstrap.scoringManager != null ? bootstrap.scoringManager.TotalScore : 0f;
                int steps = bootstrap.scoringManager != null ? bootstrap.scoringManager.StepCount : 0;

                infoText.text =
                    $"Flow: {(bootstrap.IsRunning ? "Running" : "Stopped")}\n" +
                    $"Step: {bootstrap.CurrentFlowStep}\n" +
                    $"State: {stateName}\n" +
                    $"Recipe: {bootstrap.SelectedRecipeId}\n" +
                    $"Score: {total:0.##} ({steps} steps)";
            }

            private Text CreateText(string name, RectTransform parent, string text, int size, Vector2 anchoredPosition)
            {
                var textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
                var rect = textObject.GetComponent<RectTransform>();
                rect.SetParent(parent, false);
                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(0f, 0f);
                rect.pivot = new Vector2(0f, 0f);
                rect.sizeDelta = new Vector2(440f, 140f);
                rect.anchoredPosition = anchoredPosition;

                var textComp = textObject.GetComponent<Text>();
                textComp.font = GetDefaultFont();
                textComp.text = text;
                textComp.fontSize = size;
                textComp.color = Color.white;
                textComp.raycastTarget = false;
                return textComp;
            }

            private void CreateButton(string label, Vector2 anchoredPosition, Vector2 size, UnityEngine.Events.UnityAction action)
            {
                var root = new GameObject($"{label}Button", typeof(RectTransform), typeof(Image), typeof(Button));
                var rect = root.GetComponent<RectTransform>();
                rect.SetParent(transform, false);
                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(0f, 0f);
                rect.pivot = new Vector2(0f, 0f);
                rect.sizeDelta = size;
                rect.anchoredPosition = anchoredPosition;

                root.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.2f);
                root.GetComponent<Button>().onClick.AddListener(action);

                var labelObj = new GameObject("Label", typeof(RectTransform), typeof(Text));
                var labelRect = labelObj.GetComponent<RectTransform>();
                labelRect.SetParent(rect, false);
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = new Vector2(6f, 2f);
                labelRect.offsetMax = new Vector2(-6f, -2f);
                var labelText = labelObj.GetComponent<Text>();
                labelText.font = GetDefaultFont();
                labelText.text = label;
                labelText.fontSize = 18;
                labelText.color = Color.white;
                labelText.alignment = TextAnchor.MiddleCenter;
                labelText.raycastTarget = false;
            }

            private static Font GetDefaultFont()
            {
                var f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
                return f;
            }
        }
    }
}
