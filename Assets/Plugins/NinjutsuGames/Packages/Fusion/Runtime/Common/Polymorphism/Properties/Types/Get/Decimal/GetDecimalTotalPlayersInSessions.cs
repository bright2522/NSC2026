using System;
using GameCreator.Runtime.Common;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Total Players in Sessions")]
    [Category("Fusion/Lobby/Total Players in Sessions")]

    [Image(typeof(IconNumber), ColorTheme.Type.Blue)]
    [Description("Returns the total number of players in all sessions.")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetDecimalTotalPlayersInSessions : PropertyTypeGetDecimal
    {
        public override double Get(Args args)
        {
            var totalPlayers = 0;
            if (NetworkManager.SessionList == null) return totalPlayers;
            
            foreach (var session in NetworkManager.SessionList)
            {
                totalPlayers += session.PlayerCount;
            }
            return totalPlayers;
        }
        public override string String => $"Total Players in Sessions";
    }
}