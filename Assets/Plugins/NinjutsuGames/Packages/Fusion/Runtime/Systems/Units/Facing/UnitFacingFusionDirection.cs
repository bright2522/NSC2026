using System;
using Fusion;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Towards Fusion Direction")]
    [Image(typeof(IconArrowCircleRight), ColorTheme.Type.Green)]
    [Category("Towards Fusion Direction")]
    [Description("NOTE: DO NOT USE this manually, this facing direction is set automatically at runtime when using Fusion. Rotates the Character towards a specific world-space network direction.")]
    [Serializable]
    public class UnitFacingFusionDirection : TUnitFacing
    {
        public bool useFusionDeltaTime = false;
        public override Axonometry Axonometry { get; set; }
        protected override Vector3 GetDefaultDirection() => m_FaceDirection;

        // STRING: --------------------------------------------------------------------------------

        public override string ToString() => "Towards Fusion Direction";
        
        private Quaternion _mRotationVelocity = Quaternion.identity;
        private NetworkObject _networkObject;

        public void SetDirection(Vector3 direction)
        {
            m_FaceDirection = direction;
        }

        public override void OnStartup(Character character)
        {
            base.OnStartup(character);
            _networkObject = character.Get<NetworkObject>();
        }

        public override void OnUpdate()
        {
            if(m_FaceDirection == Vector3.zero) return;
            base.OnUpdate();
            
            if(!useFusionDeltaTime) return;
            
            var targetRotation = Quaternion.LookRotation(m_FaceDirection);

            var srcRotation = Transform.rotation;
            var dstRotation = Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f);

            var rotation = QuaternionUtils.SmoothDamp(
                srcRotation, dstRotation,
                ref _mRotationVelocity,
                1f / (Character.Motion.AngularSpeed / 360f), 
                _networkObject.Runner.DeltaTime
            );

            m_PivotSpeed = Vector3.SignedAngle(
                srcRotation * Vector3.forward,
                dstRotation * Vector3.forward,
                Vector3.up
            );

            Transform.rotation = Quaternion.Lerp(
                rotation,
                srcRotation * Character.Animim.RootMotionDeltaRotation,
                Character.RootMotionRotation
            );
        }
    }
}