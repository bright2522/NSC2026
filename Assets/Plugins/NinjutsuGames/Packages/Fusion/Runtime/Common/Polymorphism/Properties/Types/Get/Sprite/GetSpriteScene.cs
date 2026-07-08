using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Scene Sprite")]
    [Category("Fusion/Scene Sprite")]

    [Image(typeof(IconSprite), ColorTheme.Type.White)]
    [Description("Returns the scene sprite from the specified list.")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetSpriteScene : PropertyTypeGetSprite
    {
        [SerializeField] private CollectorListVariable m_ListVariable = new();
        [SerializeReference] private TListGetPick m_Element = new GetPickFirst();

        public override Sprite Get(Args args)
        {
            var source = m_ListVariable.Get(args);
            var index = m_Element.GetIndex(source.Count, args);
            var value = source[index] as SceneConfig;
            return value?.sprite.Get(args);
        }
        public static PropertyGetSprite Create => new(new GetSpriteScene());

        public override string String => $"Scene Sprite";
    }
}