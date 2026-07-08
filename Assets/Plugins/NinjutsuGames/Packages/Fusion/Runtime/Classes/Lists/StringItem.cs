using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Serializable]
    public class StringItem : TPolymorphicItem<StringItem>
    {
        [SerializeField] private string m_Name;
        [SerializeField] private PropertyGetString m_Value = GetStringEmpty.Create;
        
        private GameObject _target;

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"{m_Name} = {m_Value}";
        
        public string Name => m_Name;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public string GetValue() => m_Value.Get(_target);

        public int CompareTo(StringItem itemB)
        {
            return string.Compare(GetValue(), itemB.GetValue(), StringComparison.Ordinal);
        }
    }
}