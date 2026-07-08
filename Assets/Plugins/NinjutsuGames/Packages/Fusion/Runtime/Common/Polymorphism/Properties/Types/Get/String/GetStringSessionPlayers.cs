using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Session Players Formatted")]
    [Category("Fusion/Session/Session Players Formatted")]

    [Image(typeof(IconCharacter), ColorTheme.Type.Yellow)]
    [Description("Get the number of players in the session and the maximum number of players in the session formatted as a string.")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetStringSessionPlayers : PropertyTypeGetString
    {
        [SerializeField] private string format = "{0}/{1}";
        public override string Get(Args args)
        {
            if(NetworkManager.IsConnected)
            {
                var sessionInfo = NetworkManager.Runner.SessionInfo;
                return string.Format(format, sessionInfo.PlayerCount, sessionInfo.MaxPlayers);
            }

            var sessionItem = args.Target.Get<SessionItemUI>();
            if (!sessionItem) return string.Empty;
            {
                var sessionInfo = sessionItem.SessionInfo;
                return string.Format(format, sessionInfo.PlayerCount, sessionInfo.MaxPlayers);
            }
        }
        public override string String => $"Session Players Formatted";
    }
}