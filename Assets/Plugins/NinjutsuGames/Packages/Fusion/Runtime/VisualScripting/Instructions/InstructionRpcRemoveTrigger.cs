using System;
using System.Threading.Tasks;
using Fusion;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Remove Cached Trigger Rpc")]
    [Description("Remove player cached Trigger Rpc.")]

    [Category("Fusion/Visual Scripting/Remove Cached Trigger Rpc")]
    
    [Parameter("Trigger", "The Trigger target that will be removed from players Rpc cache.")]
    
    [Image(typeof(IconTriggers), ColorTheme.Type.Yellow, typeof(OverlayMinus))]
    
    [Keywords("Rpc", "Fusion", "Trigger", "Remove")] 
    [Serializable]
    public class InstructionRpcRemoveTrigger : Instruction
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private PropertyGetGameObject m_Trigger = GetGameObjectTrigger.Create();
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Remove Cached Conditions Rpc {m_Trigger}";

        // RUN METHOD: ----------------------------------------------------------------------------
        

        protected override Task Run(Args args)
        {
            var networkObject = m_Trigger.Get<NetworkObject>(args);
            var trigger = m_Trigger.Get<Trigger>(args);
            if (!trigger)
            {
                Debug.LogError($"[RPC] Trigger in {m_Trigger} is null.", args.Self);
                return DefaultResult;
            }
            if (!networkObject)
            {
                Debug.LogError($"[RPC] `{trigger.name}` doesn't have a NetworkObject component.", args.Self);
                return DefaultResult;
            }

            if (networkObject.Runner && !networkObject.Runner.IsRunning)
            {
                Debug.LogError("[RPC] Trigger Not connected to server", args.Self);
                return DefaultResult;
            }
            NetworkDataManager.TryRemoveCachedRpc(networkObject.Id, RPCReceiver.RpcType.Trigger);

            return DefaultResult;
        }
    }
}