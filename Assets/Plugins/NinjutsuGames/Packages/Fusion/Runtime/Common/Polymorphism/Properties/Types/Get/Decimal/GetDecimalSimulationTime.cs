using System;
using GameCreator.Runtime.Common;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Simulation Time")]
    [Category("Fusion/Session/Simulation Time")]

    [Image(typeof(IconClock), ColorTheme.Type.Yellow)]
    [Description("The time the current State represents (the most recent FixedUpdateNetwork simulation). Use as an equivalent to Unity's Time. fixedTime. Time is relative to Tick 0 (which represents Time 0f).")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetDecimalSimulationTime : PropertyTypeGetDecimal
    {
        private const double DefaultValue = 0;

        public override double Get(Args args)
        {
            return !NetworkManager.IsConnected ? DefaultValue : NetworkManager.Runner.SimulationTime;
        }
        public override string String => $"Simulation Time";
    }
}