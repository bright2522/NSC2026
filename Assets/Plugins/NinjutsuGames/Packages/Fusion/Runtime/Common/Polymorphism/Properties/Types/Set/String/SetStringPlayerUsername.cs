using System;
using GameCreator.Runtime.Common;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Player Username")]
    [Category("Fusion/Player Username")] 

    [Image(typeof(IconString), ColorTheme.Type.Green)]
    [Description("Set player username.")]

    [Serializable]
    public class SetStringPlayerUsername : PropertyTypeSetString
    {
        public override void Set(string value, Args args)
        {
            NetworkManager.ConnectionArgs.UserName = value;

            if(!NetworkManager.IsConnected) return;
            if (!NetworkPlayer.LocalPlayer) return;
            NetworkPlayer.LocalPlayer.SetUsername(value);
        }

        public override string Get(Args args)
        {
            var username = NetworkManager.ConnectionArgs.UserName;
            if(NetworkManager.IsConnected && NetworkPlayer.LocalPlayer) username = NetworkPlayer.LocalPlayer.Username.Value;
            return username;
        }

        public override string String => $"Player Username";
    }
}