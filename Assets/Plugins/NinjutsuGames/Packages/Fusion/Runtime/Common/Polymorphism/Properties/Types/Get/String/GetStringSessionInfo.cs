using System;
using GameCreator.Runtime.Common;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Session Info")]
    [Category("Fusion/Session/Session Info")]

    [Image(typeof(IconInfoSolid), ColorTheme.Type.Blue)]
    [Description("Returns the complete info of the current session")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetStringSessionInfo : PropertyTypeGetString
    {
        public override string Get(Args args)
        {
            var sessionName = string.Empty;
            if(NetworkManager.IsConnected)
            {
                sessionName = NetworkManager.Runner.SessionInfo.ToString();
            }
            return sessionName;
        }
        public override string String => $"Session Info";
    }
}