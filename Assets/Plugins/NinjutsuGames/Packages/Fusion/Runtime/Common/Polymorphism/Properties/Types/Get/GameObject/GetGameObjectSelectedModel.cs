using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Selected Model Prefab")]
    [Category("Fusion/Selected Model Prefab")]
    
    [Image(typeof(IconCharacter), ColorTheme.Type.Yellow, typeof(OverlayTick))]
    [Description("Returns the prefab of the selected model.")]

    [Serializable]
    public class GetGameObjectSelectedModel : PropertyTypeGetGameObject
    {
        public override GameObject Get(Args args)
        {
            return NetworkManager.GetSelectedModelPrefab();
        }

        public static PropertyGetGameObject Create() => new(new GetGameObjectSelectedModel());

        public override string String => "Selected Model Prefab";
    }
}