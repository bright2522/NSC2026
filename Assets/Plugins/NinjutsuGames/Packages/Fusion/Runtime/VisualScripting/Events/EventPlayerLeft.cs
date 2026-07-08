using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using Event = GameCreator.Runtime.VisualScripting.Event;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("On Player Left")]
    [Category("Fusion/Player/On Player Left")]
    [Description("Called when a remote player left the session.")]

    [Image(typeof(IconCharacter), ColorTheme.Type.Green, typeof(OverlayArrowLeft))]

    [Keywords("Player", "Network", "Fusion", "Left")]

    [Serializable]
    public class EventPlayerLeft : Event
    {
        protected override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger); 
            NetworkPlayer.EventPlayerDespawned += OnPlayerDespawned;
        }
        
        protected override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            NetworkPlayer.EventPlayerDespawned -= OnPlayerDespawned;
        }

        private void OnPlayerDespawned(NetworkPlayer player)
        {
            _ = m_Trigger.Execute(player.gameObject);
        }
    }
}
