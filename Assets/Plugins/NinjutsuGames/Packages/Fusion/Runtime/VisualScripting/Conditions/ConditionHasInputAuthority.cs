using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using NetworkObject = Fusion.NetworkObject;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Has Input Authority")]
    [Description("Returns true if local player has input authority over the specified object.")]

    [Category("Fusion/Network Object/Has Input Authority")]

    [Keywords("Fusion", "Authority", "Input Authority", "Player")]
    
    [Image(typeof(IconJoystick), ColorTheme.Type.Green, typeof(OverlayBolt))]
    
    [Serializable]
    public class ConditionHasInputAuthority : Condition
    {
        // PROPERTIES: ----------------------------------------------------------------------------
        [SerializeField] private PropertyGetGameObject target = GetGameObjectTarget.Create();
        protected override string Summary => $"Has Input Authority of {target}";
        
        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            var go = target.Get(args);
            var networkObject = go.Get<NetworkObject>();
            if (networkObject) return networkObject.HasInputAuthority;
            Debug.LogError($"Couldn't find NetworkObject on {go}");
            return false;
        }
    }
}