using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Last Player Left")]
    [Category("Fusion/Last Player Left")]
    
    [Image(typeof(IconCharacter), ColorTheme.Type.Green, typeof(OverlayArrowLeft))]
    [Description("Returns the last player game object reference that left the session")]

    [Serializable]
    public class GetGameObjectLastLeftPlayer : PropertyTypeGetGameObject
    {
        public override GameObject Get(Args args)
        {
            return PlayerManager.LastLeftPlayer ? PlayerManager.LastLeftPlayer.gameObject : 
                (PlayerManager.LastLeftPlayerData ? PlayerManager.LastLeftPlayerData.gameObject : null);
        }

        public static PropertyGetGameObject Create()
        {
            return new PropertyGetGameObject(new GetGameObjectLastLeftPlayer());
        }

        public override string String => "Last Player Left";
    }
}