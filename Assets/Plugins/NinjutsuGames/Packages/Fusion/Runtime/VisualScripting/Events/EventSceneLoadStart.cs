using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using Event = GameCreator.Runtime.VisualScripting.Event;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("On Scene Load Start")]
    [Category("Fusion/Session/On Scene Load Start")]
    [Description("Callback when NetworkRunner starts loading the scene.")]

    [Image(typeof(IconUnity), ColorTheme.Type.Green)]

    [Keywords("Scene", "Network", "Fusion", "Load", "Start")]

    [Serializable]
    public class EventSceneLoadStart : Event
    {
        protected override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            NetworkManager.EventSceneLoadStart += OnSceneLoadStart;
        }

        protected override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            NetworkManager.EventSceneLoadStart -= OnSceneLoadStart;
        }

        private void OnSceneLoadStart()
        {
            _ = m_Trigger.Execute(Self);
        }
    }
}
