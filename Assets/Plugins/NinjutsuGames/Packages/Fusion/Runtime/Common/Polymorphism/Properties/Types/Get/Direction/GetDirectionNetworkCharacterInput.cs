using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Network Input Direction")]
    [Category("Characters/Network Input Direction")]
    
    [Image(typeof(IconGamepadCross), ColorTheme.Type.Green, typeof(OverlayArrowRight))]
    [Description("The desired input direction of a Network Character in world space")]

    [Serializable]
    public class GetDirectionNetworkCharacterInput : PropertyTypeGetDirection
    {
        [SerializeField]
        protected PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();
        
        public override Vector3 Get(Args args) => GetDirection(args);

        private Vector3 GetDirection(Args args)
        {
            var character = m_Character.Get<NetworkCharacter>(args);
            return character ? character.InputDirection : default;
        }
        
        public static PropertyGetDirection Create => new(new GetDirectionNetworkCharacterInput());

        public override string String => $"{m_Character} Network Input";
    }
}