using System;
using System.Runtime.InteropServices;
using Fusion;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    /// <summary>A timer that is based on ticks instead of seconds.</summary>
    [NetworkStructWeaved(4)]
    [StructLayout(LayoutKind.Explicit)]
    public struct CustomTickTimer : INetworkStruct
    {
        [FieldOffset(0)] private int _target;
        [FieldOffset(4)] private int _initialTick;

        /// <summary>Gets a TickTimer that is not running.</summary>
        public static CustomTickTimer None => new();

        /// <summary>
        /// Gets a value indicating whether the TickTimer is running.
        /// </summary>
        /// <value>true if the TickTimer is running; otherwise, false.</value>
        public bool IsRunning => _target > 0;

        /// <summary>Gets the target tick of the TickTimer.</summary>
        /// <value>
        /// The target tick if the TickTimer is running; otherwise, null.
        /// </value>
        public int? TargetTick => _target > 0 ? _target : 0;

        /// <summary>Checks if the TickTimer has expired.</summary>
        /// <param name="runner">The NetworkRunner associated with the TickTimer.</param>
        /// <returns>true if the TickTimer is alive, the runner is running, and the target tick has been reached or passed; otherwise, false.</returns>
        public bool Expired(NetworkRunner runner)
        {
            return runner && runner.IsRunning && _target > 0 && _target <= runner.Tick;
        }

        /// <summary>
        /// Checks if the TickTimer has expired or is not running.
        /// </summary>
        /// <param name="runner">The NetworkRunner associated with the TickTimer.</param>
        /// <returns>true if the TickTimer is not running, the runner is not running, or the TickTimer has expired; otherwise, false.</returns>
        public bool ExpiredOrNotRunning(NetworkRunner runner)
        {
            return _target == 0 || !runner.IsRunning || Expired(runner);
        }

        /// <summary>
        /// Gets the number of remaining ticks until the TickTimer expires.
        /// </summary>
        /// <param name="runner">The NetworkRunner associated with the TickTimer.</param>
        /// <returns>The number of remaining ticks if the TickTimer is alive and running; otherwise, null.</returns>
        public int? RemainingTicks(NetworkRunner runner)
        {
            if (!runner || !runner.IsRunning) return new int?();
            return IsRunning ? Math.Max(0, _target - runner.Tick) : new int?();
        }

        /// <summary>
        /// Gets the remaining time in seconds until the TickTimer expires.
        /// </summary>
        /// <param name="runner">The NetworkRunner associated with the TickTimer.</param>
        /// <returns>The remaining time in seconds if there are remaining ticks; otherwise, null.</returns>
        public float? RemainingTime(NetworkRunner runner)
        {
            var nullable = RemainingTicks(runner);
            return nullable.HasValue ? nullable.Value * runner.DeltaTime : new float?();
        }

        /// <summary>
        /// Creates a TickTimer from a specified delay in seconds.
        /// </summary>
        /// <param name="runner">The NetworkRunner associated with the TickTimer.</param>
        /// <param name="delayInSeconds">The delay in seconds to set the TickTimer to.</param>
        /// <returns>A TickTimer that will expire after the specified delay in seconds. If the NetworkRunner is not alive or not running, returns a default TickTimer.</returns>
        public static CustomTickTimer CreateFromSeconds(NetworkRunner runner, float delayInSeconds)
        {
            if (!runner || !runner.IsRunning) return new CustomTickTimer();
            
            CustomTickTimer fromSeconds;
            fromSeconds._initialTick = runner.Tick;
            fromSeconds._target = runner.Tick + (int)Math.Ceiling(delayInSeconds / (double)runner.DeltaTime);
            return fromSeconds;
        }

        /// <summary>Creates a TickTimer from a specified number of ticks.</summary>
        /// <param name="runner">The NetworkRunner associated with the TickTimer.</param>
        /// <param name="ticks">The number of ticks to set the TickTimer to.</param>
        /// <returns>A TickTimer that will expire after the specified number of ticks. If the NetworkRunner is not alive or not running, returns a default TickTimer.</returns>
        public static CustomTickTimer CreateFromTicks(NetworkRunner runner, int ticks)
        {
            if (!runner || !runner.IsRunning) return new CustomTickTimer();
            
            CustomTickTimer fromTicks;
            fromTicks._initialTick = runner.Tick;
            fromTicks._target = runner.Tick + ticks;
            return fromTicks;
        }

        /// <summary>
        /// Returns a string that represents the current TickTimer.
        /// </summary>
        /// <returns>A string that represents the current TickTimer.</returns>
        public override string ToString() => _target.ToString();
        
        public float NormalizedValue(NetworkRunner runner)
        {
            if (runner == null || runner.IsRunning == false || IsRunning == false)
                return 0;

            if (Expired(runner))
                return 1;

            return ElapsedTicks(runner) / (_target - (float)_initialTick);
        }

        /// <summary>
        /// Gets the number of elapsed ticks since the TickTimer started.
        /// </summary>
        /// <param name="runner">The NetworkRunner associated with the TickTimer.</param>
        /// <returns>
        /// The number of elapsed ticks since the TickTimer started if the TickTimer is running and the runner is running;
        /// otherwise, returns 0.
        /// </returns>
        public int ElapsedTicks(NetworkRunner runner)
        {
            if (runner == false || runner.IsRunning == false)
                return 0;

            if (IsRunning == false || Expired(runner))
                return 0;

            return runner.Tick - _initialTick;
        }

        /// <summary>
        /// Calculates the elapsed time of a TickTimer in seconds, based on the provided NetworkRunner.
        /// </summary>
        /// <param name="runner">The NetworkRunner associated with the TickTimer.</param>
        /// <returns>The elapsed time of the TickTimer in seconds.</returns>
        public float? ElapsedTime(NetworkRunner runner)
        {
            return ElapsedTicks(runner) * runner.DeltaTime;
        }
    }
}