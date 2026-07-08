using System;
using GameCreator.Runtime.Common;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Shutdown Reason")]
    [Category("Fusion/Reasons/Shutdown Reason")]

    [Image(typeof(IconShutdown), ColorTheme.Type.Red)]
    [Description("Reference to the last shutdown reason.")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetStringShutdownReason : PropertyTypeGetString
    {
        public override string Get(Args args)
        {
            return NetworkManager.LastShutdownReason.ToString();
        }
        public override string String => $"Shutdown Reason";
    }
}