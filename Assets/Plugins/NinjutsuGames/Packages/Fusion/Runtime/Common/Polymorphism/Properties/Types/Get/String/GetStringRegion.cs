using System;
using Fusion.Photon.Realtime;
using GameCreator.Runtime.Common;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Session Region")]
    [Category("Fusion/Session/Session Region")]

    [Image(typeof(IconSphereOutline), ColorTheme.Type.Blue)]
    [Description("Returns the region that the player is connected to.")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetStringRegion : PropertyTypeGetString
    {
        public override string Get(Args args)
        {
            var region = PhotonAppSettings.Global.AppSettings.FixedRegion;
            if(string.IsNullOrEmpty(NetworkManager.ConnectionArgs.SelectedRegion))
            {
                region = NetworkManager.ConnectionArgs.SelectedRegion;
            }
            if(NetworkManager.IsConnected)
            {
                region = NetworkManager.Runner.SessionInfo.Region;
            }
            return region;
        }
        public static PropertyGetString Create => new(new GetStringRegion());
        
        public override string String => $"Session Region";
    }
}