using System.Collections;
using System.Collections.Generic;
using Pep.Core;
using Pep.Scoring;
using UnityEngine;

namespace Pep.GameplayEvents
{
    public class KitchenDisturbanceManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameStateMachine gameStateMachine;
        [SerializeField] private ScoringManager scoringManager;
        [SerializeField] private CockroachRunner cockroachPrefab;
        [SerializeField] private Transform spawnParent;
        [SerializeField] private List<Transform> runPath = new List<Transform>();

        [Header("Spawn")]
        [SerializeField] private float minIntervalSeconds = 8f;
        [SerializeField] private float maxIntervalSeconds = 25f;
        [SerializeField] private float spawnChance = 0.35f;
        [SerializeField] private float runnerSpeed = 2.2f;
        [SerializeField] private bool allowOnlyOneActive = true;

        [Header("Score")]
        [SerializeField] private float hitScore = 10f;
        [SerializeField] private float escapeScore = 0f;

        private Coroutine spawnLoop;
        private CockroachRunner activeRunner;
        private bool enabledByState;

        public void Configure(GameStateMachine gsm, ScoringManager sm)
        {
            gameStateMachine = gsm;
            scoringManager = sm;

            if (!isActiveAndEnabled || gameStateMachine == null) return;
            gameStateMachine.OnStateEntered -= HandleStateEntered;
            gameStateMachine.OnStateExited -= HandleStateExited;
            gameStateMachine.OnStateEntered += HandleStateEntered;
            gameStateMachine.OnStateExited += HandleStateExited;
            enabledByState = IsActiveState(gameStateMachine.CurrentState);
            RefreshSpawnLoop();
        }

        public void SetRunPath(List<Transform> points)
        {
            runPath = points ?? new List<Transform>();
        }

        private void Awake()
        {
            if (gameStateMachine == null)
            {
                gameStateMachine = FindObjectOfType<GameStateMachine>();
            }

            if (scoringManager == null)
            {
                scoringManager = FindObjectOfType<ScoringManager>();
            }
        }

        private void OnEnable()
        {
            if (gameStateMachine == null) return;
            gameStateMachine.OnStateEntered += HandleStateEntered;
            gameStateMachine.OnStateExited += HandleStateExited;
            enabledByState = IsActiveState(gameStateMachine.CurrentState);
            RefreshSpawnLoop();
        }

        private void OnDisable()
        {
            if (gameStateMachine != null)
            {
                gameStateMachine.OnStateEntered -= HandleStateEntered;
                gameStateMachine.OnStateExited -= HandleStateExited;
            }
            StopSpawnLoop();
            ClearActiveRunner();
        }

        private void HandleStateEntered(PepGameState state)
        {
            enabledByState = IsActiveState(state);
            RefreshSpawnLoop();
        }

        private void HandleStateExited(PepGameState state)
        {
            if (!IsActiveState(state)) return;
            enabledByState = false;
            RefreshSpawnLoop();
            ClearActiveRunner();
        }

        private bool IsActiveState(PepGameState state)
        {
            return state == PepGameState.Preparation || state == PepGameState.Chopping;
        }

        private void RefreshSpawnLoop()
        {
            if (enabledByState)
            {
                if (spawnLoop == null) spawnLoop = StartCoroutine(SpawnLoop());
            }
            else
            {
                StopSpawnLoop();
            }
        }

        private IEnumerator SpawnLoop()
        {
            while (enabledByState)
            {
                float wait = Random.Range(minIntervalSeconds, maxIntervalSeconds);
                yield return new WaitForSeconds(wait);

                if (!enabledByState) break;
                if (allowOnlyOneActive && activeRunner != null) continue;
                if (Random.value > spawnChance) continue;

                SpawnCockroach();
            }
        }

        private void StopSpawnLoop()
        {
            if (spawnLoop == null) return;
            StopCoroutine(spawnLoop);
            spawnLoop = null;
        }

        private void SpawnCockroach()
        {
            if (runPath.Count < 2) return;

            if (cockroachPrefab == null)
            {
                var primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
                primitive.name = "PepCockroachPlaceholder";
                primitive.transform.localScale = new Vector3(0.2f, 0.1f, 0.3f);
                var runner = primitive.AddComponent<CockroachRunner>();
                BindRunner(runner);
                runner.Setup(runPath, runnerSpeed);
                activeRunner = runner;
                return;
            }

            Transform parent = spawnParent != null ? spawnParent : transform;
            CockroachRunner runnerInstance = Instantiate(cockroachPrefab, parent);
            BindRunner(runnerInstance);
            runnerInstance.Setup(runPath, runnerSpeed);
            activeRunner = runnerInstance;
        }

        private void BindRunner(CockroachRunner runner)
        {
            runner.OnFinished -= HandleRunnerFinished;
            runner.OnFinished += HandleRunnerFinished;
        }

        private void HandleRunnerFinished(CockroachRunner runner, CockroachOutcome outcome)
        {
            if (activeRunner == runner)
            {
                activeRunner = null;
            }

            if (scoringManager == null) return;

            if (outcome == CockroachOutcome.Hit)
            {
                scoringManager.ReportStepScore("pep/Cockroach", "Hygiene Hit", hitScore);
            }
            else
            {
                scoringManager.ReportStepScore("pep/Cockroach", "Hygiene Escape", escapeScore);
            }
        }

        private void ClearActiveRunner()
        {
            if (activeRunner == null) return;
            Destroy(activeRunner.gameObject);
            activeRunner = null;
        }
    }
}
