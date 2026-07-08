using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Serializable]
    public class FloatingTextSettings
    {
        public PropertyGetGameObject prefab = GetGameObjectNone.Create();
        public PropertyGetPosition offset = GetPositionVector3.Create(new Vector3(0, 1, 0));
        public PropertyGetColor color = GetColorColorsWhite.Create;
        [Space(4)]
        public float duration = 6;
        public float fadeOutTime = 0.5f;
    }
}