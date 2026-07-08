using System;
using GameCreator.Runtime.Common;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Is Shared Mode Master Client")]
    [Category("Fusion/Is Shared Mode Master Client")]

    [Image(typeof(IconCharacter), ColorTheme.Type.Purple)]
    [Description("Signal if the Local Peer is in a Room and is the Room Master Client.")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetBoolIsSharedModeMasterClient : PropertyTypeGetBool
    {
        public override bool Get(Args args)
        {
            return NetworkManager.IsConnected && NetworkManager.Runner.IsSharedModeMasterClient;
        }
        public override string String => "Is Shared Mode Master Client";
    }
}