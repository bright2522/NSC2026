using System;
using Fusion.Photon.Realtime;
using GameCreator.Runtime.Common;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Fusion App Version")]
    [Category("Fusion/Fusion App Version")]

    [Image(typeof(IconApplication), ColorTheme.Type.Green)]
    [Description("Returns the Fusion App Version")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetStringAppVersion : PropertyTypeGetString
    {
        public override string Get(Args args)
        {
            var appVersion = PhotonAppSettings.Global.AppSettings.AppVersion;
            if(!string.IsNullOrEmpty(NetworkManager.ConnectionArgs.CustomAppVersion))
            {
                appVersion = NetworkManager.ConnectionArgs.CustomAppVersion;
            }
            return appVersion;
        }
        public static PropertyGetString Create => new(new GetStringAppVersion());
        public override string String => $"Fusion App Version";
    }
}