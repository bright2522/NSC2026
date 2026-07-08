using Fusion;
using GameCreator.Runtime.Variables;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [RequireComponent(typeof(NetworkObject))]
    [HelpURL("https://docs.ninjutsugames.com/game-creator-2/fusion-module/variables#global-name-variables")]
    [AddComponentMenu("Game Creator/Fusion/Global Name Variables Network")]
    [Icon(GameCreator.Runtime.Common.RuntimePaths.GIZMOS + "GizmoGlobalNameVariables.png")]
    public class GlobalNameVariablesNetwork : NetworkBehaviour, IStateAuthorityChanged
    {
        public GlobalNameVariables Variables => variables;
        
        [SerializeField] private GlobalNameVariables variables;
        [SerializeField] private bool dontDestroyOnLoad;

        private bool debug = false;

        [Networked, Capacity(14)]
        private NetworkDictionary<string, VariableData> NetworkVars => default;

        private ChangeDetector _changeDetector;

        private void Awake()
        {
            if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);
        }

        public void OnEnable()
        {
            if(!variables) variables = GetComponent<GlobalNameVariables>();
            variables.Register(OnVariableChange);
        }
        
        public void OnDisable()
        {
            variables.Unregister(OnVariableChange);
        }

        public override void Spawned()
        {
            _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

            if (HasStateAuthority) return;

            UpdateLocalVariables();
        }

        private void UpdateLocalVariables()
        {
            foreach (var networkVar in NetworkVars)
            {
                variables.Set(networkVar.Key, networkVar.Value.GetValue());
            }
        }

        public override void Render()
        {
            if(HasStateAuthority) return;

            foreach (var change in _changeDetector.DetectChanges(this))
            {
                switch (change)
                {
                    case nameof(NetworkVars):
                        UpdateLocalVariables();
                        break;
                }
            }
        }

        private void OnVariableChange(string varId)
        {
            if(!Runner) return;
            if(!Runner.IsRunning) return;
            
            var data = variables.Get(varId);
            if (!data.IsAllowedType()) return;
            
            if(debug) Debug.Log($"OnVariableChange: {varId}={data} type: {data.GetType().Name} NetworkVars: {NetworkVars.Count} allowed: {data.IsAllowedType()} hasStateAuthority: {HasStateAuthority}");

            var varData = VariableData.ConvertFromObject(data);
            if (HasStateAuthority)
            {
                NetworkVars.Set(varId, varData);
            }
        }

        public void StateAuthorityChanged()
        {
            if(debug) Debug.LogWarning($"StateAuthorityChanged: {HasStateAuthority} state authority: {Object.StateAuthority}");
            if(!HasStateAuthority) UpdateLocalVariables();
        }
    }
}