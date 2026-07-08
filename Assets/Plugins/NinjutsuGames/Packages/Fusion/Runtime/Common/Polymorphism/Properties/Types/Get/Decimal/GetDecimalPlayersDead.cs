using System;
using GameCreator.Runtime.Common;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Players Dead")]
    [Category("Fusion/Session/Players Dead")]

    [Image(typeof(IconSkull), ColorTheme.Type.Red, typeof(OverlayTick))]
    [Description("Returns the number of players dead in the current session")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetDecimalPlayersDead : PropertyTypeGetDecimal
    {
        public override double Get(Args args)
        {
            var dead = 0;

            foreach (var networkCharacter in PlayerManager.Avatars)
            {
                if(!networkCharacter.Value.Character.IsDead) continue;
                dead++;
            }
            
            return dead;
        }
        public override string String => $"Player Dead";
    }
}