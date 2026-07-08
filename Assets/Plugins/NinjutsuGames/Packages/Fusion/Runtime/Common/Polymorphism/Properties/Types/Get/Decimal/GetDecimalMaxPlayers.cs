using System;
using GameCreator.Runtime.Common;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Max Players")]
    [Category("Fusion/Session/Max Players")]

    [Image(typeof(IconNumber), ColorTheme.Type.Blue)]
    [Description("Returns the maximum number of players in the current session")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetDecimalMaxPlayers : PropertyTypeGetDecimal
    {
        public override double Get(Args args)
        {
            if(NetworkManager.IsConnected)
            {
                return NetworkManager.Runner.SessionInfo.MaxPlayers;
            }

            var sessionItem = args.Target.Get<SessionItemUI>();
            return sessionItem ? sessionItem.SessionInfo.MaxPlayers : 0;
        }
        public override string String => $"Max Players";
    }
}