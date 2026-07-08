using System;
using System.Collections;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Serializable]
    public class StringList : TPolymorphicList<StringItem>, IEnumerable
    {
        [SerializeReference] private StringItem[] m_Fields = Array.Empty<StringItem>();
    
        // PROPERTIES: ----------------------------------------------------------------------------

        public override int Length => m_Fields.Length;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public StringItem Get(int index) => m_Fields[index];
        public IEnumerator GetEnumerator()
        {
            foreach (var t in m_Fields) yield return t;
        }
    }
}