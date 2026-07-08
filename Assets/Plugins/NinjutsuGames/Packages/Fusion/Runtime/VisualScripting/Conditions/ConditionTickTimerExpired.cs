using System;
using Fusion;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using NetworkObject = Fusion.NetworkObject;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Tick Timer Expired")]
    [Description("Returns true if the Tick Timer has expired.")]

    [Category("Fusion/Network Object/Tick Timer Expired")]

    [Keywords("Fusion", "Tick Timer Expired", "Expired", "Timer", "Time", "Session", "Network")]
    
    [Image(typeof(IconClock), ColorTheme.Type.Red)]
    
    [Serializable]
    public class ConditionTickTimerExpired : Condition
    {
        // PROPERTIES: ----------------------------------------------------------------------------
        
        [SerializeField] private PropertyGetGameObject networkObject = GetGameObjectInstance.Create();

        protected override string Summary => $"Tick Timer Expired";
        
        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            var p = networkObject.Get(args);
            var no = p.Get<NetworkObject>();
            if (!no)
            {
                Debug.LogError($"No NetworkObject found for {p}");
                return false;
            }

            // if (!NetworkDataManager.Instance) return false;
            
            var runner = NetworkRunner.GetRunnerForGameObject(p);
            var timer = NetworkDataManager.Instance.GetTimer(no);
            return timer.Expired(runner);
        }
    }
}