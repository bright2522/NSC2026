using System;
using GameCreator.Runtime.Common;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Total Sessions")]
    [Category("Fusion/Lobby/Total Sessions")]

    [Image(typeof(IconNumber), ColorTheme.Type.Green)]
    [Description("Returns the total number of sessions.")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetDecimalTotalSession : PropertyTypeGetDecimal
    {
        public override double Get(Args args)
        {
            return NetworkManager.SessionList != null ? NetworkManager.SessionList.Count : 0;
        }
        public override string String => $"Total Sessions";
    }
}