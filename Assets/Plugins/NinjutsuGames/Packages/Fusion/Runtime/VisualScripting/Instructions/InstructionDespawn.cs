using System;
using System.Threading.Tasks;
using Fusion;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Despawn Object")]
    [Description("Attempts to network despawn a NetworkObject using a GameObject. The supplied GameObject must have a NetworkObject component.")]

    [Category("Fusion/Network Object/Despawn Object")]
    
    [Parameter("Instance", "The Network Object reference that is despawned")]
    
    [Image(typeof(IconCubeSolid), ColorTheme.Type.Red)]
    
    [Keywords("Destroy", "Despawn", "Game Object")]
    [Serializable]
    public class InstructionDespawn : Instruction
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private PropertyGetGameObject networkObject = GetGameObjectInstance.Create();
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Despawn Network Object";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override Task Run(Args args)
        {
            var go = networkObject.Get(args);
            if (!go)
            {
                Debug.LogError($"[Despawn] GameObject in {networkObject} is null.", args.Self);
                return Task.CompletedTask;
            }

            var no = go.Get<NetworkObject>();
            if (!no)
            {
                Debug.LogError($"[Despawn] `{go.name}` doesn't have a NetworkObject component.", args.Self);
                return Task.CompletedTask;
            }
            
            NetworkManager.Instance.TryDespawn(no.Id);

            return DefaultResult;
        }
    }
}