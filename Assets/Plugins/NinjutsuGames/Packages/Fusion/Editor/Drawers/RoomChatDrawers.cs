using GameCreator.Editor.Common;
using NinjutsuGames.FusionNetwork.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace NinjutsuGames.FusionNetwork.Editor
{
    [CustomPropertyDrawer(typeof(ChatUISettings))]
    public class ChatUISettingsDrawer : TBoxDrawer
    {
        protected override string Name(SerializedProperty property) => "Settings";
        
        protected override void CreatePropertyContent(VisualElement container, SerializedProperty property)
        {
            var activateProp = property.FindPropertyRelative("activateOnInput");
            var allowFadeProp = property.FindPropertyRelative("allowChatFading");
            var activateOnInput = new Toggle
            {
                label = activateProp.displayName,
                bindingPath = activateProp.propertyPath
            };
            container.Add(activateOnInput);
            
            var allowChatFadeToggle = new Toggle
            {
                label = allowFadeProp.displayName,
                bindingPath = allowFadeProp.propertyPath
            };

            var inputField = new PropertyField(property.FindPropertyRelative("inputTrigger"));

            container.Add(inputField);
            container.Add(new SpaceSmall());
            container.Add(new PropertyField(property.FindPropertyRelative("maxLines")));
            container.Add(new PropertyField(property.FindPropertyRelative("minVisibleLines")));
            container.Add(new SpaceSmall());
            container.Add(allowChatFadeToggle);
            var fadeStartField = new PropertyField(property.FindPropertyRelative("fadeOutStart"));
            container.Add(fadeStartField);
            var fadeDurationField = new PropertyField(property.FindPropertyRelative("fadeOutDuration"));
            container.Add(fadeDurationField);
            var backgroundFadeStartField = new PropertyField(property.FindPropertyRelative("backgroundFadeOutDuration"));
            container.Add(backgroundFadeStartField);
            container.Add(new SpaceSmall());
            container.Add(new PropertyField(property.FindPropertyRelative("disablePlayerWhenTyping")));
            container.Add(new SpaceSmall());
            container.Add(new PropertyField(property.FindPropertyRelative("unseenMessages")));
            
            AlignLabel.On(container);
            AlignLabel.On(allowChatFadeToggle);
            
            activateOnInput.RegisterValueChangedCallback(changeEvent =>
            {
                inputField.style.display = changeEvent.newValue ? DisplayStyle.Flex : DisplayStyle.None;
            });
            inputField.style.display = activateProp.boolValue ? DisplayStyle.Flex : DisplayStyle.None;
            
            allowChatFadeToggle.RegisterValueChangedCallback(changeEvent =>
            {
                fadeStartField.style.display = changeEvent.newValue ? DisplayStyle.Flex : DisplayStyle.None;
                fadeDurationField.style.display = changeEvent.newValue ? DisplayStyle.Flex : DisplayStyle.None;
                backgroundFadeStartField.style.display = changeEvent.newValue ? DisplayStyle.Flex : DisplayStyle.None;
            });
            fadeStartField.style.display = allowFadeProp.boolValue ? DisplayStyle.Flex : DisplayStyle.None;
            fadeDurationField.style.display = allowFadeProp.boolValue ? DisplayStyle.Flex : DisplayStyle.None;
            backgroundFadeStartField.style.display = allowFadeProp.boolValue ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
    
    [CustomPropertyDrawer(typeof(ChatColors))]
    public class ChatColorsDrawer : TBoxDrawer
    {
        protected override string Name(SerializedProperty property) => "Colors";
    }
    
    [CustomPropertyDrawer(typeof(TextContent))]
    public class TextContentDrawer : TBoxDrawer
    {
        protected override string Name(SerializedProperty property) => "Text Content";
    }
    
    [CustomPropertyDrawer(typeof(ChatEvents))]
    public class ChatEventsDrawer : TBoxDrawer
    {
        protected override string Name(SerializedProperty property) => "Events";
        
        protected override void CreatePropertyContent(VisualElement container, SerializedProperty property)
        {
            var onOpen = property.FindPropertyRelative("onOpen");
            var onClose = property.FindPropertyRelative("onClose");
            var onMessage = property.FindPropertyRelative("onSendMessage");
            var onReceiveMessage = property.FindPropertyRelative("onReceiveMessage");
            
            container.Add(new LabelTitle("On Open:"));
            container.Add(new SpaceSmallest());
            container.Add(new PropertyField(onOpen));
            
            container.Add(new SpaceSmall());
            container.Add(new LabelTitle("On Close:"));
            container.Add(new SpaceSmallest());
            container.Add(new PropertyField(onClose));
            
            container.Add(new SpaceSmall());
            container.Add(new LabelTitle("On Send Message:"));
            container.Add(new SpaceSmallest());
            container.Add(new PropertyField(onMessage));
            
            container.Add(new SpaceSmall());
            container.Add(new LabelTitle("On Receive Message:"));
            container.Add(new SpaceSmallest());
            container.Add(new PropertyField(onReceiveMessage));
        }
    }
    
    [CustomPropertyDrawer(typeof(ProfanityFilter))]
    public class ProfanityFilterDrawer : TBoxDrawer
    {
        protected override string Name(SerializedProperty property) => "Profanity Filter";
    }
}