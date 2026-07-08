using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;
#if FUSION_ENABLE_ADDRESSABLES && !FUSION_DISABLE_ADDRESSABLES
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
#endif

namespace NinjutsuGames.FusionNetwork.Runtime
{
    public class NetworkSceneManager : Fusion.Behaviour, INetworkSceneManager
    {
        public static event Action<SceneRef> EventSceneLoadStart;
        public static event Action<SceneRef> EventSceneLoadComplete;
        public static event Action<SceneRef, float> EventSceneLoadProgress;
        /// <summary>
        /// If enabled and there is an already loaded scene that matches what the scene manager has intended to load,
        /// that scene will be used instead and load will be avoided.
        /// </summary>
        [InlineHelp] [ToggleLeft] public bool isSceneTakeOverEnabled = true;

        /// <summary>
        /// Should all scene load errors be logged into the console? If disabled, errors can still be retrieved via the
        /// <see cref="NetworkSceneAsyncOp.Error"/> or <see cref="NetworkSceneAsyncOp.AddOnCompleted"/>.
        /// </summary>
        [InlineHelp] [ToggleLeft] public bool logSceneLoadErrors = true;

        /// <summary>
        /// All the scenes loaded by all the managers. Used when <see cref="isSceneTakeOverEnabled"/> is enabled.
        /// </summary>
        private static readonly Dictionary<Scene, NetworkSceneManager> ALL_OWNED_SCENES =
            new(new FusionUnitySceneManagerUtils.SceneEqualityComparer());

        /// <summary>
        /// In multiple peer mode, each runner maintains its own scene where all the newly loaded scenes
        /// are moved to. This is to make sure physics are properly sandboxed.
        /// </summary>
        private readonly List<MultiPeerSceneRoot> _multiPeerSceneRoots = new();

        private MultiPeerSceneRoot _multiPeerActiveRoot;

        /// <summary>
        /// List of running coroutines. Only one is actually executed at a time.
        /// </summary>
        private readonly List<ICoroutine> _runningCoroutines = new();

        /// <summary>
        /// For remote clients, this manager first unloads old scenes then loads the new ones. It might happen that all
        /// the current scenes need to be unloaded and in such case a temp scene needs to be created to ensure at least one
        /// scene loaded at all times. 
        /// </summary>
        private Scene _tempUnloadScene;

        /// <summary>
        /// Scene used when Multiple Peer mode is used. Each loaded scene is merged into this one, allowing
        /// for multiple runners to have separate cross-scene physics.
        /// </summary>
        public Scene MultiPeerScene { get; private set; }

        /// <summary>
        /// Root for DontDestroyOnLoad objects. Instantiated on <see cref="MultiPeerScene"/>.
        /// </summary>
        public Transform MultiPeerDontDestroyOnLoadRoot { get; private set; }

        public NetworkRunner Runner { get; private set; }

        private bool IsMultiplePeer => Runner.Config.PeerMode == NetworkProjectConfig.PeerModes.Multiple;
        private bool _isLoading;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearStatics()
        {
            ALL_OWNED_SCENES.Clear();
        }

        static NetworkSceneManager()
        {
            SceneManager.sceneUnloaded += (s) => ALL_OWNED_SCENES.Remove(s);
        }

        #region INetworkSceneManager

        public virtual void Initialize(NetworkRunner runner)
        {
            Log.TraceSceneManager(runner, $"Initialize with {runner}");

#if FUSION_ENABLE_ADDRESSABLES && !FUSION_DISABLE_ADDRESSABLES
            _ = LoadAddressableScenePathsAsync();
#endif

            Debug.Assert(!Runner);
            Runner = runner;

            // assign an empty scene with a separate physics stage immediately, so that they won't spawn anything on the currently active scene
            // an lose track of it
            if (IsMultiplePeer)
            {
                var scene = SceneManager.CreateScene($"{runner.name}_{runner.LocalPlayer}",
                    new CreateSceneParameters(LocalPhysicsMode.Physics2D | LocalPhysicsMode.Physics3D));
                Log.TraceSceneManager(Runner, $"Assigned an initial scene: {scene.Dump()}");

                MultiPeerScene = scene;
                MultiPeerDontDestroyOnLoadRoot = new GameObject("[DontDestroyOnLoad]").transform;
                SceneManager.MoveGameObjectToScene(MultiPeerDontDestroyOnLoadRoot.gameObject, MultiPeerScene);
            }
        }

        public virtual void Shutdown()
        {
            Log.TraceSceneManager(Runner, $"Shutdown with {Runner}");

            Runner = null;

            // clear owned scenes in case this manager is reused
            var ownedScenes = ALL_OWNED_SCENES
                .Where(x => x.Value == this)
                .Select(x => x.Key)
                .ToList();

            foreach (var ownedScene in ownedScenes)
            {
                ALL_OWNED_SCENES.Remove(ownedScene);
            }

            _multiPeerSceneRoots.Clear();
            _multiPeerActiveRoot = null;

            MultiPeerDontDestroyOnLoadRoot = null;

            var sceneToUnload = MultiPeerScene;
            MultiPeerScene = default;

            if (!sceneToUnload.isLoaded) return;
            if (!sceneToUnload.CanBeUnloaded())
            {
                SceneManager.CreateScene($"FusionSceneManager_TempEmptyScene");
            }

            SceneManager.UnloadSceneAsync(sceneToUnload);
        }

