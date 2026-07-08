using System;
using GameCreator.Runtime.Common;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Is In Session")]
    [Category("Fusion/Session/Is In Session")]

    [Image(typeof(IconChip), ColorTheme.Type.Green, typeof(OverlayFlame))]
    [Description("Returns true if the current session is in progress")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetBoolSessionIsInSession : PropertyTypeGetBool
    {
        public override bool Get(Args args)
        {
            return NetworkManager.IsConnected && NetworkManager.Runner.IsInSession;
        }
        public override string String => $"Session Valid";
    }
}