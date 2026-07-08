using System;
using Fusion;
using GameCreator.Runtime.Common;
using UnityEngine;
using Behaviour = Fusion.Behaviour;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    public class PooledNetworkObjectProvider : Behaviour, INetworkObjectProvider
    {
        public const int PLAYER_MANAGER = 100000;
        public const int PLAYER_DATA = 100001;
        public const int NETWORK_DATA_MANAGER = 100002;

        // The NetworkObjectBaker class can be reused and is Runner independent.
        private static NetworkObjectBaker _baker;
        private static NetworkObjectBaker Baker => _baker ??= new NetworkObjectBaker();
        
        /// <summary>
        /// If enabled, the provider will delay acquiring a prefab instance if the scene manager is busy.
        /// </summary>
        [InlineHelp] public bool DelayIfSceneManagerIsBusy = true;

        public NetworkObjectAcquireResult AcquirePrefabInstance(NetworkRunner runner,
            in NetworkPrefabAcquireContext context, out NetworkObject instance)
        {
            switch (context.PrefabId.RawValue)
            {
                case PLAYER_DATA:
                    instance = CreateGameObject(runner, context, "PlayerData", typeof(NetworkPlayer));
                    return NetworkObjectAcquireResult.Success;
                case PLAYER_MANAGER:
                    instance = CreateGameObject(runner, context, "PlayerManager", typeof(PlayerManager), true);
                    return NetworkObjectAcquireResult.Success;
                case NETWORK_DATA_MANAGER:
                    instance = CreateGameObject(runner, context, "NetworkDataManager", typeof(NetworkDataManager), true);
                    return NetworkObjectAcquireResult.Success;
            }
            
            instance = null;

            if (DelayIfSceneManagerIsBusy && runner.SceneManager.IsBusy)
            {
                return NetworkObjectAcquireResult.Retry;
            }

            NetworkObject prefab;
            try
            {
                // Use WebGL-safe loading to handle platform-specific limitations
                bool useSynchronous = context.IsSynchronous;
#if UNITY_WEBGL && !UNITY_EDITOR
                useSynchronous = false;
#endif
                prefab = runner.Prefabs.Load(context.PrefabId, useSynchronous);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to load prefab: {ex}");
                return NetworkObjectAcquireResult.Failed;
            }

            if (!prefab)
            {
                // this is ok, as long as Fusion does not require the prefab to be loaded immediately;
                // if an instance for this prefab is still needed, this method will be called again next update
                return context.IsSynchronous ? NetworkObjectAcquireResult.Failed : NetworkObjectAcquireResult.Retry;
            }
            
            var go = prefab.gameObject;
            var fusionSettings = FusionRepository.Get.Settings;
            
            if (fusionSettings.enableNetworkPooling)
            {
                var poolSize = fusionSettings.poolSize;
                instance = PoolManager.Instance.Pick(go, poolSize).Get<NetworkObject>();
            }
            else
            {
                // Use normal instantiation when pooling is disabled
                var instantiated = Instantiate(go);
                instance = instantiated.GetComponent<NetworkObject>();
            }

            Assert.Check(instance);

            if (context.DontDestroyOnLoad)
            {
                runner.MakeDontDestroyOnLoad(instance.gameObject);
            }
            else
            {
                runner.MoveToRunnerScene(instance.gameObject);
            }

            runner.Prefabs.AddInstance(context.PrefabId);
            return NetworkObjectAcquireResult.Success;
        }

        private NetworkObject CreateGameObject(NetworkRunner runner, in NetworkPrefabAcquireContext context,
            string name, Type componentType, bool isMasterClientObject = false)
        {
            var go = new GameObject(name);
            var no = go.AddComponent<NetworkObject>();

            if (isMasterClientObject)
            {
                no.Flags |= NetworkObjectFlags.MasterClientObject;
            }

            go.AddComponent(componentType);
            // Baking is required for the NetworkObject to be valid for spawning.
            Baker.Bake(go);

            // Move the object to the applicable Runner Scene/PhysicsScene/DontDestroyOnLoad
            // These implementations exist in the INetworkSceneManager assigned to the runner.
            if (context.DontDestroyOnLoad)
            {
                runner.MakeDontDestroyOnLoad(go);
            }
            else
            {
                runner.MoveToRunnerScene(go);
            }

            // We are finished. Return the NetworkObject and report success.
            return no;
        }
        
        public void ReleaseInstance(NetworkRunner runner, in NetworkObjectReleaseContext context)
        {
            var instance = context.Object;

            if (!context.IsBeingDestroyed)
            {
                if (instance.Get<NetworkDataManager>() 
                    || instance.Get<PlayerManager>() 
                    || instance.Get<NetworkPlayer>())
                {
                    Destroy(instance.gameObject);
                    return;
                }
                if (context.TypeId.IsPrefab)
                {
                    Remove(instance.gameObject);
                }
                else if (context.TypeId.IsSceneObject)
                {
                    Remove(instance.gameObject);
                }
                else if (context.IsNestedObject)
                {
                    instance.gameObject.SetActive(false);
                }
                else
                {
                    throw new NotImplementedException($"Unknown type id {context.TypeId}");
                }
            }

            if (context.TypeId.IsPrefab)
            {
                runner.Prefabs.RemoveInstance(context.TypeId.AsPrefabId);
            }
        }

        public NetworkPrefabId GetPrefabId(NetworkRunner runner, NetworkObjectGuid prefabGuid)
        {
            return runner.Prefabs.GetId(prefabGuid);
        }

        private void Remove(GameObject go)
        {
            var fusionSettings = FusionRepository.Get.Settings;
            
            if (!fusionSettings.enableNetworkPooling)
            {
                // When pooling is disabled, always destroy the object
                Destroy(go);
                return;
            }
            
            var isPoolInstance = go.Get<NetworkObjectPrefabData>();
            if (isPoolInstance)
            {
                go.SetActive(false);
            }
            else
            {
                Destroy(go);
            }
        }
    }
}