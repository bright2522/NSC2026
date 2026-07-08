using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Spawn Player")]
    [Description("Spawns a new player instance through Fusion. This won't spawn a player in server mode.")]

    [Category("Fusion/Player/Spawn Player")]
    
    [Parameter("Prefab", "The prefab reference that is going to be spawned")]
    [Parameter("Position", "The position of the new game object instance")]
    [Parameter("Rotation", "The rotation of the new game object instance")]
    
    [Image(typeof(IconCharacter), ColorTheme.Type.Blue, typeof(OverlayPlus))]
    
    [Keywords("Spawn", "Player", "Game Object", "Fusion", "Instantiate")]
    [Serializable]
    public class InstructionSpawnPlayer : Instruction
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private PropertyGetNetworkPrefabRef prefab = GetNetworkPrefabRefInstance.Create();

        [SerializeField]
        private PropertyGetPosition m_Position = GetPositionVector3.Create(Vector3.up);

        [SerializeField]
        private PropertyGetRotation m_Rotation = GetRotationIdentity.Create;
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Spawn Player <b>{prefab}</b>";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override Task Run(Args args)
        {
            PlayerManager.AvatarSpawnData = new AvatarSpawnData
            {
                prefabId = NetworkManager.GetPrefabId(prefab.Get(args)),
                position = m_Position.Get(args),
                rotation = m_Rotation.Get(args),
            };
            return DefaultResult;
        }
    }
}