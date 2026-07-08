using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Last Player Joined")]
    [Category("Fusion/Last Player Joined")]
    
    [Image(typeof(IconCharacter), ColorTheme.Type.Green, typeof(OverlayArrowDown))]
    [Description("Returns the last player game object reference that joined the session")]

    [Serializable]
    public class GetGameObjectLastJoinedPlayer : PropertyTypeGetGameObject
    {
        public override GameObject Get(Args args)
        {
            return PlayerManager.LastJoinedPlayer ? PlayerManager.LastJoinedPlayer.gameObject : 
                (PlayerManager.LastJoinedPlayerData ? PlayerManager.LastJoinedPlayerData.gameObject : null);
        }

        public static PropertyGetGameObject Create()
        {
            var instance = new GetGameObjectLastJoinedPlayer();
            return new PropertyGetGameObject(instance);
        }

        public override string String => "Last Player Joined";
    }
}