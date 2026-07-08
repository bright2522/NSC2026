using System;
using GameCreator.Runtime.Common;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Is Connected")]
    [Category("Fusion/Is Connected")]

    [Image(typeof(IconDot), ColorTheme.Type.Green)]
    [Description("Returns whether the player is connected to the network.")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetBoolIsConnected : PropertyTypeGetBool
    {
        public override bool Get(Args args)
        {
            return NetworkManager.IsConnected;
        }
        public override string String => $"Is Connected";
    }
}