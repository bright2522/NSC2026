using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Model Sprite")]
    [Category("Fusion/Model Sprite")]

    [Image(typeof(IconSprite), ColorTheme.Type.Yellow)]
    [Description("Returns the model sprite from the specified list.")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetSpriteModel : PropertyTypeGetSprite
    {
        [SerializeField] private CollectorListVariable m_ListVariable = new();
        [SerializeReference] private TListGetPick m_Element = new GetPickFirst();

        public override Sprite Get(Args args)
        {
            var source = m_ListVariable.Get(args);
            var index = m_Element.GetIndex(source.Count, args);
            var value = source[index] as ModelConfig;
            return value?.sprite.Get(args);
        }
        public static PropertyGetSprite Create => new(new GetSpriteModel());

        public override string String => $"Model Sprite";
    }
}