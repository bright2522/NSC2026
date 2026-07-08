using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Model Prefab Name")]
    [Category("Fusion/Models/Model Prefab Name")]

    [Image(typeof(IconCubeSolid), ColorTheme.Type.Yellow)]
    [Description("Returns the model prefab name from the specified list.")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetStringModelPrefabName : PropertyTypeGetString
    {
        [SerializeField] private CollectorListVariable m_ListVariable = new();
        [SerializeReference] private TListGetPick m_Element = new GetPickFirst();

        public override string Get(Args args)
        {
            var source = m_ListVariable.Get(args);
            var index = m_Element.GetIndex(source.Count, args);
            var value = source[index] as ModelConfig;
            return value?.GetPrefabName(args);
        }
        public static PropertyGetString Create => new(new GetStringModelPrefabName());

        public override string String => $"Model Prefab Name";
    }
}