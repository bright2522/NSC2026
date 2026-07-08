using System;
using Fusion.Photon.Realtime;
using GameCreator.Runtime.Common;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Lobby Region")]
    [Category("Fusion/Lobby/Lobby Region")]

    [Image(typeof(IconSphereOutline), ColorTheme.Type.Green)]
    [Description("Returns the lobby region that the player is connected to.")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetStringLobbyRegion : PropertyTypeGetString
    {
        public override string Get(Args args)
        {
            var region = PhotonAppSettings.Global.AppSettings.FixedRegion;
            if(string.IsNullOrEmpty(NetworkManager.ConnectionArgs.SelectedRegion))
            {
                region = NetworkManager.ConnectionArgs.SelectedRegion;
            }
            if(NetworkManager.IsConnectedInLobby)
            {
                region = NetworkManager.RunnerLobby.LobbyInfo.Region;
            }
            return region;
        }
        public override string String => $"Lobby Region";
    }
}