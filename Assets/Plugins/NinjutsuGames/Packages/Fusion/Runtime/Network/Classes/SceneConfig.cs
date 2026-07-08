using System;
using Fusion;
using GameCreator.Runtime.Common;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Serializable]
    public class SceneConfig : TPolymorphicItem<SceneConfig>
    {
        public PropertyGetString name = GetStringString.Create;
        public PropertyGetSprite sprite = GetSpriteInstance.Create();
        public PropertyGetScene scene = GetSceneAsset.Create;

        public SceneRef GetSceneRef(Args args) => SceneRef.FromIndex(scene.Get(args));
        
        public override string ToString() => name == null ? "None" : name.ToString();
    }
}