using System;
using System.Threading.Tasks;
using Fusion;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Spawn Object")]
    [Description("Attempts to network instantiate a NetworkObject using a GameObject. The supplied GameObject must have a NetworkObject component.")]

    [Category("Fusion/Network Object/Spawn Object")]
    
    // [Parameter("Game Object", "Game Object reference that is instantiated")]
    [Parameter("Prefab", "The Network Object reference that is instantiated")]
    [Parameter("Position", "The position of the new game object instance")]
    [Parameter("Rotation", "The rotation of the new game object instance")]
    [Parameter("Input Authority", "A PlayerRef to identify the client with input authority over the object. (Only relevant for Host/Server Mode)")]
    [Parameter("Network Spawn Flags", "DontDestroyOnLoad:<br>Object get spawned as DontDestroyOnLoad on all clients.<br><br>SharedModeStateAuthMasterClient:<br>In shared mode, override the state authority to MasterClient.<br><br>SharedModeStateAuthLocalPlayer:<br>In shared mode, override the state authority to local player.")]
    
    [Image(typeof(IconCubeSolid), ColorTheme.Type.Green)]
    
    [Keywords("Create", "New", "Game Object")]
    [Serializable]
    public class InstructionSpawn : Instruction
    {
        public enum InputAuthority
        {
            None,
            LocalPlayer
        }
        
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private PropertyGetNetworkPrefabRef prefab = GetNetworkPrefabRefInstance.Create();
        [Space]

        [SerializeField]
        private PropertyGetPosition m_Position = GetPositionCharactersPlayer.Create;

        [SerializeField]
        private PropertyGetRotation m_Rotation = GetRotationCharactersPlayer.Create;

        [Space]
        [SerializeField] private InputAuthority inputAuthority = InputAuthority.None;
        [SerializeField] private NetworkSpawnFlags networkSpawnFlags;
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Spawn {prefab}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override Task Run(Args args)
        {
            var position = m_Position.Get(args);
            var rotation = m_Rotation.Get(args);
            
            var inputAuth = inputAuthority == InputAuthority.LocalPlayer ? NetworkManager.Runner.LocalPlayer : PlayerRef.None;
            var instance = prefab.Get(args);
            if(!instance.IsValid) return DefaultResult;
            
            NetworkManager.Instance.TrySpawn(instance, inputAuth, position, rotation, networkSpawnFlags);
            return DefaultResult;
        }
    }
}