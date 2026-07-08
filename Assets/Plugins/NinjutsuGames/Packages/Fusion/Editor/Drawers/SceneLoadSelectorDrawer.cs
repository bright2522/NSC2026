using GameCreator.Editor.Common;
using NinjutsuGames.FusionNetwork.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace NinjutsuGames.FusionNetwork.Editor
{
    [CustomPropertyDrawer(typeof(SceneLoadSelector))]

    public class SceneLoadSelectorDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();
            var sceneType = property.FindPropertyRelative("scene");
            var sceneIndex = property.FindPropertyRelative("index");
            var sceneName = property.FindPropertyRelative("name");
            var loadSceneMode = property.FindPropertyRelative("loadSceneMode");

            var sceneField = new PropertyField(sceneType);
            container.Add(sceneField);

            var sceneIndexField = new PropertyField(sceneIndex, "");
            container.Add(sceneIndexField);
            
            var sceneNameField = new PropertyField(sceneName, "");
            container.Add(sceneNameField);

            var loadSceneField = new PropertyField(loadSceneMode);
            container.Add(loadSceneField);
            
            loadSceneField.style.display = sceneType.enumValueIndex != 2 ? DisplayStyle.Flex : DisplayStyle.None;
            sceneIndexField.style.display = sceneType.enumValueIndex == 0 && sceneType.enumValueIndex != 2 ? DisplayStyle.Flex : DisplayStyle.None;
            sceneNameField.style.display = sceneType.enumValueIndex == 1 && sceneType.enumValueIndex != 2 ? DisplayStyle.Flex : DisplayStyle.None;
            
            sceneField.RegisterValueChangeCallback(evt =>
            {
                loadSceneField.style.display = sceneType.enumValueIndex != 2 ? DisplayStyle.Flex : DisplayStyle.None;
                sceneIndexField.style.display = sceneType.enumValueIndex == 0 && sceneType.enumValueIndex != 2 ? DisplayStyle.Flex : DisplayStyle.None;
                sceneNameField.style.display = sceneType.enumValueIndex == 1 && sceneType.enumValueIndex != 2 ? DisplayStyle.Flex : DisplayStyle.None;
            });
            container.Add(new SpaceSmaller());
            return container;
        }
    }
}