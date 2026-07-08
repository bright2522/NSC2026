using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using Event = GameCreator.Runtime.VisualScripting.Event;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("On Scene Load Done")]
    [Category("Fusion/Session/On Scene Load Done")]
    [Description("Callback when NetworkRunner finishes loading the scene.")]

    [Image(typeof(IconUnity), ColorTheme.Type.Blue)]

    [Keywords("Scene", "Network", "Fusion", "Load", "Done")]

    [Serializable]
    public class EventSceneLoadDone : Event
    {
        protected override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            NetworkManager.EventSceneLoadDone += OnSceneLoadDone;
        }

        protected override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            NetworkManager.EventSceneLoadDone -= OnSceneLoadDone;
        }

        private void OnSceneLoadDone()
        {
            if(!Self) return;
            if(!m_Trigger) return;
            _ = m_Trigger.Execute(Self);
        }
    }
}
