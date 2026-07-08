using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using Event = GameCreator.Runtime.VisualScripting.Event;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("On State Authority Changed")]
    [Category("Fusion/Network Object/On State Authority Changed")]
    [Description("Called on a network object when state authority is changed.<br>The NetworkObject must have the flag 'Allow State Authority Override' enabled.")]

    [Image(typeof(IconCharacterState), ColorTheme.Type.Green, typeof(OverlayTick))]

    [Keywords("Network Object", "Network", "Fusion", "Authority", "State", "Changed")]

    [Serializable]
    public class EventStateAuthorityChanged : Event
    {
        private EventAuthorityChanged _helper;
        public override Type RequiresComponent => typeof(EventAuthorityChanged);

        protected override void OnAwake(Trigger trigger)
        {
            base.OnAwake(trigger);
            _helper = trigger.gameObject.Get<EventAuthorityChanged>();
            _helper.EventOnStateAuthorityChanged += StateAuthorityChanged;
        }

        protected override void OnDestroy(Trigger trigger)
        {
            base.OnDestroy(trigger);
            if (_helper)
            {
                _helper.EventOnStateAuthorityChanged -= StateAuthorityChanged;
            }
        }

        public void StateAuthorityChanged()
        {
            _ = m_Trigger.Execute(Self);
        }
    }
}
