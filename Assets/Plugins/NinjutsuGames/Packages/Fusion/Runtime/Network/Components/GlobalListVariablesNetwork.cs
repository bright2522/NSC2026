using System;
using Fusion;
using GameCreator.Runtime.Variables;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [RequireComponent(typeof(NetworkObject))]
    [AddComponentMenu("Game Creator/Fusion/Global List Variables Network")]
    [HelpURL("https://docs.ninjutsugames.com/game-creator-2/fusion-module/variables#global-list-variables")]
    [Icon(GameCreator.Runtime.Common.RuntimePaths.GIZMOS + "GizmoGlobalListVariables.png")]
    public class GlobalListVariablesNetwork : NetworkBehaviour
    {
        [SerializeField] private ListMode m_SyncMode = ListMode.SyncData;
        [SerializeField] private GlobalListVariables list;
        [SerializeField] private bool dontDestroyOnLoad;
        
        [Networked, Capacity(14)]
        private NetworkLinkedList<VariableData> NetworkVars { get; }
            = MakeInitializer(Array.Empty<VariableData>());

        private bool _updateList;
        private ChangeDetector _changeDetector;

        private void Awake()
        {
            if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            switch (m_SyncMode)
            {
                case ListMode.SyncData:
                {
                    if (!VariableUtilities.IsSupportedType(list.TypeID.String))
                    {
                        Debug.LogWarning($"LocalListVariablesNetwork does not support type: { list.TypeID.String}.", gameObject);
                    }
                    list.Register(OnChange);
                    break;
                }
                case ListMode.PlayersList:
                {
                    if (list.TypeID.String != ValueGameObject.TYPE_ID.String)
                    {
                        Debug.LogWarning("LocalListVariablesNetwork in PlayersList mode only supports GameObjects.",
                            gameObject);
                        return;
                    }
                    NetworkCharacter.EventAvatarSpawned += AvatarSpawned;
                    NetworkCharacter.EventAvatarDespawned += AvatarDespawned;
                    break;
                }
                case ListMode.Attachments:
                {
                    if (list.TypeID.String != ValueGameObject.TYPE_ID.String)
                    {
                        Debug.LogWarning("LocalListVariablesNetwork in Attachments mode only supports GameObjects.",
                            gameObject);
                        return;
                    }
                    for(var i = 0; i < list.Count; i++)
                    {
                        var attachment = list.Get(i) as GameObject;
                        if (attachment) NetworkManager.RuntimeAttachments.TryAdd(attachment.name, attachment);
                    }
                    break;
                }
                case ListMode.Models:
                {
                    if (list.TypeID.String != ValueModelConfig.TYPE_ID.String)
                    {
                        Debug.LogWarning("LocalListVariablesNetwork in Models mode only supports ModelConfigs.",
                            gameObject);
                        return;
                    }
                    for(var i = 0; i < list.Count; i++)
                    {
                        if (list.Get(i) is ModelConfig model) NetworkManager.RuntimeModels.TryAdd(model.prefab.Get(gameObject).name, model);
                    }
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Update()
        {
            if (m_SyncMode != ListMode.PlayersList) return;
            if (!_updateList) return;
            if (!PlayerManager.Instance) return;
            _updateList = false;
            UpdatePlayerList();
        }

        private void OnDestroy()
        {
            switch (m_SyncMode)
            {
                case ListMode.SyncData:
                    list.Unregister(OnChange);
                    break;
                case ListMode.PlayersList:
                    NetworkCharacter.EventAvatarSpawned -= AvatarSpawned;
                    NetworkCharacter.EventAvatarDespawned -= AvatarDespawned;
                    break;
                case ListMode.Attachments:
                    break;
                case ListMode.Models:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void AvatarDespawned(NetworkCharacter networkCharacter)
        {
            _updateList = true;
        }

        private void AvatarSpawned(NetworkCharacter networkCharacter)
        {
            _updateList = true;
        }

        private void UpdatePlayerList()
        {
            if(!Application.isPlaying) return;
            if (m_SyncMode != ListMode.PlayersList) return;
            
            if(!PlayerManager.Instance)
            {
                Debug.LogWarning($"[PlayerManager] PlayerManager.Instance is null");
                return;
            }

            list.Clear();
            
            var index = 0;
            foreach (var pair in PlayerManager.Avatars)
            {
                if (pair.Value.gameObject)
                {
                    if (list.Get(index) != null) list.Set(index, pair.Value.gameObject);
                    else list.Insert(index, pair.Value.gameObject);
                }
                index++;
            }
        }

        public override void Spawned()
        {
            _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

            if(!HasStateAuthority) return;
            if(m_SyncMode != ListMode.SyncData) return;
            UpdateLocalList();
        }

        private void UpdateLocalList()
        {
            for(var index = 0; index < NetworkVars.Count; index++)
            {
                var data = NetworkVars.Get(index);
                var value = data.GetValue();
                if (list.Get(index) != null) list.Set(index, value);
                else list.Insert(index, value);
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
                        UpdateLocalList();
                        break;
                }
            }
        }

        private void OnChange(ListVariableRuntime.Change change, int index)
        {
            if(!Runner) return;
            if(!Runner.IsRunning) return;
            if(!HasStateAuthority) return;
            
            var data = list.Get(index);
            if (!data.IsAllowedType()) return;
            
            var varData = VariableData.ConvertFromObject(data);
            switch (change)
            {
                case ListVariableRuntime.Change.Set:
                    NetworkVars.Set(index, varData);
                    break;
                case ListVariableRuntime.Change.Move:
                    // _list.Move(index, index);
                    break;
                case ListVariableRuntime.Change.Insert:
                    NetworkVars.Set(index, varData);
                    break;
                case ListVariableRuntime.Change.Remove:
                    NetworkVars.Remove(varData);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(change), change, null);
            }
            Debug.Log($"OnListChange: {change} index: {index}={data} type: {data.GetType()} NetworkVars: {NetworkVars.Count} allowed: {data.IsAllowedType()}");
        }
    }
}