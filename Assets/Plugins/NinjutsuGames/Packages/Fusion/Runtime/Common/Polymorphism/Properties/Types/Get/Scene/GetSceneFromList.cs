using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Scene From List")]
    [Category("Scene From List")]
    
    [Image(typeof(IconUnity), ColorTheme.Type.TextNormal, typeof(OverlayListVariable))]
    [Description("Returns the scene from the specified list.")]

    [Serializable]
    public class GetSceneFromList : PropertyTypeGetScene
    {
        [SerializeField] protected CollectorListVariable m_SceneList = new();
        [SerializeReference] private TListGetPick m_Element = new GetPickFirst();

        public override int Get(Args args)
        {
            var source = m_SceneList.Get(args);
            var index = m_Element.GetIndex(source.Count, args);
            var value = source[index] as SceneConfig;
            return value?.scene.Get(args) ?? 0;
        }

        public static PropertyGetScene Create => new(
            new GetSceneFromList()
        );

        public override string String => $"Scene From List";
        
    }
}