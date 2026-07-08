using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Serializable]
    public class ModelConfig : TPolymorphicItem<ModelConfig>
    {
        public PropertyGetString name = GetStringString.Create;
        public PropertyGetSprite sprite = GetSpriteInstance.Create();
        public PropertyGetGameObject prefab = GetGameObjectInstance.Create();
        public Skeleton skeleton;
        public MaterialSoundsAsset materialSounds;
        public Vector3 offset;
        
        public string GetPrefabName(Args args) => prefab.Get(args).name;
        
        public override string ToString() => prefab == null ? "None" : prefab.ToString();
    }
}