        public virtual bool IsBusy
        {
            get
            {
                if (_isLoading)
                {
                    return true;
                }

                if (IsMultiplePeer && _multiPeerSceneRoots.Count == 0)
                {
                    // nothing to spawn on
                    return true;
                }

                return false;
            }
        }

        public virtual Scene MainRunnerScene
        {
            get
            {
                if (IsMultiplePeer)
                {
                    return MultiPeerScene;
                }

                return SceneManager.GetActiveScene();
            }
        }

        public virtual bool IsRunnerScene(Scene scene)
        {
            if (IsMultiplePeer)
            {
                return scene == MultiPeerScene;
            }

            return true;
        }

        public virtual bool TryGetPhysicsScene2D(out PhysicsScene2D scene2D)
        {
            var mainScene = MainRunnerScene;
            if (mainScene.IsValid())
            {
                scene2D = mainScene.GetPhysicsScene2D();
                return true;
            }

            scene2D = default;
            return false;
        }

        public virtual bool TryGetPhysicsScene3D(out PhysicsScene scene3D)
        {
            var mainScene = MainRunnerScene;
            if (mainScene.IsValid())
            {
                scene3D = mainScene.GetPhysicsScene();
                return true;
            }

            scene3D = default;
            return false;
        }

        public virtual void MakeDontDestroyOnLoad(GameObject obj)
        {
            if (IsMultiplePeer)
            {
                Debug.Assert(obj.transform.parent == null || obj.transform.parent == MultiPeerDontDestroyOnLoadRoot);
                obj.transform.SetParent(MultiPeerDontDestroyOnLoadRoot, true);
            }
            else
            {
                DontDestroyOnLoad(obj);
            }
        }

        public bool MoveGameObjectToScene(GameObject gameObject, SceneRef sceneRef)
        {
            if (IsMultiplePeer)
            {
                // find the first matching scene ref
                foreach (var root in _multiPeerSceneRoots)
                {
                    if (sceneRef != default && root.sceneRef != sceneRef)
                    {
                        continue;
                    }

                    if (sceneRef == default)
                    {
                        // if scene ref is not specified, use the active root, if it exists
                        if (_multiPeerActiveRoot && root != _multiPeerActiveRoot)
                        {
                            continue;
                        }
                    }

                    if (gameObject.scene != MultiPeerScene)
                    {
                        gameObject.transform.SetParent(null, true);
                        SceneManager.MoveGameObjectToScene(gameObject, MultiPeerScene);

                        if (Application.isBatchMode == false)
                            Runner.AddVisibilityNodes(gameObject);
                    }

                    gameObject.transform.SetParent(root.transform, true);
                    return true;
                }

                return false;
            }

            if (sceneRef == default)
            {
                // do nothing, all scenes belong to the runner
                return true;
            }

            for (var i = 0; i < SceneManager.sceneCount; ++i)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded && GetSceneRef(scene.path) == sceneRef)
                {
                    SceneManager.MoveGameObjectToScene(gameObject, scene);
                    return true;
                }
            }

