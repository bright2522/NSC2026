using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Local Player")]
    [Category("Fusion/Local Player")]
    
    [Image(typeof(IconPlayer), ColorTheme.Type.Green)]
    [Description("Reference to the local Player gameObject.")]

    [Serializable]
    public class GetGameObjectLocalPlayer : PropertyTypeGetGameObject
    {
        public override GameObject Get(Args args)
        {
            return NetworkCharacter.LocalPlayer ? NetworkCharacter.LocalPlayer.gameObject : null;
        }

        public static PropertyGetGameObject Create()
        {
            GetGameObjectLocalPlayer instance = new GetGameObjectLocalPlayer();
            return new PropertyGetGameObject(instance);
        }

        public override string String => "Local Player";
    }
}