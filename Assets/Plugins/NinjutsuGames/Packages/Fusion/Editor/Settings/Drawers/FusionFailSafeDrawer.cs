using System;
using GameCreator.Editor.Common;
using NinjutsuGames.FusionNetwork.Runtime;
using UnityEditor;
using UnityEngine.UIElements;

namespace NinjutsuGames.FusionNetwork.Editor
{
    [CustomPropertyDrawer(typeof(FusionFailSafe))]
    public class FusionFailSafeDrawer : TBoxDrawer
    {
        protected override string Name(SerializedProperty property) => "Fail Safe";

        protected override void CreatePropertyContent(VisualElement container, SerializedProperty property)
        {
            base.CreatePropertyContent(container, property);
            container.Add(new SpaceSmall());
            // Initiate shutdown button
            var shutdownButton = new Button(() =>
            {
                FusionFailSafeManager.Instance.InitiateSessionShutdown();
            });
            shutdownButton.SetEnabled(FusionFailSafeManager.Instance);
            shutdownButton.text = "Shutdown";
            container.Add(shutdownButton);
            
            // Test exception errors
            var throwErrorButton = new Button(() =>
            {
                for (int i = 0; i < 10; i++)
                {
                    throw new Exception("Test Exception");
                }
            });
            throwErrorButton.SetEnabled(FusionFailSafeManager.Instance);
            throwErrorButton.text = "Throw Error";
            container.Add(throwErrorButton);
        }
    }
}