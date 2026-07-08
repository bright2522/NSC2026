using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using Event = GameCreator.Runtime.VisualScripting.Event;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("On Player Despawned")]
    [Category("Fusion/Player/On Player Despawned")]
    [Description("Called when a player avatar is despawned in the session.")]

    [Image(typeof(IconCharacter), ColorTheme.Type.Red, typeof(OverlayMinus))]

    [Keywords("Player", "Despawn", "Network", "Fusion")] 

    [Serializable]
    public class EventPlayerDespawned : Event
    {
        [SerializeField] private CompareGameObjectOrAny m_Target = new();

        protected override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            NetworkCharacter.EventAvatarDespawned += OnAvatarDespawned;
        }

        protected override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            NetworkCharacter.EventAvatarDespawned -= OnAvatarDespawned;
        }

        private void OnAvatarDespawned(NetworkCharacter obj)
        {
            if (!m_Target.Match(obj.gameObject, m_Trigger.gameObject)) return;
            _ = m_Trigger.Execute(obj.gameObject);
        }
    }
}
