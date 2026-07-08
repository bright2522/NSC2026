using GameCreator.Editor.Common;
using NinjutsuGames.FusionNetwork.Runtime;
using UnityEditor;

namespace NinjutsuGames.FusionNetwork.Editor
{
    [CustomPropertyDrawer(typeof(FloatingTextSettings))]
    public class FloatingTextSettingsDrawer : TBoxDrawer
    {
        protected override string Name(SerializedProperty property) => "Settings";
    }
}