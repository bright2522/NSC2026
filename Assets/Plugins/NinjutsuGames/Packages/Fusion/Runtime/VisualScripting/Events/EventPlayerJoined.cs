using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using Event = GameCreator.Runtime.VisualScripting.Event;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("On Player Joined")]
    [Category("Fusion/Player/On Player Joined")]
    [Description("Called when a remote player entered the session.")]

    [Image(typeof(IconCharacter), ColorTheme.Type.Green, typeof(OverlayDot))]

    [Keywords("Player", "Network", "Fusion", "Joined")]

    [Serializable]
    public class EventPlayerJoined : Event
    {
        protected override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            NetworkPlayer.EventPlayerSpawned += OnPlayerJoined;
        }
        
        protected override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            NetworkPlayer.EventPlayerSpawned -= OnPlayerJoined;
        }

        private void OnPlayerJoined(NetworkPlayer player)
        {
            _ = m_Trigger.Execute(player.gameObject);
        }
    }
}
