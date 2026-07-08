using System;
using GameCreator.Runtime.Common;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Players Alive")]
    [Category("Fusion/Session/Players Alive")]

    [Image(typeof(IconSkull), ColorTheme.Type.Green, typeof(OverlayTick))]
    [Description("Returns the number of players alive in the current session")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetDecimalPlayersAlive : PropertyTypeGetDecimal
    {
        public override double Get(Args args)
        {
            var alive = 0;

            foreach (var networkCharacter in PlayerManager.Avatars)
            {
                if(networkCharacter.Value.Character.IsDead) continue;
                alive++;
            }
            
            return alive;
        }
        public override string String => $"Player Alive";
    }
}