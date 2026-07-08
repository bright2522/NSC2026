using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Player Index")]
    [Category("Fusion/Player/Player Index")]

    [Image(typeof(IconNumber), ColorTheme.Type.White)]
    [Description("Returns the player index from a list of players")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetDecimalPlayerIndex : PropertyTypeGetDecimal
    {
        [SerializeField] private PropertyGetGameObject player = GetGameObjectLocalPlayer.Create();
        [SerializeField] private CollectorListVariable list = new();

        private const double DefaultValue = 0;

        public override double Get(Args args)
        {
            if (!NetworkManager.IsConnected) return DefaultValue;
            var p = player.Get(args);
            var players = list.Get(args);
            return players.IndexOf(p);
        }
        public override string String => $"{player} Index";
    }
}