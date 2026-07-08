using System;
using GameCreator.Runtime.Common;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Network Status")]
    [Category("Fusion/Reasons/Network Status")]

    [Image(typeof(IconCircleOutline), ColorTheme.Type.Green)]
    [Description("Current network status.")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetStringNetworkStatus : PropertyTypeGetString
    {
        public override string Get(Args args)
        {
            return NetworkManager.NetworkStatus.ToString();
        }
        public override string String => $"Network Status";
    }
}