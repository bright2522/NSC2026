using System;
using GameCreator.Runtime.Common;
using LoadSceneMode = UnityEngine.SceneManagement.LoadSceneMode;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Serializable]
    public class SceneSelector
    {
        public enum SceneType
        {
            ByIndex,
            ByName,
            None
        }
        
        public SceneType scene = SceneType.ByIndex;
        public PropertyGetScene index = GetSceneActive.Create;
        public PropertyGetString name = GetStringString.Create;
        public LoadSceneMode loadSceneMode = LoadSceneMode.Single;
        
        public object Get(Args args)
        {
            return scene switch
            {
                SceneType.ByIndex => index.Get(args),
                SceneType.ByName => name.Get(args),
                _ => null
            };
        }
        
        public override string ToString()
        {
            return scene switch
            {
                SceneType.ByIndex => $"{index}",
                SceneType.ByName => $"{name}",
                _ => "None"
            };
        }
    }
}