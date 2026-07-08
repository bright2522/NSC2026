using GameCreator.Editor.Common;
using NinjutsuGames.FusionNetwork.Runtime;
using UnityEditor;

namespace NinjutsuGames.FusionNetwork.Editor
{
    [CustomPropertyDrawer(typeof(FusionSessionCodeGenerator))]
    public class FusionSessionCodeGeneratorDrawer : TBoxDrawer
    {
        /*protected override string Title => "Session Code Generator";
        
        private const string Description = "Creates human readable random codes to be shared with other players.<br>The code is used to join a session.";

        protected override void CreateContent(VisualElement body, SerializedProperty property)
        {
            body.Add(new InfoMessage(Description));
            base.CreateContent(body, property);
        }*/
    }
}