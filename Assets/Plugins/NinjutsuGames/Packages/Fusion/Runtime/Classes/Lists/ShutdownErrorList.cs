using System;
using Fusion;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Serializable]
    public class ShutdownErrorList : TPolymorphicList<ShutdownErrorItem>
    {
        [SerializeReference] private ShutdownErrorItem[] m_Errors =
        {
            new(ShutdownReason.Error,"Error", "Shutdown was caused by some internal error"),
            new(ShutdownReason.IncompatibleConfiguration,"Incompatible Config", "Mismatching type between client Server Mode and Shared Mode"),
            new(ShutdownReason.ServerInRoom,"Room name in use", "There's a room with that name! Please try a different name or wait a while."),
            new(ShutdownReason.DisconnectedByPluginLogic,"Disconnected By Plugin Logic", "You were kicked, the room may have been closed"),
            new(ShutdownReason.GameClosed,"Game Closed", "The session cannot be joined, the game is closed"),
            new(ShutdownReason.GameNotFound,"Game Not Found", "This room does not exist"),
            new(ShutdownReason.MaxCcuReached,"Max Players", "The Max CCU has been reached, please try again later"),
            new(ShutdownReason.InvalidRegion,"Invalid Region", "The currently selected region is invalid"),
            new(ShutdownReason.GameIdAlreadyExists,"ID already exists", "A room with this name has already been created"),
            new(ShutdownReason.GameIsFull,"Game is full", "This session is full!"),
            new(ShutdownReason.InvalidAuthentication,"Invalid Authentication", "The Authentication values are invalid"),
            new(ShutdownReason.CustomAuthenticationFailed,"Authentication Failed", "Custom authentication has failed"),
            new(ShutdownReason.AuthenticationTicketExpired,"Authentication Expired", "The authentication ticket has expired"),
            new(ShutdownReason.PhotonCloudTimeout,"Cloud Timeout", "Connection with the Photon Cloud has timed out"),
            new(ShutdownReason.AlreadyRunning,"Already Running", "A connection is already running"),
            new(ShutdownReason.InvalidArguments,"Invalid Arguments", "StartGame arguments are invalid"),
            new(ShutdownReason.HostMigration,"Host Migration", "The host is migrating"),
            new(ShutdownReason.ConnectionTimeout,"Timeout", "The remote server connection timed out"),
            new(ShutdownReason.ConnectionRefused,"Connection Refused", "The remote server refused the connection"),
            new(ShutdownReason.OperationCanceled,"Operation Canceled", "The current operation was canceled"),
            new(ShutdownReason.OperationTimeout,"Operation Timeout", "The current operation timed out"),
        };
    
        // PROPERTIES: ----------------------------------------------------------------------------

        public override int Length => m_Errors.Length;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public ShutdownErrorItem Get(ShutdownReason reason)
        {
            foreach (var error in m_Errors)
            {
                if (reason == error.Reason)
                {
                    return error;
                }
            }
            return null;
        }

        public ShutdownErrorItem Get(int index) => m_Errors[index];
        
        public ShutdownErrorItem[] GetAvailable()
        {
            var list = new ShutdownErrorItem[m_Errors.Length];
            var count = 0;
            foreach (var t in m_Errors)
            {
                if (!t.IsEnabled) continue;
                list[count] = t;
                count++;
            }

            Array.Resize(ref list, count);
            return list;
        }
    }
}