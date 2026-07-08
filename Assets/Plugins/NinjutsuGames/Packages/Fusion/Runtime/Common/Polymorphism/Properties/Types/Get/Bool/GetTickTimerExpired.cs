using System;
using Fusion;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Tick Timer Expired")]
    [Category("Fusion/Tick Timer Expired")]

    [Image(typeof(IconClock), ColorTheme.Type.Red)]
    [Description("Returns true if the tick timer has expired.")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetTickTimerExpired : PropertyTypeGetBool
    {
        [SerializeField] private PropertyGetGameObject networkObject = GetGameObjectInstance.Create();
        public override bool Get(Args args)
        {
            var p = networkObject.Get(args);
            var no = p.Get<NetworkObject>();
            if (no == null)
            {
                Debug.LogError($"No NetworkObject found for {p}");
                return false;
            }

            if (!NetworkDataManager.Instance) return false;
            
            var runner = NetworkRunner.GetRunnerForGameObject(p);
            var timer = NetworkDataManager.Instance.GetTimer(no);
            return timer.Expired(runner);
        }
        public override string String => $"Tick Timer Expired";
    }
}