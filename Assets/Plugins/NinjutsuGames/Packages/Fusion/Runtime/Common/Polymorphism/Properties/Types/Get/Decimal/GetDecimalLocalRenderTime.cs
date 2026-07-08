using System;
using GameCreator.Runtime.Common;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Local Render Time")]
    [Category("Fusion/Session/Local Render Time")]

    [Image(typeof(IconClock), ColorTheme.Type.Teal)]
    [Description("The current time (current State. Time + Simulation. DeltaTime) for predicted objects (objects in the local time frame). Use as an equivalent to Unity's Time. time. Time is relative to Tick 0 (which represents Time 0f)")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetDecimalLocalRenderTime : PropertyTypeGetDecimal
    {
        private const double DefaultValue = 0;

        public override double Get(Args args)
        {
            return !NetworkManager.IsConnected ? DefaultValue : NetworkManager.Runner.LocalRenderTime;
        }
        public override string String => $"Local Render Time";
    }
}