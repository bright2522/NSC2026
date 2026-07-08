using System;
using GameCreator.Runtime.Common;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Session Is Open")]
    [Category("Fusion/Session Is Open")] 

    [Image(typeof(IconToggleOn), ColorTheme.Type.Green)]
    [Description("Set session is open.")]

    [Serializable]
    public class SetBoolSessionIsOpen : PropertyTypeSetBool
    {
        public override void Set(bool value, Args args)
        {
            if(NetworkManager.IsConnected)
            {
                NetworkManager.Runner.SessionInfo.IsOpen = value;
            }
        }

        public override bool Get(Args args)
        {
            return NetworkManager.IsConnected && NetworkManager.Runner.SessionInfo.IsOpen;
        }

        public override string String => $"Session Is Open";
    }
}