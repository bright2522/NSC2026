using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using Event = GameCreator.Runtime.VisualScripting.Event;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("On Player Spawned")]
    [Category("Fusion/Player/On Player Spawned")]
    [Description("Called when a player avatar spawned in the session.")]

    [Image(typeof(IconCharacter), ColorTheme.Type.Blue, typeof(OverlayPlus))]

    [Keywords("Player", "Spawn", "Network", "Fusion", "Connection")] 

    [Serializable]
    public class EventPlayerSpawned : Event
    {
        [SerializeField] private PropertySetGameObject savePlayer = SetGameObjectNone.Create;

        protected override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            NetworkCharacter.EventAvatarSpawned += OnAvatarSpawned;
        }

        protected override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            NetworkCharacter.EventAvatarSpawned -= OnAvatarSpawned;
        }

        private void OnAvatarSpawned(NetworkCharacter obj)
        {
            savePlayer.Set(obj.gameObject, Self);
            _ = m_Trigger.Execute(obj.gameObject);
        }
    }
}
