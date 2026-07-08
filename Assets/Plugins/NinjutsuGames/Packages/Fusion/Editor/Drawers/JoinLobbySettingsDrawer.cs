using Fusion;
using NinjutsuGames.FusionNetwork.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace NinjutsuGames.FusionNetwork.Editor
{
    [CustomPropertyDrawer(typeof(JoinLobbySettings))]
    public class JoinLobbySettingsDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();
            
            var sessionLobby = property.FindPropertyRelative("sessionLobby");
            var lobbyId = property.FindPropertyRelative("lobbyId");
            var gameMode = property.FindPropertyRelative("gameMode");

            var sessionField = new PropertyField(sessionLobby);
            var lobbyField = new PropertyField(lobbyId);
            var gameModeField = new PropertyField(gameMode);
            
            root.Add(sessionField);
            root.Add(lobbyField);
            root.Add(gameModeField);
            
            // hide lobby field if sessionLobby is not set to Custom
            lobbyField.style.display = (SessionLobby)sessionLobby.enumValueIndex == SessionLobby.Custom ? DisplayStyle.Flex : DisplayStyle.None;

            sessionField.RegisterValueChangeCallback(changeEvent =>
            {
                lobbyField.style.display = (SessionLobby)changeEvent.changedProperty.enumValueIndex == SessionLobby.Custom ? DisplayStyle.Flex : DisplayStyle.None;
            });

            return root;
        }
    }
}