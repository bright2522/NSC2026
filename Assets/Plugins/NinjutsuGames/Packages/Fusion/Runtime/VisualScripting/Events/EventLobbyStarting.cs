using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using Event = GameCreator.Runtime.VisualScripting.Event;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("On Lobby Starting")]
    [Category("Fusion/Lobby/On Lobby Starting")]
    [Description("Called when the Fusion Lobby session is starting.")]

    [Image(typeof(IconHome), ColorTheme.Type.Blue, typeof(OverlayHourglass))]

    [Keywords("Lobby", "Starting", "Network", "Fusion", "Connection")] 

    [Serializable]
    public class EventLobbyStarting : Event
    {
        protected override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            NetworkManager.EventLobbyStarting += OnEvent;
        }

        protected override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            NetworkManager.EventLobbyStarting -= OnEvent;
        }

        private void OnEvent()
        {
            _ = m_Trigger.Execute(Self);
        }
    }
}
