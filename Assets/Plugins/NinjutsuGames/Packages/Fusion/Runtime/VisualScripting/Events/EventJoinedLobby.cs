using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using Event = GameCreator.Runtime.VisualScripting.Event;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("On Lobby Started")]
    [Category("Fusion/Lobby/On Lobby Started")]
    [Description("Called when the player has joined a lobby")]

    [Image(typeof(IconHome), ColorTheme.Type.Green)]

    [Keywords("Lobby", "Joined", "Network", "Fusion", "Connection")] 

    [Serializable]
    public class EventJoinedLobby : Event
    {
        protected override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            NetworkManager.EventLobbyStarted += OnLobbyStarted;
        }

        protected override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            NetworkManager.EventLobbyStarted -= OnLobbyStarted;
        }

        private void OnLobbyStarted()
        {
            _ = m_Trigger.Execute(Self);
        }
    }
}
