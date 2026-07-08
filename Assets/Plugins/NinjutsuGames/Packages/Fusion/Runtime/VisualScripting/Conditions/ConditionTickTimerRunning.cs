using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using NetworkObject = Fusion.NetworkObject;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Tick Timer Running")]
    [Description("Returns true if the Tick Timer is running.")]

    [Category("Fusion/Network Object/Tick Timer Running")]

    [Keywords("Fusion", "Running", "Timer", "Time", "Session", "Network")]
    
    [Image(typeof(IconClock), ColorTheme.Type.Green)]
    
    [Serializable]
    public class ConditionTickTimerRunning : Condition
    {
        // PROPERTIES: ----------------------------------------------------------------------------
        
        [SerializeField] private PropertyGetGameObject networkObject = GetGameObjectInstance.Create();

        protected override string Summary => $"Tick Timer Running";
        
        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            var p = networkObject.Get(args);
            var no = p.Get<NetworkObject>();
            if (no == null)
            {
                Debug.LogError($"No NetworkObject found for {p}");
                return false;
            }

            var timer = NetworkDataManager.Instance.GetTimer(no);
            return timer.IsRunning;
        }
    }
}