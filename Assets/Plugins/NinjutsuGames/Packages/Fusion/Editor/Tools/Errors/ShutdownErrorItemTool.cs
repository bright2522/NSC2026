using GameCreator.Editor.Common;
using GameCreator.Runtime.Common;
using NinjutsuGames.FusionNetwork.Runtime;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Editor
{
    public class ShutdownErrorItemTool : TPolymorphicItemTool
    {
        private static readonly IIcon DefaultIcon = new IconBug(ColorTheme.Type.Red);
        
        // PROPERTIES: ----------------------------------------------------------------------------

        protected override object Value => m_Property.GetValue<ShutdownErrorItem>();

        protected override Texture2D GetIcon() => DefaultIcon.Texture;
        
        // CONSTRUCTOR: ---------------------------------------------------------------------------

        public ShutdownErrorItemTool(IPolymorphicListTool parentTool, int index)
            : base(parentTool, index)
        { }
    }
}