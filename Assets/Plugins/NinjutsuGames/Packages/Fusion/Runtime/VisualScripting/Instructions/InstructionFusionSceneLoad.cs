using System;
using System.Threading.Tasks;
using Fusion;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using LoadSceneMode = UnityEngine.SceneManagement.LoadSceneMode;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Load Scene")]
    [Description("Loads a new Scene in the Fusion Network. This only allowed to be called on the Server/Host or Master Client.")]

    [Category("Fusion/Load Scene")]

    [Parameter(
        "Scene",
        "The scene to be loaded"
    )]

    [Parameter(
        "Mode",
        "Single mode replaces all other scenes. Additive mode loads the scene on top of the others"
    )]
    
    [Keywords("Scene", "Load", "Fusion")]
    [Image(typeof(IconUnity), ColorTheme.Type.TextNormal, typeof(OverlayBolt))]
    
    [Serializable]
    public class InstructionFusionSceneLoad : Instruction
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------
        
        [SerializeField] private SceneLoadSelector scene = new();
        [SerializeField] private LocalPhysicsMode physicsMode = LocalPhysicsMode.None;
        [SerializeField] private bool setActiveOnLoad;
        [SerializeField] private bool m_WaitToFinish;
        
        // MEMBERS: -------------------------------------------------------------------------------
        
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title =>
            $"Load{(scene.loadSceneMode == LoadSceneMode.Additive ? " additive" : string.Empty)} scene {scene}";

        // RUN METHOD: ----------------------------------------------------------------------------
        
        protected override async Task Run(Args args)
        {
            if(!NetworkManager.IsConnected)
            {
                Debug.LogError("NetworkManager is not connected");
                return;
            }
            if(!NetworkManager.Runner.IsSceneAuthority)
            {
                Debug.LogError("Runner is not scene authority");
                return;
            }
            try
            {
                var sceneRef = scene.scene == SceneLoadSelector.SceneType.ByIndex 
                    ? SceneRef.FromIndex(scene.index.Get(args)) 
                    : SceneRef.FromPath(scene.name.Get(args));
                
                var loader = NetworkManager.Runner.LoadScene(sceneRef, scene.loadSceneMode, physicsMode, setActiveOnLoad);
                
                if (m_WaitToFinish)
                {
                    // Use a non-blocking approach with small time slices to avoid main thread blocking
                    // This is especially important for WebGL
                    const float checkInterval = 0.05f; // Check every 50ms
                    
                    while (!loader.IsDone && !ApplicationManager.IsExiting)
                    {
                        // Using a short delay between checks prevents blocking the main thread
                        await Time((int)(checkInterval * 1000));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"❌Error loading scene with fusion: {e.Message} - {e.StackTrace}");
                throw;
            }
        }
    }
}