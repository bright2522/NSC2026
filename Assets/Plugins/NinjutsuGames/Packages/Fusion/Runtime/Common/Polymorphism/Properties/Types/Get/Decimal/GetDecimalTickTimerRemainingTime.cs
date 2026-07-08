using System;
using Fusion;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Remaining Time")]
    [Category("Fusion/Tick Timer/Remaining Time")]

    [Image(typeof(IconTimer), ColorTheme.Type.Green)]
    [Description("Returns the remaining time of a TickTimer from a NetworkObject if there is any.")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetDecimalTickTimerRemainingTime : PropertyTypeGetDecimal
    {
        [SerializeField] private PropertyGetGameObject networkObject = GetGameObjectInstance.Create();

        public override double Get(Args args)
        {
            var p = networkObject.Get(args);
            var no = p.Get<NetworkObject>();
            if (no == null)
            {
                Debug.LogError($"No NetworkObject found for {p}");
                return 0;
            }
            
            var runner = NetworkRunner.GetRunnerForGameObject(p);
            var timer = NetworkDataManager.Instance.GetTimer(no);

            return timer.RemainingTime(runner) ?? 0;
        }
        public override string String => $"Remaining Time from {networkObject}";
    }
}