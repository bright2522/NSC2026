using System;
using UnityEngine;

namespace Pep.Core
{
    public enum PepGameState
    {
        None = 0,
        Boot = 1,
        RecipeSelection = 2,
        SmartFridge = 3,
        Preparation = 4,
        Chopping = 5,
        Cooking = 6,
        Presentation = 7,
        Result = 8,
        Paused = 9
    }

    [Serializable]
    public struct PepGameStateTransition
    {
        public PepGameState from;
        public PepGameState to;
        public string reason;
        public float realtimeSinceStartup;

        public PepGameStateTransition(PepGameState from, PepGameState to, string reason)
        {
            this.from = from;
            this.to = to;
            this.reason = reason;
            realtimeSinceStartup = Time.realtimeSinceStartup;
        }
    }

    public class GameStateMachine : MonoBehaviour
    {
        [SerializeField] private PepGameState initialState = PepGameState.Boot;
        [SerializeField] private bool lockStateChanges;

        public PepGameState CurrentState { get; private set; } = PepGameState.None;
        public PepGameState PreviousState { get; private set; } = PepGameState.None;
        public bool IsStateChangeLocked => lockStateChanges;

        public event Action<PepGameStateTransition> OnStateChanged;
        public event Action<PepGameState> OnStateEntered;
        public event Action<PepGameState> OnStateExited;

        private bool initialized;

        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (initialized) return;

            initialized = true;
            CurrentState = initialState;
            PreviousState = PepGameState.None;
            OnStateEntered?.Invoke(CurrentState);
        }

        public void SetInitialState(PepGameState state, bool overwriteCurrent = false)
        {
            initialState = state;
            if (overwriteCurrent || !initialized)
            {
                CurrentState = state;
                PreviousState = PepGameState.None;
            }
        }

        public void SetStateLock(bool locked)
        {
            lockStateChanges = locked;
        }

        public bool CanTransitionTo(PepGameState nextState)
        {
            if (!initialized) Initialize();
            if (lockStateChanges) return false;
            if (nextState == PepGameState.None) return false;
            return nextState != CurrentState;
        }

        public bool TryChangeState(PepGameState nextState, string reason = "", bool force = false)
        {
            if (!initialized) Initialize();

            if (!force && !CanTransitionTo(nextState)) return false;
            if (force && nextState == PepGameState.None) return false;
            if (nextState == CurrentState) return false;

            var fromState = CurrentState;
            PreviousState = fromState;

            OnStateExited?.Invoke(fromState);
            CurrentState = nextState;

            var transition = new PepGameStateTransition(fromState, nextState, reason);
            OnStateChanged?.Invoke(transition);
            OnStateEntered?.Invoke(nextState);
            return true;
        }

        public bool TryChangeStateByName(string nextStateName, string reason = "", bool force = false)
        {
            if (string.IsNullOrWhiteSpace(nextStateName)) return false;
            if (!Enum.TryParse(nextStateName, true, out PepGameState parsedState)) return false;
            return TryChangeState(parsedState, reason, force);
        }

        public bool IsInState(PepGameState state)
        {
            return CurrentState == state;
        }
    }
}
