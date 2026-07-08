using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using NetworkObject = Fusion.NetworkObject;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Has State Authority")]
    [Description("Returns true if local player has state authority over the specified object.")]

    [Category("Fusion/Network Object/Has State Authority")]

    [Keywords("Fusion", "Authority", "State Authority", "Player")]
    
    [Image(typeof(IconCharacterState), ColorTheme.Type.Green, typeof(OverlayBolt))]
    
    [Serializable]
    public class ConditionHasStateAuthority : Condition
    {
        // PROPERTIES: ----------------------------------------------------------------------------
        [SerializeField] private PropertyGetGameObject target = GetGameObjectTarget.Create();
        protected override string Summary => $"Has State Authority of {target}";
        
        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            var go = target.Get(args);
            if(!go) return false;
            var networkObject = go.Get<NetworkObject>();
            return networkObject && networkObject.HasStateAuthority;
        }
    }
}