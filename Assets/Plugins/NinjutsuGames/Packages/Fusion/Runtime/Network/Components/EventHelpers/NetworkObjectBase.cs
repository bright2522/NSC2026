using System.Collections;
using System.Collections.Generic;
using Fusion;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    public abstract class NetworkObjectBase : SimulationBehaviour
    {
        private class TriggerData
        {
            public readonly Trigger Trigger;
            public readonly GameObject Target;

            public TriggerData(Trigger trigger, GameObject target)
            {
                Trigger = trigger;
                Target = target;
            }
        }

        private readonly List<TriggerData> _triggerList = new();
        private bool _registered;

        private void Awake()
        {
            hideFlags = HideFlags.HideInInspector | HideFlags.DontSaveInEditor;
        }

        private void OnEnable()
        {
            StartCoroutine(RegisterOnRunner());
        }

        private void OnDisable()
        {
            RemoveFromRunner();
        }

        private void OnDestroy()
        {
            UnregisterAll();
        }

        private IEnumerator RegisterOnRunner()
        {
            if (_registered) yield break;

            yield return new WaitUntil(() => NetworkManager.IsConnected);

            var runner = NetworkRunner.GetRunnerForGameObject(gameObject);
            if (runner && runner.IsRunning)
            {
                runner.AddGlobal(this);
                _registered = true;
            }
        }

        private void RemoveFromRunner()
        {
            if (!_registered) return;

            var runner = NetworkRunner.GetRunnerForGameObject(gameObject);
            if (runner && runner.IsRunning)
            {
                _registered = false;
                runner.RemoveGlobal(this);
            }
        }

        public void RunTriggers()
        {
            NetworkDataManager.EventSpawned -= RunTriggers;

            foreach (var triggerData in _triggerList)
            {
                if (triggerData.Trigger == null) continue;

                var args = new Args(triggerData.Trigger.gameObject);
                if (triggerData.Target) args.ChangeTarget(triggerData.Target);
                _ = triggerData.Trigger.Execute(args);
            }
        }

        protected void TryRunTriggers()
        {
            if (NetworkDataManager.Instance) RunTriggers();
            else
            {
                NetworkDataManager.EventSpawned -= RunTriggers;
                NetworkDataManager.EventSpawned += RunTriggers;
            }
        }

        public void Register(Trigger trigger, GameObject target = null)
        {
            if (trigger == null) return;

            _triggerList.Add(new TriggerData(trigger, target));
        }

        public void Unregister(Trigger trigger)
        {
            if (trigger == null) return;

            _triggerList.RemoveAll(data => data.Trigger == trigger);
        }

        public void UnregisterAll()
        {
            _triggerList.Clear();
        }
    }
}