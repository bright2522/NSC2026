using System;
using Fusion;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Last Chat Message")]
    [Category("Fusion/Last Chat Message")]

    [Image(typeof(IconUIText), ColorTheme.Type.Green)]
    [Description("Returns the last chat message from the specified player.")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetStringLastChatMessage : PropertyTypeGetString
    {
        [SerializeField] private PropertyGetGameObject fromPlayer = GetGameObjectLastChatPlayer.Create();

        public override string Get(Args args)
        {
            var p = fromPlayer.Get(args);
            if (!p) return string.Empty;
            var pl = p.Get<NetworkObject>();
            if (!pl)
            {
                Debug.Log($"Couldn't find NetworkObject on player {p}", p);
                return string.Empty;
            }
            RoomChat.Instance.LastMessages.TryGetValue(pl.InputAuthority, out var message);
            return message ?? string.Empty;
        }
        public override string String => $"Last Chat Message from {fromPlayer}";
    }
}