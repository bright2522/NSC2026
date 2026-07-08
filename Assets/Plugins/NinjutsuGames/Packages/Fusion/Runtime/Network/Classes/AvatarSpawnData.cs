using System;
using Fusion;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Serializable]
    public class AvatarSpawnData
    {
        public NetworkPrefabId prefabId;
        public Vector3 position;
        public Quaternion rotation;
    }
}