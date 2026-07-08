using System;
using GameCreator.Runtime.Common;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Session Is Visible")]
    [Category("Fusion/Session/Session Is Visible")]

    [Image(typeof(IconEye), ColorTheme.Type.Green)]
    [Description("Returns if the current session is visible or not")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetStringSessionIsVisible : PropertyTypeGetString
    {
        public override string Get(Args args)
        {
            var value = string.Empty;
            if(NetworkManager.IsConnected)
            {
                value = NetworkManager.Runner.SessionInfo.IsVisible.ToString();
            }
            return value;
        }
        public override string String => $"Session Is Visible";
    }
}