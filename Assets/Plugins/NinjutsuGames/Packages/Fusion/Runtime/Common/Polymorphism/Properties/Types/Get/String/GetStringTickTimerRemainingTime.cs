using System;
using Fusion;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Tick Timer Remaining Time")]
    [Category("Fusion/Tick Timer/Tick Timer Remaining Time")]

    [Image(typeof(IconTimer), ColorTheme.Type.Green)]
    [Description("Returns the remaining time of a TickTimer from a NetworkObject if there is any.")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetStringTickTimerRemainingTime : PropertyTypeGetString
    {
        [SerializeField] private PropertyGetGameObject networkObject = GetGameObjectInstance.Create();

        public override string Get(Args args)
        {
            var p = networkObject.Get(args);
            var no = p.Get<NetworkObject>();
            if (!no)
            {
                no = NetworkDataManager.Instance.Object;
                p = no.gameObject;
            }

            if (!NetworkDataManager.Instance) return string.Empty;
            
            var runner = NetworkRunner.GetRunnerForGameObject(p);
            var timer = NetworkDataManager.Instance.GetTimer(no);
            var totalSeconds = (int)(timer.RemainingTime(runner) ?? 0);
            var hours = totalSeconds / 3600;
            var minutes = (totalSeconds % 3600) / 60;
            var seconds = totalSeconds % 60;

            return hours > 0 ? $"{hours}:{minutes:D2}:{seconds:D2}" : $"{minutes}:{seconds:D2}";
        }
        public override string String => $"Remaining Time from {networkObject}";
    }
}