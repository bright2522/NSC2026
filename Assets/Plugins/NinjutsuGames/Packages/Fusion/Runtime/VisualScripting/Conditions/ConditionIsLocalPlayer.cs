using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using NetworkObject = Fusion.NetworkObject;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Is Local Player")]
    [Description("Returns true if the target is the local player.")]

    [Category("Fusion/Network Object/Is Local Player")]

    [Keywords("Fusion", "Is Player", "Player", "Local Player")]
    
    [Image(typeof(IconPlayer), ColorTheme.Type.Green)]
    
    [Serializable]
    public class ConditionIsLocalPlayer : Condition
    {
        // PROPERTIES: ----------------------------------------------------------------------------
        [SerializeField] private PropertyGetGameObject target = GetGameObjectTarget.Create();
        protected override string Summary => $"Is Local Player";
        
        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            var p = target.Get(args);
            var no = p.Get<NetworkObject>();
            if (!no)
            {
                Debug.LogError($"No NetworkObject found for {p}");
                return false;
            }

            return no.InputAuthority == NetworkManager.Runner.LocalPlayer;
        }
    }
}