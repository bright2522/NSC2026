using GameCreator.Editor.Common;
using NinjutsuGames.FusionNetwork.Runtime;
using UnityEditor;
using UnityEngine.UIElements;

namespace NinjutsuGames.FusionNetwork.Editor
{
    [CustomPropertyDrawer(typeof(FusionCoreSettings))]
    public class FusionCoreSettingsDrawer : TTitleDrawer
    {
        protected override string Title => "General";

        protected override void CreateContent(VisualElement body, SerializedProperty property)
        {
            base.CreateContent(body, property);
            body.Add(new SpaceSmall());
        }
    }
}