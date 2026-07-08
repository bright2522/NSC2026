using System;
using GameCreator.Runtime.Common;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Disconnect Reason")]
    [Category("Fusion/Reasons/Disconnect Reason")]

    [Image(typeof(IconDisconnected), ColorTheme.Type.Red)]
    [Description("Reference to the last server disconnect reason.")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetStringDisconnectReason : PropertyTypeGetString
    {
        public override string Get(Args args)
        {
            return NetworkManager.LastDisconnectReason.ToString();
        }
        public override string String => $"Disconnect Reason";
    }
}