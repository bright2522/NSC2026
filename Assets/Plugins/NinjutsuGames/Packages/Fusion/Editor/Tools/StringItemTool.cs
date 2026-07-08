using GameCreator.Editor.Common;
using GameCreator.Runtime.Common;
using NinjutsuGames.FusionNetwork.Runtime;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Editor
{
    public class StringItemTool : TPolymorphicItemTool
    {
        private readonly IIcon _iconString = new IconString(ColorTheme.Type.Yellow);
        
        // PROPERTIES: ----------------------------------------------------------------------------

        protected override object Value => m_Property.GetValue<StringItem>();

        protected override Texture2D GetIcon() => _iconString.Texture;
        
        // CONSTRUCTOR: ---------------------------------------------------------------------------

        public StringItemTool(IPolymorphicListTool parentTool, int index)
            : base(parentTool, index)
        { }
    }
}