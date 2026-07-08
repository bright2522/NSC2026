using System;
using Fusion;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Tick Timer Running")]
    [Category("Fusion/Tick Timer Running")]

    [Image(typeof(IconClock), ColorTheme.Type.Green)]
    [Description("Returns true if the tick timer is running.")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetTickTimerRunning : PropertyTypeGetBool
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
            
            var timer = NetworkDataManager.Instance.GetTimer(no);
            return timer.IsRunning;
        }
        public override string String => $"Tick Timer Running";
    }
}