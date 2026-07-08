using Fusion;
using GameCreator.Runtime.Characters;
using UnityEngine.Serialization;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    using System.Runtime.InteropServices;
    using UnityEngine;

    [StructLayout(LayoutKind.Explicit)]
    // [NetworkStructWeaved(WORDS + 4)]
    public unsafe struct NetworkCCData : INetworkStruct
    {
        public const int WORDS = NetworkTRSPData.WORDS + 4;
        // public const int SIZE = WORDS * 4;

        [FieldOffset(0)] public NetworkTRSPData TRSPData;

        // [FieldOffset((NetworkTRSPData.WORDS + 0) * Allocator.REPLICATE_WORD_SIZE)]
        // int _grounded;

        // [FieldOffset((NetworkTRSPData.WORDS + 1) * Allocator.REPLICATE_WORD_SIZE)]
        // Vector3Compressed _velocityData;

        /*public bool Grounded
        {
            get => _grounded == 1;
            set => _grounded = (value ? 1 : 0);
        }

        public Vector3 Velocity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _velocityData;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _velocityData = value;
        }*/
    }

    [DisallowMultipleComponent]
    // [RequireComponent(typeof(CharacterController))]
    [NetworkBehaviourWeaved(NetworkCCData.WORDS)]
    // ReSharper disable once CheckNamespace
    public sealed unsafe class NetworkCharacterController : NetworkTRSP, INetworkTRSPTeleport, IBeforeAllTicks,
        IAfterAllTicks, IBeforeCopyPreviousState
    {
        private new ref NetworkCCData Data => ref ReinterpretState<NetworkCCData>();

        public Character character;

        public bool controlCC = false;
        [FormerlySerializedAs("controlCCColllisions")] public bool controlCCCollisions = false;
        public bool disableSharedModeInterpolation = true;
        public bool useRootMotionPosition = false;
        public bool useRootMotionRotation = false;
        public float fixedDeltaTime = 0.02f;
        public bool useTransformRotation = false;
        public bool useSmoothDamp = false;
        
        public float smoothSpeed = 0.125f; // Smoothing speed
        
        private Vector3 _velocity = Vector3.zero; // velocity for smoothdamp
        
        [Networked] public Vector3 RootMotionDeltaPosition { get; set; }
        [Networked] public Quaternion RootMotionDeltaRotation { get; set; }
        // [Header("Character Controller Settings")]
        // public float gravity = -20.0f;
        //
        // public float jumpImpulse = 8.0f;
        // public float acceleration = 10.0f;
        // public float braking = 10.0f;
        // public float maxSpeed = 2.0f;
        // public float rotationSpeed = 15.0f;

        private Tick _initial;
        private CharacterController _controller;
        private Character _character;
        private NetworkCharacter _networkCharacter;

        /*public Vector3 Velocity
        {
            get => Data.Velocity;
            set => Data.Velocity = value;
        }
        public bool Grounded
        {
            get => Data.Grounded;
            set => Data.Grounded = value;
        }*/

        public void Teleport(Vector3? position = null, Quaternion? rotation = null)        {
            _controller.enabled = false;
            NetworkTRSP.Teleport(this, transform, position, rotation);
            _controller.enabled = true;
        }


        public void Jump(bool ignoreGrounded = false, float? overrideImpulse = null)
        {
            /*if (Data.Grounded || ignoreGrounded)
            {
                var newVel = Data.Velocity;
                newVel.y += overrideImpulse ?? jumpImpulse;
                Data.Velocity = newVel;
            }*/
        }

        public void Move(Vector3 direction)
        {
            /*var deltaTime = Runner.DeltaTime;
            var previousPos = transform.position;
            var moveVelocity = Data.Velocity;

            direction = direction.normalized;

            if (Data.Grounded && moveVelocity.y < 0)
            {
                moveVelocity.y = 0f;
            }*/

            /*moveVelocity.y += gravity * Runner.DeltaTime;

            var horizontalVel = default(Vector3);
            horizontalVel.x = moveVelocity.x;
            horizontalVel.z = moveVelocity.z;

            if (direction == default)
            {
                horizontalVel = Vector3.Lerp(horizontalVel, default, braking * deltaTime);
            }
            else
            {
                horizontalVel = Vector3.ClampMagnitude(horizontalVel + direction * acceleration * deltaTime, maxSpeed);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction),
                    rotationSpeed * Runner.DeltaTime);
            }

            moveVelocity.x = horizontalVel.x;
            moveVelocity.z = horizontalVel.z;

            _controller.Move(moveVelocity * deltaTime);*/

            // Data.Velocity = (transform.position - previousPos) * Runner.TickRate;
            // Data.Grounded = _controller.isGrounded;
        }

        public override void Spawned()
        {
            _initial = default;
            TryGetComponent(out _controller);
            TryGetComponent(out _character);
            CopyToBuffer();
        }

        public override void FixedUpdateNetwork()
        {
            if(!HasStateAuthority) return;

            RootMotionDeltaPosition = _character.Animim.RootMotionDeltaPosition / fixedDeltaTime;
            RootMotionDeltaRotation = _character.Animim.RootMotionDeltaRotation;
        }

        public override void Render()
        {
            if (Runner.Mode == SimulationModes.Server || Runner.Topology == Topologies.Shared && disableSharedModeInterpolation && HasStateAuthority)
                return;
            
            // Apply root motion during rendering if using root motion
            if (useRootMotionPosition) transform.position += RootMotionDeltaPosition * fixedDeltaTime;
            if(useRootMotionRotation) transform.rotation *= RootMotionDeltaRotation;
            
            NetworkTRSP.Render(this, transform, false, false, false, ref _initial);
        }

        void IBeforeAllTicks.BeforeAllTicks(bool resimulation, int tickCount)
        {
            if (Runner.Mode == SimulationModes.Server || Runner.Topology == Topologies.Shared && disableSharedModeInterpolation && HasStateAuthority)
                return;
            
            CopyToEngine();
        }

        void IAfterAllTicks.AfterAllTicks(bool resimulation, int tickCount)
        {
            CopyToBuffer();
        }

        void IBeforeCopyPreviousState.BeforeCopyPreviousState()
        {
            CopyToBuffer();
        }

        private void Awake()
        {
            TryGetComponent(out _controller);
            TryGetComponent(out _character);
            TryGetComponent(out _networkCharacter);
        }

        private void CopyToBuffer()
        {
            /*var rootVelocity = (Data.Velocity) / fixedDeltaTime;
            // Debug.LogWarning($"CopyToBuffer rootVelocity: {rootVelocity} rootMotionDelta: {_character.Animim.RootMotionDeltaPosition} rootMotion: {_networkAvatar.RootMotion} vel: {Data.Velocity}", gameObject);
            Data.TRSPData.Position = useRootMotion ? transform.position + (rootVelocity * fixedDeltaTime) : transform.position;
            
            if (useRootMotion)
            {
                Quaternion incrementalRotation = Quaternion.LerpUnclamped(Quaternion.identity, Quaternion.Euler(_character.Animim.RootMotionDeltaPosition), fixedDeltaTime);
                Data.TRSPData.Rotation = transform.rotation * incrementalRotation;
            }
            else Data.TRSPData.Rotation = transform.rotation;*/

            // var pos = transform.position;
            var rot = transform.rotation;
            if (useRootMotionPosition) transform.localPosition += RootMotionDeltaPosition * fixedDeltaTime;
            if (useRootMotionRotation) rot *= RootMotionDeltaRotation;
            Data.TRSPData.Position = transform.position;
            Data.TRSPData.Rotation = rot;
            
            // Data.TRSPData.Rotation = useRootMotion ? transform.rotation * _character.Animim.RootMotionDeltaRotation : transform.rotation;
        }

        private void CopyToEngine()
        {
            // CC must be disabled before resetting the transform state
            if(controlCC) _controller.enabled = false;
            if(controlCCCollisions) _controller.detectCollisions = false;

            /*var fdt = useFusionDeltaTime ? Runner.DeltaTime : fixedDeltaTime;
            if (useRootMotion)
            {
                var rootVelocity = (Data.Velocity) / fdt;
                Debug.LogWarning(
                    $"CopyToEngine rootVelocity: {rootVelocity} rootMotionDelta: {_character.Animim.RootMotionDeltaPosition} rootMotion: {_networkAvatar.RootMotion} vel: {Data.Velocity}",
                    gameObject);
                Data.TRSPData.Position += rootVelocity * fdt;
                
                Quaternion incrementalRotation = Quaternion.LerpUnclamped(Quaternion.identity, Quaternion.Euler(_character.Animim.RootMotionDeltaPosition), fdt);
                Data.TRSPData.Rotation *= incrementalRotation;
            }*/

            // set position and rotation
            if(useTransformRotation) transform.SetPositionAndRotation(Data.TRSPData.Position, transform.rotation);
            else
            {
                transform.SetLocalPositionAndRotation(Data.TRSPData.Position, Data.TRSPData.Rotation);
            }

            

            // Re-enable CC
            if(controlCC) _controller.enabled = true;
            if(controlCCCollisions) _controller.detectCollisions = true;
        }
    }
}