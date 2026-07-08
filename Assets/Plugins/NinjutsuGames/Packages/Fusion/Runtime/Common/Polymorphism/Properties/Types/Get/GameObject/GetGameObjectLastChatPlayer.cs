using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Last Chat Player")]
    [Category("Fusion/Last Chat Player")]
    
    [Image(typeof(IconCharacter), ColorTheme.Type.Green, typeof(OverlayTick))]
    [Description("Reference to the last player who sent a chat message.")]

    [Serializable, HideLabelsInEditor]
    public class GetGameObjectLastChatPlayer : PropertyTypeGetGameObject
    {
        public override GameObject Get(Args args)
        {
            if (!RoomChat.Instance || !RoomChat.Instance.LastPlayer) return args.Target ? args.Target : args.Self;
            var last = RoomChat.Instance.LastPlayer;
            if(!last) return null;
            var go = last.gameObject;
            if (go) return go;
            return args.Target ? args.Target : args.Self;
        }

        public static PropertyGetGameObject Create()
        {
            var instance = new GetGameObjectLastChatPlayer();
            return new PropertyGetGameObject(instance);
        }

        public override string String => "Last Chat Player";
    }
}