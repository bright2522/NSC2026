using System;
using GameCreator.Runtime.Common;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Serializable]
    public class SceneUnloadSelector
    {
        public enum SceneType
        {
            ByIndex,
            ByName
        }
        
        public SceneType scene = SceneType.ByIndex;
        public PropertyGetScene index = GetSceneActive.Create;
        public PropertyGetString name = GetStringString.Create;
        
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