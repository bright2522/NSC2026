using System;
using UnityEngine;
using Fusion;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Characters.IK;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using GameCreator.Runtime.VisualScripting;
using UnityEngine.AI;
#if FUSION_ADDRESSABLES || FUSION_ENABLE_ADDRESSABLES
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;
using System.Threading.Tasks;
#endif

namespace NinjutsuGames.FusionNetwork.Runtime
{
    // [ScriptHelp(BackColor = EditorHeaderBackColor.Steel)]
    [AddComponentMenu("Game Creator/Fusion/Network Character")]
    [HelpURL("https://docs.ninjutsugames.com/game-creator-2/fusion-module/characters")]
    [RequireComponent(typeof(NetworkObject))]
    public class NetworkCharacter : RPCReceiver, IInputAuthorityGained, IStateAuthorityChanged
    {
        public static NetworkCharacter LocalPlayer { get; private set; }
        private const string LocalCharacterFormat = "{0}_Local_{1}";
        private const string RemoteCharacterFormat = "{0}_Remote_{1}";
        private const string UniqueVariablesIdFormat = "player_{0}-variables-{1}";
        private const string UniqueCharacterVariablesIdFormat = "character-{0}-variables-{1}";
        private const float MOVE_THRESHOLD = 0.1f;

        private bool updateFacing = true;
        private bool updateAnimin = false;
        [Networked] public Vector3 MoveDirection { get; set; }
        // [Networked] public Vector3 MovePosition { get; set; }
        [Networked] public Vector3 InputDirection { get; set; }
        [Networked] public Vector3 FaceDirection { get; set; }
        [Networked] private Vector3 RagdollPosition { get; set; }
        [Networked] private Quaternion RagdollRotation { get; set; }
        [Networked] private int JumpCount { get; set; }
        [Networked] public int Token { get; set; }
        [Networked] private NetworkId TrackingObject { get; set; }
        [Networked, OnChangedRender(nameof(CheckIsDead))] private NetworkBool IsDead { get; set; }
        [Networked, Capacity(10)] public NetworkDictionary<NetworkString<_32>, NetworkProp> PropList => default;
        [Networked, OnChangedRender(nameof(ChangeModel))] private NetworkString<_64> CurrentModel { get; set; }
        
        [Networked, OnChangedRender(nameof(ChangePrimaryTarget)), UnitySerializeField] private NetworkObject CombatPrimaryTarget { get; set; }
        
        [Networked, OnChangedRender(nameof(ChangeTargets)), UnitySerializeField] [Capacity(14)]
        private NetworkLinkedList<NetworkId> CombatTargets { get; } = MakeInitializer(Array.Empty<NetworkId>());
        public bool IsMoving => _character.Driver.WorldMoveDirection.magnitude > MOVE_THRESHOLD;
        public Character Character => _character;

        public static event Action<NetworkCharacter> EventAvatarSpawned;
        public static event Action<NetworkCharacter> EventAvatarDespawned;

        private Character _character;
        private ChangeDetector _changeDetector;
        private TUnitFacing _originalFacing;
        private TUnitFacing _prevFacing;
        private float _prevSpeed = -1;
        private string _originalName;
        
#if FUSION_ADDRESSABLES || FUSION_ENABLE_ADDRESSABLES
        private AsyncOperationHandle<GameObject> _handle;
#endif

        public void Awake()
        {
            _originalName = gameObject.name;
            CacheComponents();
            _originalFacing = _character.Kernel.Facing as TUnitFacing;
        }

        private void SetupIds()
        {
            var variables = GetComponentsInChildren<LocalNameVariables>();
            var i = 0;
            foreach (var variable in variables)
            {
                var id = Object.InputAuthority.IsNone ? string.Format(UniqueCharacterVariablesIdFormat, Object.Id.Raw, i) : string.Format(UniqueVariablesIdFormat, Object.InputAuthority.PlayerId, i);
                variable.ChangeId(new IdString(id));
                i++;
            }
        }

