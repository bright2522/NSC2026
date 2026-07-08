using System;
using GameCreator.Runtime.Common;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Session Valid")]
    [Category("Fusion/Session/Session Valid")]

    [Image(typeof(IconChip), ColorTheme.Type.Green, typeof(OverlayTick))]
    [Description("Returns whether the current session is valid")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetBoolSessionIsValid : PropertyTypeGetBool
    {
        public override bool Get(Args args)
        {
            return NetworkManager.IsConnected && NetworkManager.Runner.SessionInfo.IsValid;
        }
        public override string String => $"Session Valid";
    }
}