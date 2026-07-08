using System;
using GameCreator.Runtime.Common;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Session Is Visible")]
    [Category("Fusion/Session Is Visible")] 

    [Image(typeof(IconVisibleOn), ColorTheme.Type.Green)]
    [Description("Set session is visible.")]

    [Serializable]
    public class SetBoolSessionIsVisible : PropertyTypeSetBool
    {
        public override void Set(bool value, Args args)
        {
            if(NetworkManager.IsConnected)
            {
                NetworkManager.Runner.SessionInfo.IsVisible = value;
            }
        }

        public override bool Get(Args args)
        {
            return NetworkManager.IsConnected && NetworkManager.Runner.SessionInfo.IsVisible;
        }

        public override string String => $"Session Is Visible";
    }
}