        public override async void Spawned()
        {
            _changeDetector = GetChangeDetector(ChangeDetector.Source.SnapshotTo);
            CacheComponents();
            SetupIds();
            SetupGameCreator();
            SetupEvents();

#if UNITY_EDITOR
            FormatName();
#endif
            if (Object.InputAuthority == Runner.LocalPlayer)
            {
                LocalPlayer = this;
            }

            if (Runner.GameMode == GameMode.Single)
            {
                var nt = GetComponent<NetworkTransform>();
                if(nt) nt.enabled = false;
            }

            ChangeModel();
            CheckTrackingObject();
            CheckIsDead();
            CheckAttachments();

            await Awaitable.NextFrameAsync();
            
            PlayerManager.LastJoinedPlayer = this;
            if(!Object.InputAuthority.IsNone) EventAvatarSpawned?.Invoke(this);
            
            await Awaitable.NextFrameAsync();
            TryRunCachedRpcsOnce();
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            ResetTargets();
            RemoveEvents();
            PlayerManager.LastLeftPlayer = this;
            if (!Object.InputAuthority.IsNone) EventAvatarDespawned?.Invoke(this);
#if FUSION_ADDRESSABLES || FUSION_ENABLE_ADDRESSABLES
            if(_handle.IsValid()) _handle.Release();
#endif
        }

        private void SetupEvents()
        {
            _character.Interaction.EventFocus += OnFocus;
            _character.Interaction.EventInteract += OnInteract;
            _character.Interaction.EventBlur += OnBlur;
            _character.EventJump += OnJump;
            _character.EventDie += OnDie;
            _character.EventRevive += OnRevive;
            _character.Props.EventAdd += OnAddProp;
            _character.Props.EventRemove += OnRemoveProp;
            _character.EventAfterChangeModel += OnChangeModel;
            _character.EventLand += OnLand;
            _character.Combat.Targets.EventCandidateAdd += OnAddTarget;
            _character.Combat.Targets.EventCandidateRemove += OnRemoveTarget;
            _character.Combat.Targets.EventChangeTarget += OnChangeTarget;
        }

        private void RemoveEvents()
        {
            _character.Interaction.EventFocus -= OnFocus;
            _character.Interaction.EventInteract -= OnInteract;
            _character.Interaction.EventBlur -= OnBlur;
            _character.EventJump -= OnJump;
            _character.EventDie -= OnDie;
            _character.EventRevive -= OnRevive;
            _character.Props.EventAdd -= OnAddProp;
            _character.Props.EventRemove -= OnRemoveProp;
            _character.EventAfterChangeModel -= OnChangeModel;
            _character.EventLand -= OnLand;
            _character.Combat.Targets.EventCandidateAdd -= OnAddTarget;
            _character.Combat.Targets.EventCandidateRemove -= OnRemoveTarget;
            _character.Combat.Targets.EventChangeTarget -= OnChangeTarget;
        }

        private void SetupGameCreator()
        {
            if (HasInputAuthority)
            {
                _character.IsPlayer = true;
                _character.Player.IsControllable = true;
            }
            else
            {
                _character.Player.IsControllable = false;
                _character.enabled = false;
                _character.enabled = true;
            }
            
            if(_prevSpeed > 0) _character.Motion.AngularSpeed = _prevSpeed;

            if (Runner.Topology != Topologies.Shared && Runner.Mode != SimulationModes.Client)
            {
                _prevSpeed = _character.Motion.AngularSpeed;
                _character.Motion.AngularSpeed *= HasStateAuthority ? 4 : 5;
            }

            if (!HasStateAuthority)
            {
                if(_character.Kernel.Facing is not UnitFacingFusionDirection)
                {
                    _character.enabled = false;
                    _prevFacing = _character.Kernel.Facing as TUnitFacing;
                    _character.Kernel.ChangeFacing(_character, new UnitFacingFusionDirection { useFusionDeltaTime = Runner.Topology != Topologies.Shared });
                    _character.enabled = true;
                }
            }
            else
            {
                if(_prevFacing != null) _character.Kernel.ChangeFacing(_character, _prevFacing);
                else if(_originalFacing != null) _character.Kernel.ChangeFacing(_character, _originalFacing);
            }
            
            if(_character.Driver is UnitDriverNavmesh navmeshDriver)
            {
                var agent = gameObject.Get<NavMeshAgent>();
                agent.enabled = false;
                agent.enabled = true;
                navmeshDriver.OnStartup(_character);
            }
            
            /*if((!Object.HasInputAuthority && (Runner.IsServer && !Runner.IsClient)) && _character.Kernel.Player is not UnitFusionPlayerDirectional)
            {
                _character.Kernel.ChangePlayer(_character, new UnitFusionPlayerDirectional());
            }*/
        }

#if UNITY_EDITOR
        private void FormatName()
        {
            var suffix = Runner.Mode.ToString();
            if (Object.InputAuthority.IsNone)
            {
                gameObject.name = string.Format(HasStateAuthority ? LocalCharacterFormat : RemoteCharacterFormat,
                    _originalName, suffix);
            }
            else
            {
                gameObject.name = string.Format(HasInputAuthority ? LocalCharacterFormat : RemoteCharacterFormat,
                    Object.InputAuthority.ToString(), suffix);
            }
        }
#endif

