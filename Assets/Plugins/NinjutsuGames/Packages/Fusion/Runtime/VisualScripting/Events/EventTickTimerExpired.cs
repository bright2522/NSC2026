using System;
using Fusion;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using Event = GameCreator.Runtime.VisualScripting.Event;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("On Tick Timer Expired")]
    [Category("Fusion/Network Object/On Tick Timer Expired")]
    [Description("Called on a network object when a tick timer has expired.")]

    [Image(typeof(IconTimer), ColorTheme.Type.Green)]

    [Keywords("Network Object", "Network", "Fusion", "Timer")]

    [Serializable]
    public class EventTickTimerExpired : Event
    {
        [SerializeField] private PropertyGetGameObject networkObject = GetGameObjectInstance.Create();
        
        protected override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            NetworkDataManager.EventOnTickTimerExpired += OnTickTimerExpired;
        }

        protected override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            NetworkDataManager.EventOnTickTimerExpired -= OnTickTimerExpired;
        }

        private void OnTickTimerExpired(NetworkId networkId)
        {
            var no = networkObject.Get(new Args(m_Trigger.gameObject));
            if (no.Get<NetworkObject>().Id == networkId)
            {
                _ = m_Trigger.Execute(no);
            }
        }
    }
}
