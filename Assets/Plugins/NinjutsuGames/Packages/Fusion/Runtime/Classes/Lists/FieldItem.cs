using System;
using System.Globalization;
using Fusion;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Common.UnityUI;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Serializable]
    public class FieldItem : TPolymorphicItem<FieldItem>
    {
        public enum FieldType
        {
            Number,
            String,
        }
        [SerializeField] private FieldType m_Type = FieldType.Number;
        [SerializeField] private TextReference m_Text;
        [SerializeField] private PropertyGetDecimal m_Number;
        [SerializeField] private PropertyGetString m_String = GetStringEmpty.Create;
        [SerializeField] private bool m_UseColor;
        [SerializeField] private PropertyGetColor m_Color = GetColorColorsWhite.Create;
        [SerializeField] private bool m_UseFormat;
        [SerializeField] private string format = "{0}";
        
        private GameObject _target;

        // PROPERTIES: ----------------------------------------------------------------------------

        public FieldType Type => m_Type;
        public override string Title => $"{m_Text} = {(m_Type == FieldType.Number ? m_Number : m_String)}";
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public string GetText() => m_String.Get(_target);
        
        public double GetNumber() => m_Number.Get(_target);

        public void Refresh(GameObject target, SessionInfo sessionInfo)
        {
            _target = target;
            var text =  m_UseFormat ? 
                string.Format(CultureInfo.InvariantCulture, format, m_Type == FieldType.Number ? GetNumber() : GetText()) : 
                (m_Type == FieldType.Number ? GetNumber().ToString(CultureInfo.InvariantCulture) : GetText());
            
            if(m_UseColor) m_Text.Color = m_Color.Get(target);
            m_Text.Text = text;
        }

        public int CompareTo(FieldItem itemB)
        {
            return m_Type == FieldType.Number ? m_Number.Get(_target).CompareTo(itemB.GetNumber()) : string.Compare(GetText(), itemB.GetText(), StringComparison.Ordinal);
        }
    }
}