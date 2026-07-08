using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using Event = GameCreator.Runtime.VisualScripting.Event;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("On Despawned")]
    [Category("Fusion/Network Object/On Despawned")]
    [Description("Called on a network object when is despawned.")]

    [Image(typeof(IconCubeSolid), ColorTheme.Type.Blue, typeof(OverlayMinus))]

    [Keywords("Network Object", "Network", "Fusion", "Despawned")]

    [Serializable]
    public class EventNetworkObjectDespawned : Event
    {
        [SerializeField] private PropertyGetGameObject networkObject = GetGameObjectSelf.Create();
        
        private NetworkObjectDespawned _despawned;

        protected override void OnAwake(Trigger trigger)
        {
            base.OnAwake(trigger);
            
            if (_despawned) return;
            var no = networkObject.Get(trigger.gameObject);
            if (!no) return;
            _despawned = no.Require<NetworkObjectDespawned>();
            _despawned.Register(trigger, no);
        }
    }
}
