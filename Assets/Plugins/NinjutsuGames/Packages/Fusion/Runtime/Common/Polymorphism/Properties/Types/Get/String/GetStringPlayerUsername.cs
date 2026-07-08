using System;
using Fusion;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Player Username")]
    [Category("Fusion/Player/Player Username")]

    [Image(typeof(IconString), ColorTheme.Type.Purple)]
    [Description("Returns the specified player username.")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetStringFusionUsername : PropertyTypeGetString
    {
        [SerializeField] private PropertyGetGameObject player = GetGameObjectLocalPlayer.Create();

        public override string Get(Args args)
        {
            var userName = NetworkManager.ConnectionArgs.UserName;
            if(!NetworkManager.IsConnected)
            {
                if (string.IsNullOrEmpty(userName))
                {
                    var repo = FusionRepository.Get;
                    userName = string.Format(repo.Settings.defaultPlayerName, repo.SessionCodeGenerator.Create(3));
                }
                return userName;
            }
            
            var p = player.Get(args);
            if(!p)
            {
                // Debug.Log($"Couldn't find player {p}");
                return string.Empty; 
            }
            
            if(!PlayerManager.Instance)
            {
                return userName;
            }

            var networkObject = p.Get<NetworkObject>();
            if(!networkObject)
            {
                Debug.Log($"[Fusion Username] Couldn't find NetworkObject on: {p}");
                return string.Empty;
            }
            var networkPlayer = PlayerManager.Instance.GetPlayer(networkObject.InputAuthority);
            if(!networkPlayer) networkPlayer = p.Get<NetworkPlayer>();
            #if UNITY_EDITOR
            if(!networkPlayer) Debug.Log($"[Fusion Username] Couldn't find player: {p}", p);
            #endif
            return !networkPlayer ? string.Empty : networkPlayer.Username.Value;
        }
        public static PropertyGetString Create => new(new GetStringFusionUsername());

        public override string String => $"{player} Username";
    }
}