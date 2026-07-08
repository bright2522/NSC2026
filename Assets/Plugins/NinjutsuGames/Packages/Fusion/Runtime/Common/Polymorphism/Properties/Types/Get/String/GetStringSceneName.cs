using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Scene Name")]
    [Category("Fusion/Scenes/Scene Name")]

    [Image(typeof(IconUnity), ColorTheme.Type.White)]
    [Description("Returns the scene name from the specified list.")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetStringSceneName : PropertyTypeGetString
    {
        [SerializeField] private CollectorListVariable m_ListVariable = new();
        [SerializeReference] private TListGetPick m_Element = new GetPickFirst();

        public override string Get(Args args)
        {
            var source = m_ListVariable.Get(args);
            var index = m_Element.GetIndex(source.Count, args);
            var value = source[index] as SceneConfig;
            return value?.name.Get(args);
        }
        public static PropertyGetString Create => new(new GetStringSceneName());

        public override string String => $"Scene Name";
    }
}