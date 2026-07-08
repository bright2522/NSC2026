using System;
using GameCreator.Runtime.Common;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Player Count")]
    [Category("Fusion/Session/Player Count")]

    [Image(typeof(IconNumber), ColorTheme.Type.Blue)]
    [Description("Returns the number of players in the current session")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetDecimalPlayerCount : PropertyTypeGetDecimal
    {
        public override double Get(Args args)
        {
            if(NetworkManager.IsConnected)
            {
                return NetworkManager.Runner.SessionInfo.PlayerCount;
            }

            var sessionItem = args.Target.Get<SessionItemUI>();
            return sessionItem ? sessionItem.SessionInfo.PlayerCount : 0;
        }
        public override string String => $"Player Count";
    }
}