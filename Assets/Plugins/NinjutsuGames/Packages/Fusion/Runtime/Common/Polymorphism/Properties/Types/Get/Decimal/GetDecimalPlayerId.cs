using System;
using Fusion;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Player Id")]
    [Category("Fusion/Player/Player Id")]

    [Image(typeof(IconID), ColorTheme.Type.Purple)]
    [Description("Returns the PlayerRef as an integer Id value. -1=None -2=MasterClient >=0=PlayerId")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetDecimalPlayerId : PropertyTypeGetDecimal
    {
        [SerializeField] private PropertyGetGameObject player = GetGameObjectLocalPlayer.Create();

        private const double DefaultValue = 0;

        public override double Get(Args args)
        {
            if (!NetworkManager.IsConnected) return DefaultValue;
            var p = player.Get(args);
            if(!p || !PlayerManager.Instance) return DefaultValue; 
            var no = p.Get<NetworkObject>();
            var playerRef = no ? no.InputAuthority : NetworkManager.Runner.LocalPlayer;
            return playerRef.PlayerId;
        }
        public override string String => $"{player} Id";
    }
}