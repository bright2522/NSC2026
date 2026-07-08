using System;
using Fusion.Photon.Realtime;
using GameCreator.Runtime.Common;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Fusion App Version")]
    [Category("Fusion/Fusion App Version")] 

    [Image(typeof(IconApplication), ColorTheme.Type.Green)]
    [Description("Set fusion app version.")]

    [Serializable]
    public class SetStringFusionAppVersion : PropertyTypeSetString
    {
        public override void Set(string value, Args args)
        {
            NetworkManager.ConnectionArgs.CustomAppVersion = value;
            PhotonAppSettings.Global.AppSettings.AppVersion = value;
        }

        public override string Get(Args args)
        {
            return PhotonAppSettings.Global.AppSettings.AppVersion;
        }

        public override string String => $"Fusion App Version";
    }
}