        private void OnValidate()
        {
            if (Application.isPlaying) return;
            CacheComponents();
            if (_character) _character.IsPlayer = false;
        }

        private void CacheComponents()
        {
            TryGetComponent(out _character);
            // TryGetComponent(out _controller);
        }

        public override void FixedUpdateNetwork()
        {
            if (HasInputAuthority)
            {
                // RootMotion = _character.Animim.RootMotionDeltaPosition;
                InputDirection = _character.Player.InputDirection;
            }
            
            if (Runner.Topology != Topologies.Shared)
            {
                if (GetInput<NetworkInputData>(out var input))
                {
                    MoveDirection = input.MoveDirection;
                    FaceDirection = input.FaceDirection;
                    JumpCount = input.JumpCount;
                }

                if (updateFacing)
                {
                    _character.Kernel.Facing.OnUpdate();
                }

                if (updateAnimin)
                {
                    _character.Animim.OnUpdate();
                }
            }
            else if (HasStateAuthority)
            {
                MoveDirection = IsMoving ? _character.Driver.WorldMoveDirection : Vector3.zero;

                if (_character.Ragdoll.IsRagdoll)
                {
                    RagdollPosition = _character.Animim.Animator.transform.position;
                    RagdollRotation = _character.Animim.Animator.transform.rotation;
                }
            }

            if (!HasStateAuthority)
            {
                if (_character.Ragdoll.IsRagdoll)
                {
                    _character.Animim.Animator.transform.position = RagdollPosition;
                    _character.Animim.Animator.transform.rotation = RagdollRotation;
                }
            }
            
            if (_character.Driver is UnitDriverFusionController controller)
            {
                controller.FixedUpdateNetwork();
            }
        }

        public override void Render()
        {
            // Only move own player and not every other player. Each player controls its own player object.
            if (!HasInputAuthority)
            {
                ((UnitMotionController)_character.Motion).MoveDirection = MoveDirection;
            }

            // Attempt to replay cached RPCs once after replication catches up for late joiners
            TryRunCachedRpcsOnce();

            foreach (var change in _changeDetector.DetectChanges(this))
            {
                if (change != nameof(JumpCount)) continue;
                if (!HasInputAuthority && JumpCount > 0)
                {
                    // Debug.LogWarning($"JumpCount: {JumpCount} auth: {HasStateAuthority} input: {HasInputAuthority} player: {Object.InputAuthority}");
                    _character.Motion.ForceJump();
                }
            }
        }

        #region Character Events
        
        private void OnLand(float obj)
        {
            if (HasStateAuthority)
            {
                JumpCount = 0;
            }
        }

        private void OnJump(float jumpForce)
        {
            if (!HasStateAuthority) return;
            JumpCount++;
        }

        private void OnInteract(Character character, IInteractive interactive)
        {
        }

        private void OnBlur(Character character, IInteractive interactive)
        {
            if (!HasStateAuthority) return;
            
            // Debug.LogWarning($"OnBlur {interactive?.Instance} Interaction Target: {character.Interaction.Target} character: {character}");

            if (character.Interaction.Target == null) return;
            if (!character.Interaction.Target.Instance) return;
            if (interactive == null) return;

            var obj = character.Interaction.Target.Instance;
            if (!obj) return;
            var no = obj.Get<NetworkObject>();
            if(!no) return;

            var n = new NetworkId
            {
                Raw = 0
            };
            TrackingObject = n;
            RPC_UntrackObject(no.Id);
        }

        private void OnFocus(Character character, IInteractive interactive)
        {
            if (!HasStateAuthority) return;
            
            // Debug.LogWarning($"OnFocus {interactive?.Instance} Interaction Target: {character.Interaction.Target} character: {character}");

            if (character.Interaction.Target == null) return;
            if (!character.Interaction.Target.Instance) return;
            if (interactive == null) return;

            var obj = character.Interaction.Target.Instance;
            if (!obj) return;
            var no = obj.Get<NetworkObject>();
            if(!no) return;
            if(!obj.Get<Hotspot>()) return;
            
            TrackingObject = no.Id;
            RPC_TrackObject(no.Id);
        }

        private void OnStopInteraction(Character character, IInteractive interactive)
        {
            Debug.LogWarning($"OnStopInteraction {interactive?.Instance}");
        }

        private void OnStartInteraction(Character character, IInteractive interactive)
        {
            Debug.LogWarning($"OnStartInteraction {interactive?.Instance}");
        }

        #endregion
        
        #region Combat Targets

