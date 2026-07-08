using System;
using Fusion;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Chat Color from Target")]
    [Category("Fusion/Chat Color from Target")]

    [Image(typeof(IconUIText), ColorTheme.Type.White)]
    [Description("Returns the color for the current message depending if the target is the local player or not.")]

    [Serializable]
    public class GetColorLastMessageTarget : PropertyTypeGetColor
    {
        [SerializeField] protected PropertyGetGameObject m_Target = GetGameObjectLocalPlayer.Create();

        public override Color Get(Args args)
        {
            var p = m_Target.Get(args);
            var no = p.Get<NetworkObject>();
            return !no ? Color.white : RoomChat.GetColorFromTarget(args, no.InputAuthority);
        }

        public override string String => $"Chat Color from {m_Target}";
    }
}