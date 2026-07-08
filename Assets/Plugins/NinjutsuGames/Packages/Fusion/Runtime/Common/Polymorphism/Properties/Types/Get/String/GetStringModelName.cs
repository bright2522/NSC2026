using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Model Name")]
    [Category("Fusion/Models/Model Name")]

    [Image(typeof(IconString), ColorTheme.Type.Yellow)]
    [Description("Returns the model name from the specified list.")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetStringModelName : PropertyTypeGetString
    {
        [SerializeField] private CollectorListVariable m_ListVariable = new();
        [SerializeReference] private TListGetPick m_Element = new GetPickFirst();

        public override string Get(Args args)
        {
            var source = m_ListVariable.Get(args);
            var index = m_Element.GetIndex(source.Count, args);
            var value = source[index] as ModelConfig;
            return value?.name.Get(args);
        }
        public static PropertyGetString Create => new(new GetStringModelName());

        public override string String => $"Model Name";
    }
}