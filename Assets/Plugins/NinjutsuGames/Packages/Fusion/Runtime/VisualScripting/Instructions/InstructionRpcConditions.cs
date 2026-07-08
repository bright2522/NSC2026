using System;
using System.Threading.Tasks;
using Fusion;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Rpc Conditions")]
    [Description("Runs Conditions via RPC. The Trigger needs to have a NetworkObject component attached to it.")]

    [Category("Fusion/Visual Scripting/Rpc Conditions")]
    
    [Parameter("Rpc Targets", "The targets of the RPC<br>" +
                              "<br><b><i>All:</i></b> can be sent / is executed by all peers in the session (including the server)." +
                              "<br><br><b><i>Proxies:</i></b> can be sent / is executed by a peer who does not have either Input Authority or State Authority over the object." +
                              "<br><br><b><i>Input Authority:</i></b> can be sent / is executed by the peer with Input Authority over the object." +
                              "<br><br><b><i>State Authority:</i></b> can be sent / is executed by the peer with State Authority over the object.")]
    [Parameter("Conditions", "The trigger that will run this RPC. This needs to have a NetworkObject component attached to it.")]
    
    [Image(typeof(IconConditions), ColorTheme.Type.Green, typeof(OverlayBolt))]
    
    [Keywords("Rpc", "Fusion", "Conditions")]
    [Serializable]
    public class InstructionRpcConditions : Instruction
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private RpcTargets m_RpcTarget = RpcTargets.Proxies;
        [SerializeField] private PropertyGetBool m_CacheState = GetBoolFalse.Create;
        [SerializeField] private PropertyGetGameObject m_Conditions = GetGameObjectConditions.Create();
        [SerializeField] private bool m_WaitToFinish = true;
         
        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Rpc {m_Conditions} {(m_WaitToFinish ? "and wait" : string.Empty)}";


        // RUN METHOD: ----------------------------------------------------------------------------


        protected override async Task Run(Args args)
        {
            if(!NetworkManager.Runner.IsPlayer) return;
            if(args.Target.IsProxy()) return;

            var networkObject = m_Conditions.Get<NetworkObject>(args);
            var conditions = m_Conditions.Get<Conditions>(args);
            if (!conditions)
            {
                Debug.LogError($"$[RPC] Conditions in {m_Conditions} is null.", args.Self);
                return;
            }

            if (!networkObject)
            {
                Debug.LogError($"[RPC] `{conditions.name}` doesn't have a NetworkObject component.", args.Self);
                return;
            }

            if (networkObject.Runner && !networkObject.Runner.IsRunning)
            {
                Debug.LogError("[RPC] Conditions Not connected to server", args.Self);
                return;
            }

            NetworkDataManager.RPC(m_RpcTarget, networkObject.Id, RPCReceiver.RpcType.Conditions, m_CacheState.Get(args));
            if (m_WaitToFinish) await Until(() => conditions.IsRunning == false);
        }
    }
}