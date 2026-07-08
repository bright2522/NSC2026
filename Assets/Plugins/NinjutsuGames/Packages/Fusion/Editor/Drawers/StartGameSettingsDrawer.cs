using GameCreator.Editor.Common;
using NinjutsuGames.FusionNetwork.Runtime;
using UnityEditor;

namespace NinjutsuGames.FusionNetwork.Editor
{
    [CustomPropertyDrawer(typeof(StartGameSettings))]
    public class StartGameSettingsDrawer : TBoxDrawer
    {
        protected override string Name(SerializedProperty property) => "Advanced Settings";
    }
}