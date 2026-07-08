using System;
using GameCreator.Runtime.Common;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Lobby Name")]
    [Category("Fusion/Lobby/Lobby Name")]

    [Image(typeof(IconString), ColorTheme.Type.Green)]
    [Description("Returns the name of the current lobby session")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetStringLobbyName : PropertyTypeGetString
    {
        public override string Get(Args args)
        {
            var sessionName = string.Empty;
            if(NetworkManager.IsConnectedInLobby)
            {
                sessionName = NetworkManager.RunnerLobby.LobbyInfo.Name;
            }
            return sessionName;
        }
        public override string String => $"Lobby Name";
    }
}