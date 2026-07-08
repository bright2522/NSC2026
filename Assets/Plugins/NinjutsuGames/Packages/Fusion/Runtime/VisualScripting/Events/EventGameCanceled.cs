using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using Event = GameCreator.Runtime.VisualScripting.Event;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("On Game Canceled")]
    [Category("Fusion/Session/On Game Canceled")]
    [Description("Called when the Fusion session has been canceled")]

    [Image(typeof(IconGamepad), ColorTheme.Type.Yellow, typeof(OverlayCross))]

    [Keywords("Game", "Canceled", "Network", "Fusion", "Connection")] 

    [Serializable]
    public class EventGameCanceled : Event
    {
        protected override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            NetworkManager.EventGameCanceled += OnGameCanceled;
        }

        protected override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            NetworkManager.EventGameCanceled -= OnGameCanceled;
        }

        private void OnGameCanceled()
        {
            _ = m_Trigger.Execute(Self);
        }
    }
}
