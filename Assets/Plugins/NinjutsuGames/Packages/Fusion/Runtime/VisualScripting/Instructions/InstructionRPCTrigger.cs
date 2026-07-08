using System;
using System.Threading.Tasks;
using Fusion;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Rpc Trigger")]
    [Description("Runs a Trigger via RPC. The Trigger needs to have a NetworkObject component attached to it.")]

    [Category("Fusion/Visual Scripting/Rpc Trigger")]
    
    [Parameter("Rpc Targets", "The targets of the RPC<br>" +
                              "<br><b><i>All:</i></b> can be sent / is executed by all peers in the session (including the server)." +
                              "<br><br><b><i>Proxies:</i></b> can be sent / is executed by a peer who does not have either Input Authority or State Authority over the object." +
                              "<br><br><b><i>Input Authority:</i></b> can be sent / is executed by the peer with Input Authority over the object." +
                              "<br><br><b><i>State Authority:</i></b> can be sent / is executed by the peer with State Authority over the object.")]
    [Parameter("Cache State", "If true, the state of the trigger will be cached and sent to newly connected peers.")]
    [Parameter("Trigger", "The trigger that will run this RPC. This needs to have a NetworkObject component attached to it.")]
    
    [Image(typeof(IconTriggers), ColorTheme.Type.Yellow, typeof(OverlayBolt))]
    
    [Keywords("Rpc", "Fusion", "Trigger")]
    [Serializable]
    public class InstructionRpcTrigger : Instruction
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private RpcTargets m_RpcTarget = RpcTargets.Proxies;
        [SerializeField] private PropertyGetBool m_CacheState = GetBoolFalse.Create;
        [SerializeField] private PropertyGetGameObject m_Trigger = GetGameObjectTrigger.Create();
        [SerializeField] private bool m_WaitToFinish = true;
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Rpc {m_Trigger} {(m_WaitToFinish ? "and wait" : string.Empty)}";


        // RUN METHOD: ----------------------------------------------------------------------------
        

        protected override async Task Run(Args args)
        {
            if(!NetworkManager.Runner.IsPlayer) return;
            if(args.Target.IsProxy()) return;
            
            var networkObject = m_Trigger.Get<NetworkObject>(args);
            var trigger = m_Trigger.Get<Trigger>(args);
            if (!trigger)
            {
                Debug.LogError($"[RPC] Trigger in {m_Trigger} is null.", args.Self);
                return;
            } 
            if (!networkObject)
            {
                Debug.LogError($"[RPC] `{trigger.name}` doesn't have a NetworkObject component.", args.Self);
                return;
            }

            if (networkObject.Runner && !networkObject.Runner.IsRunning)
            {
                Debug.LogError("[RPC] Trigger Not connected to server", args.Self);
                return;
            }
            NetworkDataManager.RPC(m_RpcTarget, networkObject.Id, RPCReceiver.RpcType.Trigger, m_CacheState.Get(args));
            
            if (m_WaitToFinish) await Until(() => !trigger.IsExecuting);
        }
    }
}