using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using Event = GameCreator.Runtime.VisualScripting.Event;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("On Region Selected Change")]
    [Category("Fusion/On Region Selected Change")]
    [Description("Triggers when the selected region changes using the UI dropdown menu.")]

    [Image(typeof(IconSphereOutline), ColorTheme.Type.Green, typeof(OverlayTick))]

    [Keywords("Region", "Changed", "Network", "Fusion", "Selected")] 

    [Serializable]
    public class EventSelectedRegionChanged : Event
    {
        protected override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            NetworkManager.EventSelectedRegionChanged += OnRegionSelectedChange;
        }

        protected override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            NetworkManager.EventSelectedRegionChanged -= OnRegionSelectedChange;
        }

        private void OnRegionSelectedChange()
        {
            _ = m_Trigger.Execute(Self);
        }
    }
}
