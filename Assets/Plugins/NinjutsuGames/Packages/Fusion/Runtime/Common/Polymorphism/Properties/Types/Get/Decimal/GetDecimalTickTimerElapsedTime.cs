using System;
using Fusion;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Elapsed Time")]
    [Category("Fusion/Tick Timer/Elapsed Time")]

    [Image(typeof(IconTimer), ColorTheme.Type.Green, typeof(OverlayTick))]
    [Description("Returns the elapsed time of a TickTimer from a NetworkObject if there is any.")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetDecimalTickTimerElapsedTime : PropertyTypeGetDecimal
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

            return timer.ElapsedTime(runner) ?? 0;
        }
        public override string String => $"Elapsed Time from {networkObject}";
    }
}