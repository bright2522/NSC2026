using GameCreator.Editor.Common;
using GameCreator.Runtime.Common;
using NinjutsuGames.FusionNetwork.Runtime;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Editor
{
    public class FieldItemTool : TPolymorphicItemTool
    {
        private readonly IIcon _iconNumber = new IconNumber(ColorTheme.Type.Blue);
        private readonly IIcon _iconString = new IconString(ColorTheme.Type.Yellow);
        
        // PROPERTIES: ----------------------------------------------------------------------------

        protected override object Value => m_Property.GetValue<FieldItem>();
        
        protected override Texture2D GetIcon()
        {
            m_Property.serializedObject.Update();
            var instance = m_Property.GetValue<FieldItem>();
            return instance.Type == FieldItem.FieldType.Number ? _iconNumber.Texture : _iconString.Texture;
        }
        
        // CONSTRUCTOR: ---------------------------------------------------------------------------

        public FieldItemTool(IPolymorphicListTool parentTool, int index)
            : base(parentTool, index)
        { }
    }
}