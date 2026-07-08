using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using Event = GameCreator.Runtime.VisualScripting.Event;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("On Game Started")]
    [Category("Fusion/Session/On Game Started")]
    [Description("Called when the Fusion has fully started.")]

    [Image(typeof(IconGamepad), ColorTheme.Type.Green, typeof(OverlayBolt))]

    [Keywords("Game", "Started", "Network", "Fusion", "Connection")] 

    [Serializable]
    public class EventGameStarted : Event
    {
        protected override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            NetworkManager.EventGameStarted += OnGameStarted;
        }

        protected override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            NetworkManager.EventGameStarted -= OnGameStarted;
        }

        private void OnGameStarted()
        {
            _ = m_Trigger.Execute(Self);
        }
    }
}
