using GameCreator.Editor.Common;
using NinjutsuGames.FusionNetwork.Runtime;
using UnityEditor;

namespace NinjutsuGames.FusionNetwork.Editor
{
    [CustomPropertyDrawer(typeof(LobbyAdvancedSettings))]
    public class LobbyAdvancedSettingsDrawer : TBoxDrawer
    {
        protected override string Name(SerializedProperty property) => "Advanced Settings";
    }
}