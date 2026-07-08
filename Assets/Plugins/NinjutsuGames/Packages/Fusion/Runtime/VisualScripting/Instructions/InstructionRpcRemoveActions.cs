using System;
using System.Threading.Tasks;
using Fusion;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Remove Cached Actions Rpc")]
    [Description("Remove player cached Actions Rpc.")]

    [Category("Fusion/Visual Scripting/Remove Cached Actions Rpc")]
    
    [Parameter("Actions", "The Actions target that will be removed from players Rpc cache")]
    
    [Image(typeof(IconInstructions), ColorTheme.Type.Blue, typeof(OverlayMinus))]
    
    [Keywords("Rpc", "Fusion", "Actions", "Remove")] 
    [Serializable]
    public class InstructionRpcRemoveActions : Instruction
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private PropertyGetGameObject m_Actions = GetGameObjectActions.Create();
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Remove Cached Actions Rpc {m_Actions}";

        // RUN METHOD: ----------------------------------------------------------------------------
        

        protected override Task Run(Args args)
        {
            var networkObject = m_Actions.Get<NetworkObject>(args);
            var actions = m_Actions.Get<Actions>(args);
            if (!actions)
            {
                Debug.LogError($"[RPC] Actions in {m_Actions} is null.", args.Self);
                return DefaultResult;
            }
            if (!networkObject)
            {
                Debug.LogError($"[RPC] `{actions.name}` doesn't have a NetworkObject component.", args.Self);
                return DefaultResult;
            }

            if (networkObject.Runner && !networkObject.Runner.IsRunning)
            {
                Debug.LogError("[RPC] Actions Not connected to server", args.Self);
                return DefaultResult;
            }
            NetworkDataManager.TryRemoveCachedRpc(networkObject.Id, RPCReceiver.RpcType.Actions);
            return DefaultResult;
        }
    }
}