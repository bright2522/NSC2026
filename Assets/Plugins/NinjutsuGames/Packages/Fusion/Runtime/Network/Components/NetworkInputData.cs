using Fusion;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    public struct NetworkInputData : INetworkInput
    {
        public Vector3 MoveDirection { get; set; }
        public Vector3 FaceDirection { get; set; }

        public int JumpCount { get; set; }
        // public float IsJumpingForce { get; set; }
    }
}