using System;
using GameCreator.Runtime.Common;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Session Name")]
    [Category("Fusion/Session/Session Name")]

    [Image(typeof(IconString), ColorTheme.Type.Green)]
    [Description("Returns the name of the current session")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetStringSessionName : PropertyTypeGetString
    {
        public override string Get(Args args)
        {
            if(NetworkManager.IsConnected)
            {
                return NetworkManager.Runner.SessionInfo.Name;
            }

            var sessionItem = args.Target.Get<SessionItemUI>();
            return sessionItem ? sessionItem.SessionInfo.Name : string.Empty;
        }
        public override string String => $"Session Name";
    }
}