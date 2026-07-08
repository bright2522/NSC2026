using System;
using GameCreator.Runtime.Common;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Latest Server Tick")]
    [Category("Fusion/Session/Latest Server Tick")]

    [Image(typeof(IconClock), ColorTheme.Type.Green)]
    [Description("Get the latest confirmed tick of the server we are aware of. This represents a frame number.")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetDecimalLatestServerTick : PropertyTypeGetDecimal
    {
        private const double DefaultValue = 0;

        public override double Get(Args args)
        {
            if (!NetworkManager.IsConnected) return DefaultValue;
            return NetworkManager.Runner.LatestServerTick.Raw;
        }
        public override string String => $"Latest Server Tick";
    }
}