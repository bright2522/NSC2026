using GameCreator.Editor.Common;
using GameCreator.Runtime.Common;
using NinjutsuGames.FusionNetwork.Runtime;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Editor
{
    public class RegionItemTool : TPolymorphicItemTool
    {
        private readonly IIcon _iconEnabled = new IconSphereOutline(ColorTheme.Type.Blue);
        private readonly IIcon _iconDisabled = new IconSphereOutline(ColorTheme.Type.Red);
        
        // PROPERTIES: ----------------------------------------------------------------------------

        protected override object Value => m_Property.GetValue<RegionItem>();
        
        protected override Texture2D GetIcon()
        {
            m_Property.serializedObject.Update();
            var instance = m_Property.GetValue<RegionItem>();
            return instance.IsEnabled ? _iconEnabled.Texture : _iconDisabled.Texture;
        }
        
        // CONSTRUCTOR: ---------------------------------------------------------------------------

        public RegionItemTool(IPolymorphicListTool parentTool, int index)
            : base(parentTool, index)
        { }
    }
}