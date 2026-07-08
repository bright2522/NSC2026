using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Serializable]
    public struct NetworkProp : INetworkStruct
    {
        public NetworkString<_32> propName;
        public Vector3 position;
        public Quaternion rotation;

        public GameObject GetProp()
        {
            return NetworkManager.RuntimeAttachments.GetValueOrDefault(propName.Value);
        }
        
        public override string ToString()
        {
            return $"Prop: {propName}, Position: {position}, Rotation: {rotation}";
        }
    }
}