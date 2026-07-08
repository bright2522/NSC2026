using System;
using System.Threading.Tasks;
using Fusion;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Remove Cached Conditions Rpc")]
    [Description("Remove player cached Conditions Rpc.")]

    [Category("Fusion/Visual Scripting/Remove Cached Conditions Rpc")]
    
    [Parameter("Conditions", "The Conditions target that will be removed from players Rpc cache.")]
    
    [Image(typeof(IconConditions), ColorTheme.Type.Green, typeof(OverlayMinus))]
    
    [Keywords("Rpc", "Fusion", "Conditions", "Remove")] 
    [Serializable]
    public class InstructionRpcRemoveConditions : Instruction
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private PropertyGetGameObject m_Conditions = GetGameObjectConditions.Create();
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Remove Cached Conditions Rpc {m_Conditions}";

        // RUN METHOD: ----------------------------------------------------------------------------
        

        protected override Task Run(Args args)
        {
            var networkObject = m_Conditions.Get<NetworkObject>(args);
            var conditions = m_Conditions.Get<Conditions>(args);
            if (!conditions)
            {
                Debug.LogError($"[RPC] Conditions in {m_Conditions} is null.", args.Self);
                return DefaultResult;
            }
            if (!networkObject)
            {
                Debug.LogError($"[RPC] `{conditions.name}` doesn't have a NetworkObject component.", args.Self);
                return DefaultResult;
            }

            if (networkObject.Runner && !networkObject.Runner.IsRunning)
            {
                Debug.LogError("[RPC] Conditions Not connected to server", args.Self);
                return DefaultResult;
            }
            NetworkDataManager.TryRemoveCachedRpc(networkObject.Id, RPCReceiver.RpcType.Conditions);
            
            return DefaultResult;
        }
    }
}