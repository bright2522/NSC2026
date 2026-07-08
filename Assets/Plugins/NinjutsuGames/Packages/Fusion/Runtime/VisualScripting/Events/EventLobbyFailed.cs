using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using Event = GameCreator.Runtime.VisualScripting.Event;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("On Lobby Failed")]
    [Category("Fusion/Lobby/On Lobby Failed")]
    [Description("Called when the Fusion Lobby session has failed")]

    [Image(typeof(IconHome), ColorTheme.Type.Red)]

    [Keywords("Lobby", "Failed", "Network", "Fusion", "Connection")] 

    [Serializable]
    public class EventLobbyFailed : Event
    {
        protected override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            NetworkManager.EventLobbyFailed += OnEvent;
        }

        protected override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            NetworkManager.EventLobbyFailed -= OnEvent;
        }

        private void OnEvent()
        {
            _ = m_Trigger.Execute(Self);
        }
    }
}
