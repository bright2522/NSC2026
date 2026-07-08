using System;
using Fusion;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Fusion Character Controller")]
    [Image(typeof(IconCapsuleSolid), ColorTheme.Type.Green, typeof(OverlayBolt))]
    
    [Category("Fusion Character Controller")]
    [Description("Moves the Character using Unity's default Character Controller and synchronize it with the Fusion Network.")]
    
    [Serializable]
    public class UnitDriverFusionController : TUnitDriver
    {
        private const float MAX_SLOPE_SLIDE_FROM_CHARACTER = 90;
        private const float EPSILON_SLIDE_FROM_CHARACTER = 0.001f;
        
        private const float VELOCITY_INTERVAL = 0.1f;
        
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        protected bool useFusionFixedUpdate = true;
        protected bool useFusionTime = true;
        protected bool updatePhysicsInFusion = true;
        protected bool updatePropertiesInFusion = true;
        protected bool updateTranslationInFusion = true;
        protected bool updateTranslationInUpdate = true;
        
        [SerializeField] protected float m_SkinWidth = 0.08f;
        [SerializeField] protected float m_PushForce = 1.0f;
        [SerializeField] protected float m_MaxSlope = 45f;
        [SerializeField] protected float m_StepHeight = 0.3f;
        [SerializeField] private Axonometry m_Axonometry = new();

        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized] protected CharacterController m_Controller;

        [NonSerialized] protected Vector3 m_MoveDirection;
        [NonSerialized] protected float m_VerticalSpeed;
 
        [NonSerialized] protected AnimVector3 m_FloorNormal;
 
        [NonSerialized] protected int m_GroundFrame = -100;
        [NonSerialized] protected float m_GroundTime = -100f;
        [NonSerialized] protected float m_JumpTime = -100f;

        [NonSerialized] private DriverControllerComponent m_Helper;
        [NonSerialized] private DriverAdditionalTranslation m_AddTranslation;
        
        [NonSerialized] private Vector3 m_PreviousPosition;
        [NonSerialized] private Vector3 m_Velocity;
        
        [NonSerialized] private Vector3 m_AccumulatedDisplacement;
        [NonSerialized] private float m_AccumulatedTime;
        
        [NonSerialized] private Vector3 m_SlideFromCharacter;
        [NonSerialized] private int m_FrameSlideFromCharacter;
        
        [NonSerialized] private bool m_IsOnSteepSlope;
        
        private NetworkObject networkObject;
        private float _deltaTime;
        private Vector3 _previousPosition;
        // private Vector3 _velocity;
        // private NetworkCharacterController m_NetworkController;


        // INTERFACE PROPERTIES: ------------------------------------------------------------------

        public override Vector3 WorldMoveDirection => m_Controller ? m_Velocity : Vector3.zero;
        public override Vector3 LocalMoveDirection => Transform.InverseTransformDirection(
            WorldMoveDirection
        );

        public override float SkinWidth => m_Controller != null 
            ? m_Controller.skinWidth
            : 0f;
        
        public override bool IsGrounded
        {
            get
            {
                if (m_Controller == null) return false;
                if (m_ForceGrounded) return true;

                var inSlideFrame = m_FrameSlideFromCharacter < Time.frameCount;
                return m_Controller.isGrounded && inSlideFrame && !m_IsOnSteepSlope;
            }
        }
        
        public override Vector3 FloorNormal => m_FloorNormal.Current;

        public override bool Collision
        {
            get => m_Controller.detectCollisions;
            set => m_Controller.detectCollisions = value;
        }

        public override Axonometry Axonometry
        {
            get => m_Axonometry;
            set => m_Axonometry = value;
        }

        // private bool UseFusionPhysics => networkObject.Runner.Config.PhysicsEngine != NetworkProjectConfig.PhysicsEngines.None;
        private bool UseFusionPhysics => useFusionFixedUpdate && networkObject.Runner.Topology != Topologies.Shared;// && networkObject.Runner.Topology != Topologies.Shared && networkObject.Runner.Mode != SimulationModes.Server;
        private float CharacterTime => networkObject.Runner.LocalRenderTime + networkObject.Runner.DeltaTime;
        // private int CharacterFrame => UseFusionPhysics ? networkObject.Runner.Tick.Raw : Time.frameCount;

        // INITIALIZERS: --------------------------------------------------------------------------

        public UnitDriverFusionController()
        {
            m_MoveDirection = Vector3.zero;
            m_VerticalSpeed = 0f;
            
            m_SlideFromCharacter = Vector3.zero;
            m_FrameSlideFromCharacter = -1;
        }
        
        public override void OnEnable()
        {
            if (Character != null)
            {
                m_GroundTime = Character.Time.Time;
                m_GroundFrame = Character.Time.Frame;   
                m_Velocity = Vector3.zero;
                m_PreviousPosition = Transform.localPosition;
                
                m_PreviousPosition = Transform.localPosition;
                
                if (Transform.parent != null)
                {
                    var parentScale = Transform.parent.localScale;
                    m_PreviousPosition = Vector3.Scale(m_PreviousPosition, parentScale);
                }
            }
        }
        
        public override void OnStartup(Character character)
        {
            base.OnStartup(character);
            
            networkObject = character.Get<NetworkObject>();
            
            if(!networkObject.HasInputAuthority) updatePhysicsInFusion = false;

            m_FloorNormal = new AnimVector3(Vector3.up, 0.15f);

            m_Controller = Character.GetComponent<CharacterController>();
            if (m_Controller == null)
            {
                var instance = Character.gameObject;
                m_Controller = instance.AddComponent<CharacterController>();
                m_Controller.hideFlags = HideFlags.HideInInspector;
                
                var height = Character.Motion.Height;
                var radius = Character.Motion.Radius;
                
                m_Controller.height = height;
                m_Controller.radius = radius;
                m_Controller.center = Vector3.zero;  
                
                m_Controller.skinWidth = m_SkinWidth;
                m_Controller.slopeLimit = m_MaxSlope;
                m_Controller.stepOffset = m_StepHeight;
                m_Controller.minMoveDistance = 0f;
            }
            
            m_Helper = DriverControllerComponent.Register(
                Character,
                OnControllerColliderHit
            );

            character.Ragdoll.EventBeforeStartRagdoll += OnStartRagdoll;
            character.Ragdoll.EventAfterStartRecover += OnEndRagdoll;
        }

        public override void OnDispose(Character character)
        {
            base.OnDispose(character);

            Object.Destroy(m_Helper);
            Object.Destroy(m_Controller);
            
            character.Ragdoll.EventBeforeStartRagdoll -= OnStartRagdoll;
            character.Ragdoll.EventAfterStartRecover -= OnEndRagdoll;
        }

        // UPDATE METHODS: ------------------------------------------------------------------------

        public void FixedUpdateNetwork()
        {
            if (!UseFusionPhysics) return;

            if (Character.IsDead) return;
            if(m_Controller == null) return;

            if (updatePropertiesInFusion) UpdateProperties(false);
            if(updateTranslationInFusion) UpdateTranslation(Character.Motion, useFusionTime);
            if(updatePhysicsInFusion) UpdatePhysicProperties();

        }

        public override void OnUpdate()
        {
            // if (UseFusionPhysics) return;

            if (Character.IsDead) return;
            if(m_Controller == null) return;
            
            if (!updatePropertiesInFusion) UpdateProperties(false);

            UpdateGravity(Character.Motion, true);
            UpdateJump(Character.Motion, true);

            if(updateTranslationInUpdate) UpdateTranslation(Character.Motion, false);
            m_Axonometry?.ProcessPosition(this, Transform.position);
            
            var currentPosition = Transform.localPosition;
            if (Transform.parent != null)
            {
                var parentScale = Transform.parent.localScale;
                currentPosition = Vector3.Scale(currentPosition, parentScale);
            }
            
            m_AccumulatedDisplacement += currentPosition - m_PreviousPosition;
            m_AccumulatedTime += Character.Time.DeltaTime;
            
            if (m_AccumulatedTime >= VELOCITY_INTERVAL)
            {
                var localVelocity = m_AccumulatedDisplacement / m_AccumulatedTime;
                m_Velocity = Transform.parent != null
                    ? Transform.parent.TransformDirection(localVelocity)
                    : localVelocity;
                
                m_AccumulatedDisplacement = Vector3.zero;
                m_AccumulatedTime = 0f;
            }

            m_PreviousPosition = currentPosition;
        }

        public override void OnFixedUpdate()
        {
            if (updatePhysicsInFusion) return;
            if (Character.IsDead) return; 
            if(m_Controller == null) return;
            
            base.OnFixedUpdate();
            UpdatePhysicProperties();
        }

        protected virtual void UpdateProperties(bool useFusion)
        {
            var deltaTime = useFusion ? networkObject.Runner.DeltaTime : Character.Time.DeltaTime;
            m_FloorNormal.UpdateWithDelta(deltaTime);
            m_MoveDirection = Vector3.zero;
            
            var floorAngle = Vector3.Angle(FloorNormal, Vector3.up);
            m_IsOnSteepSlope = IsGrounded && floorAngle > m_MaxSlope;
            
            if (Math.Abs(m_Controller.skinWidth - m_SkinWidth) > float.Epsilon)
            {
                m_Controller.skinWidth = m_SkinWidth;
            }
            
            if (Math.Abs(m_Controller.slopeLimit - m_MaxSlope) > float.Epsilon)
            {
                m_Controller.slopeLimit = m_MaxSlope;
            }
            
            if (Math.Abs(m_Controller.stepOffset - m_StepHeight) > float.Epsilon)
            {
                m_Controller.stepOffset = m_StepHeight;
            }
        }
        
        protected virtual void UpdatePhysicProperties()
        {
            var height = Character.Motion.Height;
            var radius = Character.Motion.Radius;

            if (Math.Abs(m_Controller.height - height) > float.Epsilon)
            {
                var offset = (m_Controller.height - height) * 0.5f;
                
                Transform.localPosition += Vector3.down * offset;
                m_Controller.height = height;
                
                Character.Animim.ApplyMannequinPosition();
            }

            if (Math.Abs(m_Controller.radius - radius) > float.Epsilon)
            {
                m_Controller.radius = radius;
            }

            if (m_Controller.center != Vector3.zero)
            {
                m_Controller.center = Vector3.zero;   
            }
        }

        protected virtual void UpdateJump(IUnitMotion motion, bool useFusion)
        {
            if (!motion.IsJumping) return;
            if (!motion.CanJump) return;
            
            var time = useFusion ? CharacterTime : Character.Time.Time;
            
            var jumpCooldown = m_JumpTime + motion.JumpCooldown < time;
            if (!jumpCooldown) return;
            
            m_VerticalSpeed = motion.IsJumpingForce;
            m_JumpTime = time;
            Character.OnJump(motion.IsJumpingForce);
        }

        protected virtual void UpdateGravity(IUnitMotion motion, bool useFusion)
        {
            var gravity = WorldMoveDirection.y >= 0f 
                ? motion.GravityUpwards 
                : motion.GravityDownwards;
            var deltaTime = useFusion ? networkObject.Runner.DeltaTime : Character.Time.DeltaTime;
            var time = useFusion ? CharacterTime : Character.Time.Time;
            
            gravity *= GravityInfluence;
            m_VerticalSpeed += gravity * deltaTime;

            if (m_Controller.isGrounded|| (m_Controller.isGrounded && !m_IsOnSteepSlope))
            {
                if (time - m_GroundTime > COYOTE_TIME &&
                    Character.Time.Frame - m_GroundFrame > COYOTE_FRAMES)
                {
                    Character.OnLand(m_VerticalSpeed);
                }
                
                m_GroundTime = time;
                m_GroundFrame = Character.Time.Frame;

                m_VerticalSpeed = Mathf.Max(
                    m_VerticalSpeed, gravity
                );
            }

            m_VerticalSpeed = Mathf.Max(
                m_VerticalSpeed,
                gravity
            );
        }

        protected virtual void UpdateTranslation(IUnitMotion motion, bool useFusion)
        {
            var deltaTime = useFusion ? networkObject.Runner.DeltaTime : Character.Time.DeltaTime;
            var movement = Vector3.up * (m_VerticalSpeed * deltaTime);

            var kinetic = motion.MovementType switch
            {
                Character.MovementType.MoveToDirection => UpdateMoveToDirection(motion, useFusion),
                Character.MovementType.MoveToPosition => UpdateMoveToPosition(motion, useFusion),
                _ => Vector3.zero
            };

            var rootMotion = Character.Animim.RootMotionDeltaPosition;
            var translation = Vector3.Lerp(kinetic, rootMotion, Character.RootMotionPosition);
            
            if (m_IsOnSteepSlope && m_Controller.isGrounded)
            {
                var direction = Vector3.ProjectOnPlane(m_FloorNormal.Current, Vector3.up).normalized;
                movement += direction * (Mathf.Abs(motion.GravityDownwards) * Character.Time.DeltaTime);
            }
            
            movement += m_Axonometry?.ProcessTranslation(this, translation) ?? translation;

            m_IsOnSteepSlope = false; 

            if (m_FrameSlideFromCharacter >= Time.frameCount - 1)
            {
                var deltaSpeed = motion.LinearSpeed * deltaTime;
                movement += m_SlideFromCharacter * deltaSpeed;
            }
            
            movement += m_AddTranslation.Consume();

            if (m_Controller.enabled)
            {
                m_Controller.Move(movement);
            }
        }
        
        // POSITION METHODS: ----------------------------------------------------------------------

        protected virtual Vector3 UpdateMoveToDirection(IUnitMotion motion, bool useFusion)
        {
            var deltaTime = useFusion ? networkObject.Runner.DeltaTime : Character.Time.DeltaTime;

            m_MoveDirection = motion.MoveDirection;
            return m_MoveDirection * deltaTime;
        }

        protected virtual Vector3 UpdateMoveToPosition(IUnitMotion motion, bool useFusion)
        {
            var distance = Vector3.Distance(Character.Feet, motion.MovePosition);
            var brakeRadiusHeuristic = Math.Max(motion.Height, motion.Radius * 2f);
            var velocity = motion.MoveDirection.magnitude;
            
            if (distance < brakeRadiusHeuristic)
            {
                velocity = Mathf.Lerp(
                    motion.LinearSpeed, Mathf.Max(motion.LinearSpeed * 0.25f, 1f),
                    1f - Mathf.Clamp01(distance / brakeRadiusHeuristic)
                );
            }
            
            m_MoveDirection = motion.MoveDirection;
            
            var deltaTime = useFusion ? networkObject.Runner.DeltaTime : Character.Time.DeltaTime;
            return m_MoveDirection.normalized * (velocity * deltaTime);
        }

        // INTERFACE METHODS: ---------------------------------------------------------------------

        public override void SetPosition(Vector3 position, bool teleport = false)
        {
            position += Vector3.up * (Character.Motion.Height * 0.5f);
            Transform.position = position;
            if (teleport)
            {
                m_PreviousPosition = position;
            }
            Physics.SyncTransforms();
        }

        public override void SetRotation(Quaternion rotation)
        {
            Transform.rotation = rotation;
            Physics.SyncTransforms();
        }

        public override void SetScale(Vector3 scale)
        {
            Transform.localScale = scale;
            Physics.SyncTransforms();
        }

        public override void AddPosition(Vector3 amount)
        {
            m_AddTranslation.Add(amount);
        }

        public override void AddRotation(Quaternion amount)
        {
            Transform.rotation *= amount;
            Physics.SyncTransforms();
        }

        public override void AddScale(Vector3 scale)
        {
            Transform.localScale += scale;
            Physics.SyncTransforms();
        }

        // GRAVITY METHODS: -----------------------------------------------------------------------

        public override void ResetVerticalVelocity()
        {
            m_VerticalSpeed = 0f;
        }

        // CALLBACK METHODS: ----------------------------------------------------------------------

        protected virtual void OnControllerColliderHit(ControllerColliderHit hit)
        {
            var angle = Vector3.Angle(hit.normal, Vector3.up);
            
            var capsuleRadius = Mathf.Min(
                Character.Motion.Radius,
                Character.Motion.Height * 0.5f
            );
            
            if (hit.point.y < Character.Feet.y + (capsuleRadius - 0.01f))
            {
                m_FloorNormal.Target = hit.normal;
            }
            
            OnColliderHitPushRigidbodies(hit, angle);
            OnColliderHitSlideFromCharacters(hit, angle);
        }

        private void OnColliderHitSlideFromCharacters(ControllerColliderHit hit, float angle)
        {
            if (WorldMoveDirection.y > 0f || angle >= MAX_SLOPE_SLIDE_FROM_CHARACTER) return;
            
            var other = hit.collider.Get<Character>();
            if (!other) return;
            
            var slideDirection =
                Vector3.Scale(Transform.position, Vector3Plane.NormalUp) -
                Vector3.Scale(other.transform.position, Vector3Plane.NormalUp);

            slideDirection = slideDirection.sqrMagnitude > EPSILON_SLIDE_FROM_CHARACTER
                ? slideDirection.normalized
                : other.transform.forward;
            
            slideDirection.y = -1f;
                    
            m_SlideFromCharacter = slideDirection;
            m_FrameSlideFromCharacter = Time.frameCount;
        }

        private void OnColliderHitPushRigidbodies(ControllerColliderHit hit, float angle)
        {
            if (m_PushForce < float.Epsilon) return;
            
            switch (angle)
            {
                case > 90f:
                case < 5f:
                    return;
            }

            var hitRigidbody = hit.collider.attachedRigidbody;
            if (!hitRigidbody || hitRigidbody.isKinematic) return;
            
            var force = hit.controller.velocity * m_PushForce;
            force /= Character.Time.FixedDeltaTime;

            hitRigidbody.AddForceAtPosition(force, hit.point, ForceMode.Force);
        }
        
        private void OnStartRagdoll()
        {
            m_Controller.enabled = false;
            m_Controller.detectCollisions = false;
        }
        
        private void OnEndRagdoll()
        {
            m_Controller.enabled = true;
            m_Controller.detectCollisions = true;
            
            m_Controller.Move(Vector3.zero);
            m_MoveDirection = Vector3.zero;
        }
        
        // GIZMOS: --------------------------------------------------------------------------------

        public override void OnDrawGizmos(Character character)
        {
            var motion = character.Motion;
            if (motion == null) return;

            switch (motion.MovementType)
            {
                case Character.MovementType.MoveToPosition:
                    OnDrawGizmosToTarget(motion);
                    break;
                case Character.MovementType.None:
                    break;
                case Character.MovementType.MoveToDirection:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        protected void OnDrawGizmosToTarget(IUnitMotion motion)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(Character.Feet, motion.MovePosition);
        }

        public override string ToString() => "Fusion Character Controller";

        
    }
}