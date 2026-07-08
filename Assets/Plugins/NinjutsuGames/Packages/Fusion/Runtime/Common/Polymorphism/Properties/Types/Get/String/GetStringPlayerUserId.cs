using System;
using Fusion;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Player User Id")]
    [Category("Fusion/Player/Player User Id")]

    [Image(typeof(IconString), ColorTheme.Type.Pink)]
    [Description("Returns the specified player user id.")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetStringFusionUserId : PropertyTypeGetString
    {
        [SerializeField] private PropertyGetGameObject player = GetGameObjectLocalPlayer.Create();

        public override string Get(Args args)
        {
            var p = player.Get(args);
            if(!p)
            {
                Debug.Log($"Couldn't find player {p}");
                return string.Empty; 
            }
            var networkObject = p.Get<NetworkObject>();
            if (networkObject)
            {
                return NetworkManager.Runner.GetPlayerUserId(networkObject.InputAuthority);
            }
            Debug.Log($"[Fusion User Id] Couldn't find NetworkObject on: {p}");
            return string.Empty;
        }
        public static PropertyGetString Create => new(new GetStringFusionUserId());

        public override string String => $"{player} User Id";
    }
}