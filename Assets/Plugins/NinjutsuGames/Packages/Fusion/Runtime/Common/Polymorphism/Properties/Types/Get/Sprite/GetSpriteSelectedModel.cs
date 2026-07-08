using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Selected Model Sprite")]
    [Category("Fusion/Selected Model Sprite")]

    [Image(typeof(IconSprite), ColorTheme.Type.Yellow, typeof(OverlayTick))]
    [Description("Returns the selected model sprite.")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetSpriteSelectedModel : PropertyTypeGetSprite
    {
        public override Sprite Get(Args args)
        {
            return NetworkManager.GetSelectedModelSprite();
        }
        public static PropertyGetSprite Create => new(new GetSpriteSelectedModel());

        public override string String => $"Selected Model Sprite";
    }
}