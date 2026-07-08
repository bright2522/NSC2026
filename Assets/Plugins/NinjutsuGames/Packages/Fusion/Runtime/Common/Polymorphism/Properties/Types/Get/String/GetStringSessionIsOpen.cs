using System;
using GameCreator.Runtime.Common;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Session Is Open")]
    [Category("Fusion/Session/Session Is Open")]

    [Image(typeof(IconToggleOn), ColorTheme.Type.Green)]
    [Description("Returns if the current session is open or not")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetStringSessionIsOpen : PropertyTypeGetString
    {
        public override string Get(Args args)
        {
            var value = string.Empty;
            if(NetworkManager.IsConnected)
            {
                value = NetworkManager.Runner.SessionInfo.IsOpen.ToString();
            }
            return value;
        }
        public override string String => $"Session Is Open";
    }
}