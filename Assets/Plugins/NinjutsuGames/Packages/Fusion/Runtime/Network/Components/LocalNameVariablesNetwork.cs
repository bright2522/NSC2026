using Fusion;
using GameCreator.Runtime.Variables;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [RequireComponent(typeof(LocalNameVariables), typeof(NetworkObject))]
    [HelpURL("https://docs.ninjutsugames.com/game-creator-2/fusion-module/variables#local-name-variables")]
    [AddComponentMenu("Game Creator/Fusion/Local Name Variables Network")]
    [Icon(GameCreator.Runtime.Common.RuntimePaths.GIZMOS + "GizmoLocalNameVariables.png")]
    public class LocalNameVariablesNetwork : NetworkBehaviour, IStateAuthorityChanged
    {
        private bool debug = false;

        [Networked, Capacity(14), OnChangedRender(nameof(VariablesChanged))]
        private NetworkDictionary<string, VariableData> NetworkVars => default;
        
        [SerializeField] private bool dontDestroyOnLoad;

        private LocalNameVariables _localNameVars;
        
        public void Awake()
        {
            if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);
        }

        public void OnEnable()
        {
            if(!_localNameVars) _localNameVars = GetComponent<LocalNameVariables>();
            _localNameVars.Register(OnVariableChange);
        }
        
        public void OnDisable()
        {
            _localNameVars.Unregister(OnVariableChange);
        }

        public override void Spawned()
        {
            VariablesChanged();
        }

        private void UpdateLocalVariables()
        {
            foreach (var networkVar in NetworkVars)
            {
                _localNameVars.Set(networkVar.Key, networkVar.Value.GetValue());
            }
        }

        private void VariablesChanged()
        {
            if(HasStateAuthority) return;
            UpdateLocalVariables();
        }

        private void OnVariableChange(string varId)
        {
            if(!Runner) return;
            if(!Runner.IsRunning) return;
            if(!HasStateAuthority) return;
            
            var data = _localNameVars.Get(varId);
            if (!data.IsAllowedType()) return;
            
            if(debug) Debug.Log($"OnVariableChange: {varId}={data} type: {data.GetType().Name} NetworkVars: {NetworkVars.Count} allowed: {data.IsAllowedType()} hasStateAuthority: {HasStateAuthority}");

            var varData = VariableData.ConvertFromObject(data);
            NetworkVars.Set(varId, varData);
        }

        public void StateAuthorityChanged()
        {
            if(debug) Debug.LogWarning($"StateAuthorityChanged: {HasStateAuthority} state authority: {Object.StateAuthority}");
            if(!HasStateAuthority) UpdateLocalVariables();
        }
    }
}