            return false;
        }

        public virtual NetworkSceneAsyncOp LoadScene(SceneRef sceneRef, NetworkLoadSceneParameters parameters)
        {
            Log.TraceSceneManager(Runner, $"Load scene {sceneRef} called with parameters: {parameters}");
            return NetworkSceneAsyncOp.FromCoroutine(sceneRef,
                StartTracedCoroutine(LoadSceneCoroutine(sceneRef, parameters)));
        }

        public virtual NetworkSceneAsyncOp UnloadScene(SceneRef sceneRef)
        {
            Log.TraceSceneManager(Runner, $"Unload scene {sceneRef} called");
            return NetworkSceneAsyncOp.FromCoroutine(sceneRef, StartTracedCoroutine(UnloadSceneCoroutine(sceneRef)));
        }

        public virtual SceneRef GetSceneRef(string sceneNameOrPath)
        {
            var buildIndex = FusionUnitySceneManagerUtils.GetSceneBuildIndex(sceneNameOrPath);
            if (buildIndex >= 0)
            {
                return SceneRef.FromIndex(buildIndex);
            }

#if FUSION_ENABLE_ADDRESSABLES && !FUSION_DISABLE_ADDRESSABLES
            // this may be a blocking call due to WaitForCompletion being used internally
            if (!TryGetAddressableScenes(out var addressableScenes))
            {
                Log.Error(this,
                    $"Failed to resolve addressable scene paths, won't be able to resolve {sceneNameOrPath} or any other addressable scene.");
                addressableScenes = Array.Empty<string>();
            }

            var index = FusionUnitySceneManagerUtils.GetSceneIndex(addressableScenes, sceneNameOrPath);
            if (index >= 0)
            {
                return SceneRef.FromPath(addressableScenes[index]);
            }
#endif

            return SceneRef.None;
        }

        public SceneRef GetSceneRef(GameObject gameObject)
        {
            if (IsMultiplePeer)
            {
                if (gameObject.scene != MultiPeerScene)
                {
                    // not a part of this scene
                    return default;
                }

                // find among scene roots
                var sceneRoot = gameObject.transform.root;
                foreach (var root in _multiPeerSceneRoots)
                {
                    if (root.transform == sceneRoot)
                    {
                        return root.sceneRef;
                    }
                }

                return default;
            }

            var scene = gameObject.scene;
            return GetSceneRef(scene.path);
        }

        public bool OnSceneInfoChanged(NetworkSceneInfo sceneInfo, NetworkSceneInfoChangeSource changeSource)
        {
            // implement this method and return true if you want to handle scene info changes manually
            return false;
        }

        #endregion

        protected virtual IEnumerator LoadSceneCoroutine(SceneRef sceneRef, NetworkLoadSceneParameters sceneParams)
        {
            Runner.InvokeSceneLoadStart(sceneRef);
            EventSceneLoadStart?.Invoke(sceneRef);

            // Yield immediately to allow the scene load start event to be processed
            yield return null;

            Scene scene = default;

            using (MakeLoadingScope())
            {
                Log.TraceSceneManager(Runner, $"LoadSceneCoroutine called with {sceneRef}, {sceneParams}");
                var localPhysicsMode = sceneParams.LocalPhysicsMode;
                var loadSceneMode = sceneParams.LoadSceneMode;

                if (IsMultiplePeer)
                {
                    if (localPhysicsMode != LocalPhysicsMode.None)
                    {
                        throw new ArgumentException($"Local physics mode is not supported in multiple peer mode",
                            nameof(sceneParams));
                    }

                    if (loadSceneMode == LoadSceneMode.Single)
                    {
                        // all the current scenes need to be "unloaded", except possibly for the one
                        // that matches the sceneRef, if scene take over is enabled
                        loadSceneMode = LoadSceneMode.Additive;

                        try
                        {
                            // Destroy roots in chunks to avoid freezing
                            const int destroyChunkSize = 5;
                            for (var i = 0; i < _multiPeerSceneRoots.Count; i += destroyChunkSize)
                            {
                                var endIndex = Math.Min(i + destroyChunkSize, _multiPeerSceneRoots.Count);
                                for (var j = i; j < endIndex; j++)
                                {
                                    var root = _multiPeerSceneRoots[j];
                                    Log.TraceSceneManager(Runner,
                                        $"Destroying scene {sceneRef} root {root.name} due to single-mode load");
                                    Destroy(root.gameObject);
                                }
                                
                                // Yield after each chunk
                                yield return null;
                            }

                            // Wait for each root to be destroyed, but with a timeout
                            var timeout = Time.realtimeSinceStartup + 5f; // 5 second timeout
                            bool allDestroyed;
                            do
                            {
                                allDestroyed = true;
                                foreach (var root in _multiPeerSceneRoots)
                                {
                                    if (root != null)
                                    {
                                        allDestroyed = false;
                                        break;
                                    }
                                }
                                
                                if (!allDestroyed)
                                {
                                    yield return null;
                                    
                                    // Check for timeout
                                    if (Time.realtimeSinceStartup > timeout)
                                    {
                                        Log.Warn(Runner, "Timeout waiting for scene roots to be destroyed");
                                        break;
                                    }
                                }
                            } while (!allDestroyed);
                        }
                        finally
                        {
                            _multiPeerSceneRoots.Clear();
                        }
                    }
                }

                // Yield to ensure UI responsiveness
                yield return null;

                if (isSceneTakeOverEnabled)
                {
                    // check if a loaded scene can be taken over
                    var candidate = FindSceneToTakeOver(sceneRef);
                    if (candidate.IsValid())
                    {
                        Log.TraceSceneManager(Runner, $"Taking over {sceneRef}: {candidate.Dump()}");

                        if (candidate.GetLocalPhysicsMode() != localPhysicsMode)
                        {
                            throw new InvalidOperationException(
                                $"Tried to take over {candidate.Dump()} for {sceneRef}, but physics mode were different: {candidate.GetLocalPhysicsMode()} != {localPhysicsMode}");
                        }

                        scene = candidate;
                        MarkSceneAsOwned(sceneRef, candidate);

                        if (loadSceneMode == LoadSceneMode.Single && !IsMultiplePeer)
                        {
                            // need to unload scenes manually, multiple peer mode is handled at the beginning of this method, because
                            // it always needs to the manual cleanup for single mode
                            
                            // Get all scenes to unload first to avoid modifying during iteration
                            var scenesToUnload = new List<Scene>();
                            for (var i = 0; i < SceneManager.sceneCount; i++)
                            {
                                var toUnload = SceneManager.GetSceneAt(i);
                                if (toUnload != candidate)
                                {
                                    scenesToUnload.Add(toUnload);
                                }
                            }
                            
                            // Unload scenes one by one with yields between
                            foreach (var toUnload in scenesToUnload)
                            {
                                Log.TraceSceneManager(Runner,
                                    $"Unloading {sceneRef} ({toUnload.Dump()}) due to single-mode take over of {candidate.Dump()}");
                                var unloadOp = SceneManager.UnloadSceneAsync(toUnload);
                                
                                // Wait for unload to complete with periodic yields
                                while (unloadOp != null && !unloadOp.isDone)
                                {
                                    yield return null;
                                }
                                
                                // Additional yield after each scene unload
                                yield return null;
                            }
                        }
                    }
                }

                // Yield to ensure UI responsiveness
                yield return null;

                if (!scene.IsValid())
                {
#if FUSION_ENABLE_ADDRESSABLES && !FUSION_DISABLE_ADDRESSABLES
                    if (loadSceneMode == LoadSceneMode.Single)
                    {
                        // single mode unloads all the scenes anyway
                        _addressableOperations.Clear();
                    }
#endif

                    if (sceneRef.IsIndex)
                    {
                        Log.TraceSceneManager(Runner,
                            $"Loading scene {sceneRef} with build index {sceneRef.AsIndex} with mode {loadSceneMode}");
                        var op = SceneManager.LoadSceneAsync(sceneRef.AsIndex,
                            new LoadSceneParameters(loadSceneMode, localPhysicsMode));
                        if (op == null)
                        {
                            throw new InvalidOperationException($"Scene not found: {sceneRef.AsIndex}");
                        }

                        // Set allowSceneActivation to true to ensure scene loads immediately
                        op.allowSceneActivation = true;
                        
                        Debug.Assert(SceneManager.sceneCount > 0);
                        scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
                        MarkSceneAsOwned(sceneRef, scene);

                        Debug.Assert(scene.buildIndex == sceneRef.AsIndex);

                        // Report progress more frequently with shorter waits
                        var lastReportedProgress = 0f;
                        while (!op.isDone)
                        {
                            // Only report progress if it's changed significantly
                            if (op.progress - lastReportedProgress > 0.05f)
                            {
                                OnLoadSceneProgress(sceneRef, op.progress);
                                lastReportedProgress = op.progress;
                            }
                            
                            yield return null;
                        }
                        
                        // Final progress report
                        OnLoadSceneProgress(sceneRef, 1.0f);
                    }
                    else
                    {
#if FUSION_ENABLE_ADDRESSABLES && !FUSION_DISABLE_ADDRESSABLES
                        if (!TryGetAddressableScenes(out var addressableScenes))
                        {
                            Log.Error(this,
                                $"Failed to resolve addressable scene paths, won't be able to resolve {sceneRef}");
                            addressableScenes = Array.Empty<string>();
                        }

                        string sceneAddress = null;
                        foreach (var path in addressableScenes)
                        {
                            if (sceneRef.IsPath(path))
                            {
                                sceneAddress = path;
                                break;
                            }
                        }

                        if (sceneAddress == null)
                        {
                            throw new InvalidOperationException(
                                $"Unable to find addressable scene path for {sceneRef}");
                        }

                        Log.TraceSceneManager(Runner, $"Loading scene {sceneRef} from addressable: {sceneAddress}");

#if FUSION_ENABLE_ADDRESSABLES_LOCAL_PHYSICS
            var loadSceneParameters = new LoadSceneParameters(loadSceneMode, localPhysicsMode);
#else
                        if (localPhysicsMode != LocalPhysicsMode.None)
                        {
                            throw new InvalidOperationException(
                                $"{nameof(LocalPhysicsMode)} is not supported in this version of Addressables");
                        }

                        var loadSceneParameters = loadSceneMode;
#endif
                        var op = Addressables.LoadSceneAsync(sceneAddress, loadSceneParameters);

                        // to get the scene a callback is used, as it fires immediately when loading finished,
                        // compared to waiting for the coroutine to resume
                        scene = default;
                        op.Completed += op =>
                        {
                            if (op.Status == AsyncOperationStatus.Succeeded)
                            {
                                scene = op.Result.Scene;
                                MarkSceneAsOwned(sceneRef, scene);
                            }
                        };

                        op.Destroyed += _ =>
                        {
                            // this will happen in MP mode when scenes are merged or when a scene is loaded in a single mode
                            if (_addressableOperations.Remove(sceneRef))
                            {
                                Log.TraceSceneManager(Runner, $"Destroyed Addressables op for {sceneRef}");
                            }
                        };

                        _addressableOperations.Add(sceneRef, op);

                        // Report progress more frequently with shorter waits
                        var lastReportedProgress = 0f;
                        while (!op.IsDone)
                        {
                            // Only report progress if it's changed significantly
                            if (op.PercentComplete - lastReportedProgress > 0.05f)
                            {
                                OnLoadSceneProgress(sceneRef, op.PercentComplete);
                                lastReportedProgress = op.PercentComplete;
                            }
                            
                            yield return null;
                        }

                        if (!op.IsValid())
                        {
                            throw new InvalidOperationException($"Loading operation for {sceneRef} has been destroyed");
                        }

                        if (op.Status == AsyncOperationStatus.Failed)
                        {
                            _addressableOperations.Remove(sceneRef);
                            Addressables.Release(op);
                            throw new InvalidOperationException(
                                $"Failed to load scene from addressable: {sceneAddress}");
                        }
#else
                        throw new InvalidOperationException(
                            $"SceneRef {sceneRef} points to an addressable scene, but FUSION_ENABLE_ADDRESSABLES is not defined");
#endif
                    }
                }
                
                // Final yield before processing the loaded scene
                yield return null;
            }

            yield return StartCoroutine(OnSceneLoaded(sceneRef, scene, sceneParams));
        }

        protected virtual IEnumerator UnloadSceneCoroutine(SceneRef sceneRef)
        {
            Log.TraceSceneManager(Runner, $"UnloadSceneCoroutine called for {sceneRef}");

            using (MakeLoadingScope())
            {
                if (IsMultiplePeer)
                {
                    // in multiple peer, the unload simply destroys the scene root
                    for (var i = 0; i < _multiPeerSceneRoots.Count; ++i)
                    {
                        var root = _multiPeerSceneRoots[i];
                        if (root.sceneRef == sceneRef)
                        {
                            if (root == _multiPeerActiveRoot)
                            {
                                _multiPeerActiveRoot = null;
                            }

                            _multiPeerSceneRoots.RemoveAt(i);
                            Log.TraceSceneManager(Runner, $"Destroying scene root {root.name} for {sceneRef}");

                            Log.TraceSceneManager(Runner, $"Started unloading {root.scene.ToString()} for {sceneRef}");
                            Destroy(root.gameObject);
                            while (root != null)
                            {
                                yield return null;
                            }

                            Log.TraceSceneManager(Runner, $"Finished unloading {root.scene.ToString()} for {sceneRef}");
                            yield break;
                        }
                    }

                    throw new ArgumentOutOfRangeException($"Did not find a scene to unload: {sceneRef}",
                        nameof(sceneRef));
                }

                Scene sceneToUnload = default;

                // find the scene to unload
                for (var i = 0; i < SceneManager.sceneCount; ++i)
                {
                    var scene = SceneManager.GetSceneAt(i);
                    if (GetSceneRef(scene.path) == sceneRef)
                    {
                        sceneToUnload = scene;
                        break;
                    }
                }

                if (!sceneToUnload.IsValid())
                {
                    throw new ArgumentOutOfRangeException($"Did not find a scene to unload: {sceneRef}",
                        nameof(sceneRef));
                }

                Log.TraceSceneManager(Runner, $"Started unloading {sceneToUnload.Dump()} for {sceneRef}");

                if (!sceneToUnload.CanBeUnloaded())
                {
                    Log.Warn(Runner,
                        $"Scene {sceneToUnload.Dump()} can't be unloaded for {sceneRef}, creating a temporary scene to unload it");
                    
                    // Only create a new temporary scene if we don't already have one
                    if (!_tempUnloadScene.IsValid())
                    {
                        // In WebGL, creating a scene can be expensive, so yield before continuing
                        if (Application.platform == RuntimePlatform.WebGLPlayer)
                        {
                            yield return null;
                        }
                        
                        _tempUnloadScene = SceneManager.CreateScene($"FusionSceneManager_TempEmptyScene");
                        
                        // Give a frame for the scene creation to complete
                        yield return null;
                    }
                }

#if FUSION_ENABLE_ADDRESSABLES && !FUSION_DISABLE_ADDRESSABLES
                if (_addressableOperations.TryGetValue(sceneRef, out var asyncOp))
                {
                    Log.TraceSceneManager(Runner, $"Unloading addressable scene {sceneToUnload.Dump()} for {sceneRef}");
                    yield return Addressables.UnloadSceneAsync(asyncOp);
                }
                else
#endif
                {
                    Log.TraceSceneManager(Runner, $"Unloading {sceneToUnload.Dump()} for {sceneRef}");
                    var op = SceneManager.UnloadSceneAsync(sceneToUnload);
                    if (op == null)
                    {
                        throw new InvalidOperationException($"Failed to unload {sceneToUnload.Dump()}");
                    }

                    yield return op;
                }

                Log.TraceSceneManager(Runner, $"Finished unloading {sceneToUnload.Dump()} for {sceneRef}");
            }
        }

        protected virtual IEnumerator OnSceneLoaded(SceneRef sceneRef, Scene scene,
            NetworkLoadSceneParameters sceneParams)
        {
            Log.TraceSceneManager(Runner, $"Finished loading, starting processing {scene.Dump()} for {sceneRef}");

            // Always yield after scene load to ensure the next frame has started
            yield return null;
            
            // Get scene objects in a separate frame to avoid freezing
            NetworkObject[] sceneObjects = null;
            GameObject[] rootObjects = null;
            
            yield return StartCoroutine(GetSceneObjectsAsync(scene, (objects, roots) => {
                sceneObjects = objects;
                rootObjects = roots;
            }));
            
            // Always yield after getting scene objects
            yield return null;
            
            // Sort objects in a separate frame
            yield return StartCoroutine(SortSceneObjectsAsync(sceneObjects));
            
            // Process the scene based on peer mode
            if (IsMultiplePeer)
            {
                // Create a root GO for all the gameObjects in the newly loaded scene
                var newSceneRoot = new GameObject($"[{scene.name}]").AddComponent<MultiPeerSceneRoot>();
                newSceneRoot.sceneRef = sceneRef;
                newSceneRoot.sceneHandle = scene.handle;
                newSceneRoot.scene = scene;
                newSceneRoot.scenePath = scene.path;
                
                SceneManager.MoveGameObjectToScene(newSceneRoot.gameObject, scene);
                
                // Process root objects in chunks with yields between chunks
                yield return StartCoroutine(ProcessRootObjectsAsync(rootObjects, newSceneRoot.transform));
                
                // Store the info
                _multiPeerSceneRoots.Add(newSceneRoot);
                
                Log.TraceSceneManager(Runner, $"Merging {scene.Dump()} to {MultiPeerScene.Dump()} for {sceneRef}");
                
                // Yield before merging scenes
                yield return null;
                
                SceneManager.MergeScenes(scene, MultiPeerScene);
                
                if (sceneParams.IsActiveOnLoad)
                {
                    _multiPeerActiveRoot = newSceneRoot;
                }
            }
            else
            {
                if (sceneParams.IsActiveOnLoad)
                {
                    SceneManager.SetActiveScene(scene);
                }
            }
            
            // Always register objects in chunks regardless of platform
            yield return StartCoroutine(RegisterSceneObjectsAsync(sceneRef, sceneObjects, sceneParams.LoadId));
            
            Log.TraceSceneManager(Runner, $"Finished loading & processing {scene.Dump()} for {sceneRef}");
            Runner.InvokeSceneLoadDone(new SceneLoadDoneArgs(sceneRef, sceneObjects, scene, rootObjects));
            EventSceneLoadComplete?.Invoke(sceneRef);
        }
        
        private IEnumerator GetSceneObjectsAsync(Scene scene, Action<NetworkObject[], GameObject[]> callback)
        {
            NetworkObject[] sceneObjects = null;
            GameObject[] rootObjects = null;
            
            // Use a coroutine to get scene objects to avoid freezing the main thread
            yield return null;
            
            sceneObjects = scene.GetComponents<NetworkObject>(includeInactive: true, out rootObjects);
            
            callback(sceneObjects, rootObjects);
        }
        
        private IEnumerator SortSceneObjectsAsync(NetworkObject[] sceneObjects)
        {
            // For very large scenes, sort in a separate frame
            if (sceneObjects.Length > 100)
            {
                yield return null;
            }
            
            // Since it is impossible to get objects in deterministic order (sibling index is 0 for all root objects in builds),
            // scene objects need to be sorted by something that will guarantee the order
            Array.Sort(sceneObjects, NetworkObjectSortKeyComparer.Instance);
        }
        
        private IEnumerator ProcessRootObjectsAsync(GameObject[] rootObjects, Transform parentTransform)
        {
            const int chunkSize = 20; // Smaller chunk size for better responsiveness
            
            for (var i = 0; i < rootObjects.Length; i += chunkSize)
            {
                var endIndex = Math.Min(i + chunkSize, rootObjects.Length);
                for (var j = i; j < endIndex; j++)
                {
                    rootObjects[j].transform.SetParent(parentTransform, true);
                }
                
                // Always yield after each chunk to maintain responsiveness
                yield return null;
            }
        }
        
        private IEnumerator RegisterSceneObjectsAsync(SceneRef sceneRef, NetworkObject[] sceneObjects, NetworkSceneLoadId loadId)
        {
            // Use smaller chunks for better responsiveness
            const int registerChunkSize = 50;
            
            for (var i = 0; i < sceneObjects.Length; i += registerChunkSize)
            {
                var count = Math.Min(registerChunkSize, sceneObjects.Length - i);
                var chunk = new NetworkObject[count];
                Array.Copy(sceneObjects, i, chunk, 0, count);
                
                // Register this chunk
                Runner.RegisterSceneObjects(sceneRef, chunk, loadId);
                
                // Always yield after each chunk
                yield return null;
            }
        }

        protected virtual void OnLoadSceneProgress(SceneRef sceneRef, float progress)
        {
            Log.TraceSceneManager(Runner, $"Loading scene progress {sceneRef} ({progress:P2})");
            EventSceneLoadProgress?.Invoke(sceneRef, progress);
        }

        private Scene FindSceneToTakeOver(SceneRef sceneRef)
        {
            // Avoid checking all scenes if there are no scenes loaded
            if (SceneManager.sceneCount == 0)
            {
                return default;
            }
            
            // Fast path: try active scene first as it's the most likely candidate
            var activeScene = SceneManager.GetActiveScene();
            if (activeScene.isLoaded && 
                GetSceneRef(activeScene.path) == sceneRef && 
                !ALL_OWNED_SCENES.ContainsKey(activeScene))
            {
                return activeScene;
            }
            
            // Check remaining scenes if active scene wasn't a match
            for (var i = 0; i < SceneManager.sceneCount; ++i)
            {
                var candidate = SceneManager.GetSceneAt(i);
                // Skip the active scene since we already checked it
                if (candidate == activeScene)
                {
                    continue;
                }
                
                if (!candidate.isLoaded)
                {
                    continue;
                }

                if (GetSceneRef(candidate.path) != sceneRef)
                {
                    continue;
                }

                if (ALL_OWNED_SCENES.ContainsKey(candidate))
                {
                    continue;
                }

                return candidate;
            }

            return default;
        }

        private ICoroutine StartTracedCoroutine(IEnumerator inner)
        {
            var coro = new FusionCoroutine(inner);

            _runningCoroutines.Add(coro);

            coro.Completed += x =>
            {
                if (logSceneLoadErrors && x.Error != null)
                {
                    Log.Error(Runner, $"Failed async op: {x.Error.SourceException}");
                }

                // remove this one from the list
                var index = _runningCoroutines.IndexOf((ICoroutine)x);
                Debug.Assert(index == 0, "Expected the completed coroutine to be the first in the list");
                _runningCoroutines.RemoveAt(index);

                // start the next one
                if (index < _runningCoroutines.Count)
                {
                    Log.TraceSceneManager(Runner, $"Starting enqueued coroutine {index} of {_runningCoroutines.Count}");
                    StartCoroutine(_runningCoroutines[index]);
                }
            };

            if (_runningCoroutines.Count == 1)
            {
                // start immediately
                StartCoroutine(coro);
            }
            else
            {
                Log.TraceSceneManager(Runner,
                    $"Enqueued coroutine, there are already {_runningCoroutines.Count - 1} running");
            }

            return coro;
        }

        protected LoadingScope MakeLoadingScope()
        {
            return new LoadingScope(this);
        }

        protected void MarkSceneAsOwned(SceneRef sceneRef, Scene scene)
        {
            if (ALL_OWNED_SCENES.TryGetValue(scene, out var manager))
            {
                Log.Warn(Runner, $"Scene {scene.Dump()} (for {sceneRef}) already owned by {manager}");
            }
            else
            {
                ALL_OWNED_SCENES.Add(scene, this);
            }
        }

        private NetworkSceneAsyncOp FailOp(SceneRef sceneRef, Exception exception)
        {
            if (logSceneLoadErrors)
            {
                Log.Error(Runner, $"Failed with: {exception}");
            }

            return NetworkSceneAsyncOp.FromError(sceneRef, exception);
        }

