/*using System;
using Fusion;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Fusion Directional [Deprecated]")]
    [Image(typeof(IconGamepadCross), ColorTheme.Type.Blue, typeof(OverlayBolt))]
    
    [Category("Fusion Directional")]
    [Description(
        "Moves the Player using a directional input from the Main Camera's perspective"
    )]

    [Serializable]
    public class UnitFusionPlayerDirectional : TUnitPlayer
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField] private InputPropertyValueVector2 m_InputMove;
        private Vector3 m_NetworkInput;

        // INITIALIZERS: --------------------------------------------------------------------------

        public UnitFusionPlayerDirectional()
        {
            m_InputMove = InputValueVector2MotionPrimary.Create();
        }
        
        // OVERRIDERS: ----------------------------------------------------------------------------

        public override void OnStartup(Character character)
        {
            base.OnStartup(character);
            m_InputMove.OnStartup();
        }
        
        public override void OnDispose(Character character)
        {
            base.OnDispose(character);
            m_InputMove.OnDispose();
        }

        public override void OnDisable()
        {
            base.OnDisable();
            Character.Motion?.MoveToDirection(Vector3.zero, Space.World, 0);
        }

        // UPDATE METHODS: ------------------------------------------------------------------------

        public override void OnUpdate()
        {
            base.OnUpdate();
            
            if(Character.IsPlayer)
            {
                m_InputMove.OnUpdate();
                InputDirection = Vector3.zero;

                Vector3 inputMovement = m_IsControllable
                    ? m_InputMove.Read()
                    : Vector2.zero;

                InputDirection = GetMoveDirection(inputMovement);
            }
            else
            {
                InputDirection = m_NetworkInput;
            }
            
            var speed = Character.Motion?.LinearSpeed ?? 0f;
            Character.Motion?.MoveToDirection(InputDirection * speed, Space.World, 0);
        }

        protected virtual Vector3 GetMoveDirection(Vector3 input)
        {
            var direction = new Vector3(input.x, 0f, input.y);

            var moveDirection = Camera
                ? Camera.TransformDirection(direction)
                : Vector3.zero;

            moveDirection.Scale(Vector3Plane.NormalUp);
            moveDirection.Normalize();

            return moveDirection * direction.magnitude;
        }
        
        public void SetInput(Vector3 input)
        {
            m_NetworkInput = input;
        }
        
        // STRING: --------------------------------------------------------------------------------

        public override string ToString() => "Fusion Directional";
    }
}*/