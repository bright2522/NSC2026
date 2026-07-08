using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Serializable]
    public class FusionCoreSettings
    {
        [Tooltip("Use this if you need to setup a custom runner prefab")]
        public PropertyGetGameObject customRunnerPrefab = GetGameObjectNone.Create();
        
        [Tooltip("The default player name when a new player joins the game")]
        public string defaultPlayerName = "Player {0}";
        
        [Tooltip("The size of the object pool for network objects")]
        public int poolSize = 10;
        
        [Tooltip("Enable network object pooling. When disabled, objects will be instantiated and destroyed normally instead of using Game Creator's pooling system")]
        public bool enableNetworkPooling = true;
    }
}