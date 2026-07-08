using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using Event = GameCreator.Runtime.VisualScripting.Event;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("On Spawned")]
    [Category("Fusion/Network Object/On Spawned")]
    [Description("Called on a network object when is spawned.")]

    [Image(typeof(IconCubeSolid), ColorTheme.Type.Blue, typeof(OverlayPlus))]

    [Keywords("Network Object", "Network", "Fusion", "Spawned")]

    [Serializable]
    public class EventNetworkObjectSpawned : Event
    {
        [SerializeField] private PropertyGetGameObject networkObject = GetGameObjectSelf.Create();
        private NetworkObjectSpawned _spawned;

        protected override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            
            var no = networkObject.Get(trigger.gameObject);
            if (!no) return;
            if (_spawned) return;
            _spawned = no.Require<NetworkObjectSpawned>();
            _spawned.Register(trigger, no);
        }
    }
}
