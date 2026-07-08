using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using Event = GameCreator.Runtime.VisualScripting.Event;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("On Username Changed")]
    [Category("Fusion/Player/On Username Changed")]
    [Description("Triggers when a player's username has changed")]

    [Image(typeof(IconString), ColorTheme.Type.Purple)]

    [Keywords("Player", "Network", "Fusion", "Username", "Changed")]

    [Serializable]
    public class EventPlayerUsernameChanged : Event
    {
        protected override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            NetworkPlayer.EventUsernameChanged += OnPlayerUsernameChanged;
        }
        
        protected override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            NetworkPlayer.EventUsernameChanged -= OnPlayerUsernameChanged;
        }

        private void OnPlayerUsernameChanged(NetworkPlayer player)
        {
            var no = player.Object;
            if (no == null) return;
            var inputAuthority = no.InputAuthority;
            if (inputAuthority.IsNone) return;
            var avatar = PlayerManager.Instance.GetAvatar(inputAuthority);
            if (avatar == null) return;
            _ = m_Trigger.Execute(avatar.gameObject);
        }
    }
}
