using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using Event = GameCreator.Runtime.VisualScripting.Event;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("On Game Failed")]
    [Category("Fusion/Session/On Game Failed")]
    [Description("Called when the Fusion session has failed")]

    [Image(typeof(IconGamepad), ColorTheme.Type.Red)]

    [Keywords("Game", "Failed", "Network", "Fusion", "Connection")] 

    [Serializable]
    public class EventGameFailed : Event
    {
        protected override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            NetworkManager.EventGameFailed += OnEvent;
        }

        protected override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            NetworkManager.EventGameFailed -= OnEvent;
        }

        private void OnEvent()
        {
            _ = m_Trigger.Execute(Self);
        }
    }
}