        private void CheckTargets()
        {
            foreach (var target in CombatTargets)
            {
                var no = Runner.FindObject(target);
                if (!no) continue;
                if (!no.gameObject) continue;
                if(_character.Combat.Targets.List.Contains(no.gameObject)) continue;
                _character.Combat.Targets.AddCandidate(no.gameObject);
            }   
            
            if (CombatPrimaryTarget && CombatPrimaryTarget.IsValid)
            {
                _character.Combat.Targets.Primary = CombatPrimaryTarget.gameObject;
            }
        }
        
        private void ChangeTargets()
        {
            if(HasStateAuthority) return;

            CheckTargets();
        }
        
        private void ChangePrimaryTarget()
        {
            if(HasStateAuthority) return;

            if (!CombatPrimaryTarget) return;
            if (!CombatPrimaryTarget.IsValid) return;
            _character.Combat.Targets.Primary = CombatPrimaryTarget.gameObject;
        }
        
        private void OnAddTarget(GameObject target)
        {
            if (!HasStateAuthority) return;
            var no = target.Get<NetworkObject>();
            if (!no) return;
            if(CombatTargets.Count >= CombatTargets.Capacity) return;
            if(!CombatTargets.Contains(no.Id)) CombatTargets.Add(no.Id);
        }
        
        private void OnRemoveTarget(GameObject target)
        {
            if (!HasStateAuthority) return;
            var no = target.Get<NetworkObject>();
            if (!no) return;
            if(CombatTargets.Contains(no.Id)) CombatTargets.Remove(no.Id);
        }
        
        private void OnChangeTarget(GameObject target)
        {
            if (!HasStateAuthority) return;
            var no = target.Get<NetworkObject>();
            if (!no) return;
            CombatPrimaryTarget = no;
        }
        
        private void ResetTargets()
        {
            _character.Combat.Targets.Primary = null;
            _character.Combat.Targets.List.Clear();
            CombatTargets.Clear();
            CombatPrimaryTarget = default;
        }
        
        #endregion
        
        #region Model

        public void SetCurrentModel(string model)
        {
            CurrentModel = model;
        }
        
        private void OnChangeModel()
        {
            if (!HasStateAuthority) return;
            CurrentModel = _character.Animim.Animator.gameObject.name;
        }
        
        private void ChangeModel()
        {
            var model = CurrentModel.Value;
            if(string.IsNullOrEmpty(model)) return;
            
            if (!NetworkManager.RuntimeModels.TryGetValue(model, out var config))
            {
                // Debug.LogWarning($"Could not find model with name {model}");
#if FUSION_ADDRESSABLES || FUSION_ENABLE_ADDRESSABLES
                if(!string.IsNullOrEmpty(model)) _ = LoadAddressable(model);
#endif
                return;
            }
            var options = new Character.ChangeOptions
            {
                materials = config.materialSounds,
                offset = config.offset
            };
            _character.ChangeModel(config.prefab.Get(gameObject), options);
        }
        
#if FUSION_ADDRESSABLES || FUSION_ENABLE_ADDRESSABLES
        private async Task LoadAddressable(string id)
        {
            _handle = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>(id);
            await _handle.Task;
            
            if(_handle.IsValid() && _handle.Status == AsyncOperationStatus.Succeeded)
            {
                _character.ChangeModel(_handle.Result, new Character.ChangeOptions());
            }
        }
#endif
        
        #endregion

        #region Attachments

        private void CheckAttachments()
        {
            foreach (var prop in PropList)
            {
                if (TryGetBone(prop.Key.Value, out var bone)) return;
                _character.Props.AttachPrefab(new Bone(bone), prop.Value.GetProp(), prop.Value.position, prop.Value.rotation);
            }
        }

        private static bool TryGetBone(string prop, out HumanBodyBones bone)
        {
            if (Enum.TryParse(prop, out bone)) return false;
            bone = BoneFinder.FindClosestBone(prop);
            return bone == HumanBodyBones.LastBone;
        }

        private void OnRemoveProp(Transform bone)
        {
            if (!bone) return;
            if (!HasStateAuthority) return;

            if(PropList.ContainsKey(bone.name)) PropList.Remove(bone.name);
            else
            {
                // check if bone name is prop name
                foreach (var prop in PropList)
                {
                    if (prop.Value.propName != bone.name) continue;
                    PropList.Remove(prop.Key);
                    break;
                }
            }
            RPC_RemoveProp(bone.name);
        }

