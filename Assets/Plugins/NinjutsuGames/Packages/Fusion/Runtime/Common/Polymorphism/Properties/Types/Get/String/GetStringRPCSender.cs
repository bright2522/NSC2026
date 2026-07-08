using System;
using GameCreator.Runtime.Common;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("RPC Sender")]
    [Category("Fusion/RPC Sender")]

    [Image(typeof(IconCharacter), ColorTheme.Type.Green, typeof(OverlayArrowRight))]
    [Description("Reference to the last player who sent an RPC.")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetStringRPCSender : PropertyTypeGetString
    {
        public PropertyGetGameObject fallbackTo = GetGameObjectLocalPlayer.Create();
        public override string Get(Args args)
        {
            return args.Target.IsPlayerAvatar() ? args.Target.name : fallbackTo.Get(args).name;
        }
        
        public override string String => $"RPC Sender";
    }
}