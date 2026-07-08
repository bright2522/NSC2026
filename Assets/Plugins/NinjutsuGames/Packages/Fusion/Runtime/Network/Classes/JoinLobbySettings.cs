using System;
using Fusion;
using GameCreator.Runtime.Common;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Serializable]
    public class JoinLobbySettings
    {
        public SessionLobby sessionLobby = SessionLobby.Shared;
        public PropertyGetString lobbyId = GetStringEmpty.Create;
        public GameMode gameMode = GameMode.Shared;
    }
}