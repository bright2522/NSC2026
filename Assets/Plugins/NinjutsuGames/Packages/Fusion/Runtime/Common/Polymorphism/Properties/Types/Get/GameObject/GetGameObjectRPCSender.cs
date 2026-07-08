using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("RPC Sender")]
    [Category("Fusion/RPC Sender")]
    
    [Image(typeof(IconCharacter), ColorTheme.Type.Green, typeof(OverlayArrowRight))]
    [Description("Reference to the last player who sent an RPC.")]

    [Serializable]
    public class GetGameObjectRPCSender : PropertyTypeGetGameObject
    {
        public PropertyGetGameObject fallbackTo = GetGameObjectLocalPlayer.Create();

        public override GameObject Get(Args args)
        {
            return args.Target.IsPlayerAvatar() ? args.Target : fallbackTo.Get(args);
        }
        
        public override GameObject Get(GameObject gameObject)
        {
            return gameObject ? gameObject : fallbackTo.Get(gameObject);
        }

        public static PropertyGetGameObject Create()
        {
            var instance = new GetGameObjectRPCSender();
            return new PropertyGetGameObject(instance);
        }

        public override string String => "RPC Sender";
    }
}