        /// <summary>
        /// Check if the character has attached a prop to a bone and if so, send it to the network.
        /// </summary>
        /// <param name="bone"></param>
        /// <param name="attachment"></param>
        private void OnAddProp(Transform bone, GameObject attachment)
        {
            if (!bone) return;
            if (!HasStateAuthority) return;
            var propName = attachment.name.Replace("(Clone)", string.Empty);
            var localPosition = attachment.transform.localPosition;
            var localRotation = attachment.transform.localRotation;

            PropList.Add(bone.name, new NetworkProp { propName = propName, position = localPosition, rotation = localRotation });
            RPC_AttachProp(bone.name);
        }
        
        [Rpc(RpcSources.All, RpcTargets.Proxies, HostMode = RpcHostMode.SourceIsHostPlayer)]
        private void RPC_AttachProp(NetworkString<_32> boneName)
        {
            if(PropList.TryGet(boneName, out var prop))
            {
                _character.Props.AttachPrefab(
                    TryGetBone(boneName.Value, out var bone) ? new Bone(boneName.Value) : new Bone(bone),
                    prop.GetProp(),
                    prop.position, prop.rotation);
            }
            else
            {
                Debug.LogWarning($"[NetworkCharacter] AttachProp: Could not find prop with name {boneName.Value} in {gameObject.name}", gameObject);
            }
        }
        
        [Rpc(RpcSources.All, RpcTargets.Proxies, HostMode = RpcHostMode.SourceIsHostPlayer)]
        private void RPC_RemoveProp(NetworkString<_32> boneName)
        {
            if (TryGetBone(boneName.Value, out var bone)) return;
            _character.Props.RemoveAtBone(new Bone(bone));
        }

        #endregion

        #region Ragdoll

        private void CheckIsDead()
        {
            _character.IsDead = IsDead;
        }
        
        private void OnDie()
        {
            if (!HasStateAuthority) return;

            IsDead = true;
            _character.IsDead = true;
        }

        private void OnRevive()
        {
            if (!HasStateAuthority) return;

            IsDead = false;
            _character.IsDead = false;
        }

        #endregion

        #region Tracking Object

        private void CheckTrackingObject()
        {
            if(TrackingObject.IsValid)
            {
                TrackObject(TrackingObject);
            }
        }

        [Rpc(RpcSources.All, RpcTargets.Proxies, HostMode = RpcHostMode.SourceIsHostPlayer)]
        private void RPC_TrackObject(NetworkId id, RpcInfo info = default)
        {
            TrackObject(id);
        }

        private void TrackObject(NetworkId id)
        {
            var trackingObject = Runner.FindObject(id);
            if(!trackingObject)
            {
                Debug.LogWarning($"Could not find tracking object with id {id}");
                return;
            }
            var rig = _character.IK.GetRig<RigLookTo>();
            rig?.SetTarget(new LookToTransform(
                0, 
                trackingObject.transform, 
                Vector3.zero
            ));
        }

        [Rpc(RpcSources.All, RpcTargets.Proxies, HostMode = RpcHostMode.SourceIsHostPlayer)]
        private void RPC_UntrackObject(NetworkId id, RpcInfo info = default)
        {
            var trackingObject = Runner.FindObject(id);
            if(!trackingObject)
            {
                Debug.LogWarning($"Could not find tracking object with id {id}");
                return;
            }
            
            IInteractive tracker = InteractionTracker.Require(trackingObject.gameObject);

            var rig = _character.IK.GetRig<RigLookTo>();
            rig?.RemoveTarget(new LookToTransform(
                0, 
                trackingObject.transform, 
                Vector3.zero
            ));
            tracker.Stop();
        }

        #endregion

        public void InputAuthorityGained()
        {
            if(debug) Debug.LogWarning($"Avatar InputAuthorityGained: {Object.InputAuthority}", gameObject);

            if (Object.InputAuthority == Runner.LocalPlayer)
            {
                LocalPlayer = this;
            }
            
            SetupIds();
            SetupGameCreator();
            
#if UNITY_EDITOR
            FormatName();
#endif
            EventAvatarSpawned?.Invoke(this);
        }

        public void StateAuthorityChanged()
        {
            if(debug) Debug.LogWarning($"Avatar StateAuthorityChanged: {Object.StateAuthority}", gameObject);

            SetupGameCreator();
            CheckIsDead();
#if UNITY_EDITOR
            FormatName();
#endif

            if (!HasStateAuthority &&
                Object.StateAuthority == PlayerRef.None &&
                (Object.Flags & NetworkObjectFlags.AllowStateAuthorityOverride) == NetworkObjectFlags.AllowStateAuthorityOverride &&
                Runner.IsSharedModeMasterClient)
            {
                if(debug) Debug.LogWarning($"Avatar StateAuthority Requested: {gameObject}", gameObject);
                Object.RequestStateAuthority();
            }
        }
    }
}