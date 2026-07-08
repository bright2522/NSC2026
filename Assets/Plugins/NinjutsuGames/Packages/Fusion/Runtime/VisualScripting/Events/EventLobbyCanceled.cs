using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using Event = GameCreator.Runtime.VisualScripting.Event;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("On Lobby Canceled")]
    [Category("Fusion/Lobby/On Lobby Canceled")]
    [Description("Called when the Fusion Lobby session has been canceled")]

    [Image(typeof(IconHome), ColorTheme.Type.Yellow, typeof(OverlayCross))]

    [Keywords("Lobby", "Canceled", "Network", "Fusion", "Connection")] 

    [Serializable]
    public class EventLobbyCanceled : Event
    {
        protected override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            NetworkManager.EventLobbyCanceled += OnEvent;
        }

        protected override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            NetworkManager.EventLobbyCanceled -= OnEvent;
        }

        private void OnEvent()
        {
            _ = m_Trigger.Execute(Self);
        }
    }
}
