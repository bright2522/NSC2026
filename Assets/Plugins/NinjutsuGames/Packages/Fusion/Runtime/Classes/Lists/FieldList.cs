using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Serializable]
    public class FieldList : TPolymorphicList<FieldItem>
    {
        [SerializeReference] private FieldItem[] m_Fields = Array.Empty<FieldItem>();
    
        // PROPERTIES: ----------------------------------------------------------------------------

        public override int Length => m_Fields.Length;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public FieldItem Get(int index) => m_Fields[index];
    }
}