#if FUSION_ENABLE_ADDRESSABLES && !FUSION_DISABLE_ADDRESSABLES
        /// <summary>
        /// A label by which addressable scenes can be discovered.
        /// </summary>
        [InlineHelp] public string AddressableScenesLabel = "FusionScenes";

        public NetworkSceneManager()
        {
            _addressableScenesTask = new Lazy<GetAddressableScenesResult>(GetAddressableScenes);
        }

        public Task LoadAddressableScenePathsAsync()
        {
            return _addressableScenesTask.Value.Task;
        }

        /// <summary>
        /// Creates a task that resolves addressable scene paths. By default, this method locates all the addressable scenes with
        /// <see cref="AddressableScenesLabel"/> label. Override this method to provide a custom implementation. For example, user
        /// might want to have a pre-defined set of addressable scenes to avoid the wait:
        /// <example><code>
        /// protected override GetAddressableScenesResult GetAddressableScenes() {
        ///   return Task.FromResult(new string[] {
        ///     "Assets/Scenes/AddressableScene1.unity",
        ///     "Assets/Scenes/AddressableScene2.unity",
        ///   });
        /// }
        /// </code></example>
        /// </summary>
        /// <returns>A task representing resolve operation and optionally a delegate to be invoked before the task is going to be
        /// awaited synchronously</returns>
        protected virtual GetAddressableScenesResult GetAddressableScenes()
        {
            Log.TraceSceneManager(Runner, $"Locating addressable scenes with label: {AddressableScenesLabel}");

            var tcs = new TaskCompletionSource<string[]>();
            var result = Addressables.LoadResourceLocationsAsync(AddressableScenesLabel, typeof(SceneInstance));

            result.Completed += op =>
            {
                try
                {
                    if (op.Status == AsyncOperationStatus.Failed)
                    {
                        tcs.SetException(op.OperationException);
                    }
                    else
                    {
                        var paths = op.Result.Select(x => x.PrimaryKey).ToArray();
                        Log.TraceSceneManager(Runner,
                            $"Found {paths.Length} addressable scenes: {string.Join(", ", paths)}");
                        tcs.SetResult(paths);
                    }
                }
                finally
                {
                    Addressables.Release(op);
                }
            };

            return new GetAddressableScenesResult
            {
                Task = tcs.Task,

                // awaiting tasks synchronously does not play well with addressables; simply waiting will block the main thread and that's it.
                // addressables *need* to have WaitForCompletion called
                BeforeWaitForCompletion = () =>
                {
                    if (result.IsValid())
                    {
                        result.WaitForCompletion();
                    }
                },
            };
        }

        /// <summary>
        /// Returns the timeout for addressable scene paths to be resolved. By default, this method returns 10 seconds.
        /// </summary>
        /// <returns></returns>
        protected virtual TimeSpan GetAddressableScenePathsTimeout()
        {
            return TimeSpan.FromSeconds(10);
        }

        private bool TryGetAddressableScenes(out string[] addressableScenes)
        {
            if (!_addressableScenesTask.IsValueCreated)
            {
                Log.Info(Runner,
                    $"Initializing addressable scene paths. Consider calling {nameof(LoadAddressableScenePathsAsync)} earlier.");
            }
        
            var t = _addressableScenesTask.Value;
            if (!t.Task.IsCompleted)
            {
                // If we're in WebGL, we can't block with Task.Wait
                // Instead, return cached results if available, or empty array if first run
                if (Application.platform == RuntimePlatform.WebGLPlayer)
                {
                    if (_cachedAddressableScenes != null)
                    {
                        addressableScenes = _cachedAddressableScenes;
                        return true;
                    }
                    
                    addressableScenes = Array.Empty<string>();
                    StartCoroutine(CacheAddressableScenesWhenReady(t.Task));
                    return false;
                }
                else
                {
                    // For non-WebGL platforms, we can use the original approach but with a safety timeout
                    t.BeforeWaitForCompletion?.Invoke();
                    
                    if (!t.Task.Wait(GetAddressableScenePathsTimeout()))
                    {
                        addressableScenes = _cachedAddressableScenes ?? Array.Empty<string>();
                        return _cachedAddressableScenes != null;
                    }
                }
            }
        
            if (t.Task.Status == TaskStatus.RanToCompletion)
            {
                addressableScenes = t.Task.Result;
                _cachedAddressableScenes = addressableScenes; // Cache the result for future fast access
                return true;
            }
            else
            {
                addressableScenes = _cachedAddressableScenes ?? Array.Empty<string>();
                return _cachedAddressableScenes != null;
            }
        }
        
        private string[] _cachedAddressableScenes;
        
        private IEnumerator CacheAddressableScenesWhenReady(Task<string[]> task)
        {
            while (!task.IsCompleted)
            {
                yield return null;
            }
            
            if (task.Status == TaskStatus.RanToCompletion)
            {
                _cachedAddressableScenes = task.Result;
            }
            else
            {
                Log.Error(Runner, $"Failed to load addressable scenes: {(task.Exception != null ? task.Exception.Message : "Unknown error")}");
                _cachedAddressableScenes = Array.Empty<string>();
            }
        }

        protected struct GetAddressableScenesResult
        {
            public Task<string[]> Task;
            public Action BeforeWaitForCompletion;

            public static implicit operator GetAddressableScenesResult(Task<string[]> task)
            {
                return new GetAddressableScenesResult
                {
                    Task = task,
                };
            }
        }

        private readonly Lazy<GetAddressableScenesResult> _addressableScenesTask;
        private readonly Dictionary<SceneRef, AsyncOperationHandle<SceneInstance>> _addressableOperations = new();
#endif

        protected sealed class MultiPeerSceneRoot : MonoBehaviour
        {
            public SceneRef sceneRef;
            public string scenePath;
            public int sceneHandle;
            public Scene scene;
        }

        protected struct LoadingScope : IDisposable
        {
            private readonly NetworkSceneManager _manager;

            public LoadingScope(NetworkSceneManager manager)
            {
                _manager = manager;
                _manager._isLoading = true;
                Log.TraceSceneManager(manager.Runner, "Loading scope started");
            }

            public void Dispose()
            {
                _manager._isLoading = false;
                Log.TraceSceneManager(_manager.Runner, "Loading scope ended");
            }
        }
    }
}