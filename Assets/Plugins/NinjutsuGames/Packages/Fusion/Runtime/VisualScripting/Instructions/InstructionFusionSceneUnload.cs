using System;
using System.Threading.Tasks;
using Fusion;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Unload Scene")]
    [Description("Unloads a Scene in the Fusion Network. This only allowed to be called on the Server/Host or Master Client.")]

    [Category("Fusion/Unload Scene")]

    [Parameter(
        "Scene",
        "The scene to be loaded"
    )]

    [Parameter(
        "Mode",
        "Single mode replaces all other scenes. Additive mode loads the scene on top of the others"
    )]
    
    [Keywords("Scene", "Load", "Fusion")]
    [Image(typeof(IconUnity), ColorTheme.Type.Red, typeof(OverlayBolt))]
    
    [Serializable]
    public class InstructionFusionSceneUnload : Instruction
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------
        
        [SerializeField] private SceneUnloadSelector scene = new();
        [SerializeField] private bool m_WaitToFinish;
        
        private NetworkSceneAsyncOp m_Loader;

        // MEMBERS: -------------------------------------------------------------------------------
        
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title =>
            $"Unload scene {scene}";

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
            var startingScene = scene.Get(args);
            var sceneRef = startingScene is int i ? SceneRef.FromIndex(i) : SceneRef.FromPath((string)startingScene);
            if (sceneRef.IsValid)
            {
                if (m_WaitToFinish)
                {
                    m_Loader = NetworkManager.Runner.UnloadScene(sceneRef);
                    await Until(() => m_Loader.IsDone || ApplicationManager.IsExiting);
                }
                else
                {
                    _ = NetworkManager.Runner.UnloadScene(sceneRef);
                }
            }
        }
    }
}