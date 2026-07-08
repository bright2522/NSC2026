using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using Event = GameCreator.Runtime.VisualScripting.Event;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("On Receive Chat Message")]
    [Category("Fusion/Room Chat/On Receive Chat Message")]
    [Description("Called when the Fusion has fully started.")]

    [Image(typeof(IconUIText), ColorTheme.Type.White, typeof(OverlayBolt))]

    [Keywords("Chat", "Message", "Network", "Fusion", "Receive")] 

    [Serializable]
    public class EventReceiveChatMessage : Event
    {
        [SerializeField] private CompareGameObjectOrAny target = new();

        protected override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            RoomChat.EventChatMessage += OnChatMessage;
        }

        protected override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            RoomChat.EventChatMessage -= OnChatMessage;
        }

        private void OnChatMessage(Args args)
        {
            if(!target.Match(args.Target, args)) return;
            
            _ = m_Trigger.Execute(args);
        }
    }
}
