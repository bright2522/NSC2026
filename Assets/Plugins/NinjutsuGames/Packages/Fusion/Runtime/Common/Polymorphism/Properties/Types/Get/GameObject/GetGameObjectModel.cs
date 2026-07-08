using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Model Prefab")]
    [Category("Fusion/Model Prefab")]
    
    [Image(typeof(IconCharacter), ColorTheme.Type.Yellow)]
    [Description("Returns the GameObject of the model from the specified list.")]

    [Serializable]
    public class GetGameObjectModel : PropertyTypeGetGameObject
    {
        [SerializeField] private CollectorListVariable m_ListVariable = new();
        [SerializeReference] private TListGetPick m_Element = new GetPickFirst();

        public override GameObject Get(Args args)
        {
            var source = m_ListVariable.Get(args);
            var index = m_Element.GetIndex(source.Count, args);
            var value = source[index] as ModelConfig;
            return value?.prefab.Get(args);
        }

        public static PropertyGetGameObject Create()
        {
            var instance = new GetGameObjectModel();
            return new PropertyGetGameObject(instance);
        }

        public override string String => "Model Prefab";
    }
}