using System;
using GameCreator.Runtime.Common;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Is Shared Mode Master Client")]
    [Category("Fusion/Session/Is Shared Mode Master Client")]

    [Image(typeof(IconCharacter), ColorTheme.Type.Purple)]
    [Description("Signal if the Local Peer is in a Room and is the Room Master Client.")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetStringIsSharedModeMasterClient : PropertyTypeGetString
    {
        public override string Get(Args args)
        {
            var value = string.Empty;
            if(NetworkManager.IsConnected)
            {
                value = NetworkManager.Runner.IsSharedModeMasterClient.ToString();
            }
            return value;
        }
        public override string String => $"Session Is Visible";
    }
}