using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using NetworkObject = Fusion.NetworkObject;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Is Proxy")]
    [Description("Returns if LocalPlayer is neither the Input nor State Source for this network entity.")]

    [Category("Fusion/Network Object/Is Proxy")]

    [Keywords("Fusion", "Is Proxy", "Proxy", "Local Player")]
    
    [Image(typeof(IconCharacter), ColorTheme.Type.Yellow)]
    
    [Serializable]
    public class ConditionIsProxy : Condition 
    {
        // PROPERTIES: ----------------------------------------------------------------------------
        [SerializeField] private PropertyGetGameObject target = GetGameObjectTarget.Create();
        protected override string Summary => $"{target} Is Proxy";
        
        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            var go = target.Get(args);
            var networkObject = go.Get<NetworkObject>();
            return networkObject && networkObject.IsProxy;
        }
    }
}