using System;
using Fusion;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Player Ping")]
    [Category("Fusion/Player/Player Ping")]

    [Image(typeof(IconNumber), ColorTheme.Type.Green)]
    [Description("Returns the specified player ping.")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetDecimalPlayerPing : PropertyTypeGetDecimal
    {
        [SerializeField] private PropertyGetGameObject player = GetGameObjectLocalPlayer.Create();

        private const double DefaultValue = 0;

        public override double Get(Args args)
        {
            if (!NetworkManager.IsConnected) return DefaultValue;
            var p = player.Get(args);
            if(!p || !PlayerManager.Instance) return DefaultValue; 
            var no = p.Get<NetworkObject>();
            var networkPlayer = PlayerManager.Instance.GetPlayer(no ? no.InputAuthority : NetworkManager.Runner.LocalPlayer);
            if(!networkPlayer) Debug.Log($"Couldn't find player: {player}");
            return !networkPlayer ? DefaultValue : networkPlayer.Ping;
        }
        public override string String => $"{player} Ping";
    }
}