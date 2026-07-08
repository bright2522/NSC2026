using System;
using System.Threading.Tasks;
using Fusion;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Request State Authority")]
    [Description("Request state authority over a NetworkObject on shared mode.<br>The NetworkObject must have the flag 'Allow State Authority Override' enabled.")]

    [Category("Fusion/Network Object/Request State Authority")]
    
    [Parameter("Target", "The target NetworkObject to request state authority")]
    
    [Image(typeof(IconCharacterState), ColorTheme.Type.Green, typeof(OverlayArrowRight))]
    
    [Keywords("Authority", "Request", "Network Object")]
    [Serializable]
    public class InstructionRequestStateAuthority : Instruction
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private PropertyGetGameObject target = GetGameObjectTarget.Create();
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Request state authority of {target}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override Task Run(Args args)
        {
            var go = target.Get(args);
            if(!go) return DefaultResult;
            var networkObject = go.Get<NetworkObject>();
            if(!networkObject)
            {
                Debug.LogError($"[Request State Authority] Couldn't find NetworkObject on {go}", args.Self);
                return DefaultResult;
            }
            if(networkObject && networkObject.Runner && 
               networkObject.Runner.IsRunning && !networkObject.HasStateAuthority)
            {
                networkObject.RequestStateAuthority();
            }
            return DefaultResult;
        }
    }
}