using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using Event = GameCreator.Runtime.VisualScripting.Event;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("On Game Starting")]
    [Category("Fusion/Session/On Game Starting")]
    [Description("Called when the Fusion session is starting a new game or joining an existing one")]

    [Image(typeof(IconGamepad), ColorTheme.Type.Blue, typeof(OverlayHourglass))]

    [Keywords("Game", "Starting", "Network", "Fusion", "Connection")] 

    [Serializable]
    public class EventGameStarting : Event
    {
        protected override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            NetworkManager.EventGameStarting += OnGameStarting;
        }

        protected override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            NetworkManager.EventGameStarting -= OnGameStarting;
        }

        private void OnGameStarting()
        {
            _ = m_Trigger.Execute(Self);
        }
    }
}
