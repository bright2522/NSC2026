using System;
using Fusion.Photon.Realtime;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Serializable]
    public class StartGameSettings
    {
        [Tooltip("Enable this if the session name passed is a code from generator that needs to be validated.")]
        public bool validateSessionCode;
        
        [Tooltip("Options for matchmaking rules for JoinRandom.\n\n" +
                 "<b>FillRoom</b>: Fills up rooms (oldest first) to get players together as fast as possible. Default.\n\n" +
                 "<b>SerialMatching</b>: Distributes players across available rooms sequentially but takes filter into account. \n\n" +
                 "<b>RandomMatching</b>: Joins a (fully) random room. Expected properties must match but aside from this, any available room")]
        public MatchmakingMode MatchmakingMode = MatchmakingMode.FillRoom;
        
        [Tooltip("Custom Session Properties.")]
        public CollectorNameVariable SessionProperties = new(); 
        
        [Tooltip("Session should be created Open or Closed to accept joins")]
        public PropertyGetBool IsOpen = GetBoolTrue.Create;
        
        [Tooltip("Session should be Visible or not in the Session Lobby list")]
        public PropertyGetBool IsVisible = GetBoolTrue.Create;
        
        [Tooltip("Enables the Session creation when starting a Client with an specific Session Name")]
        public PropertyGetBool EnableClientSessionCreation = GetBoolTrue.Create;
        
        public PropertyGetString CustomLobbyName = GetStringEmpty.Create;
        public PropertyGetString CustomAppVersion = GetStringEmpty.Create;
        
    }
}