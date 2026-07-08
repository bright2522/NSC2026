using System;
using GameCreator.Runtime.Common;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Serializable]
    public class LobbyAdvancedSettings
    {
        public PropertyGetBool useDefaultCloudPorts = GetBoolFalse.Create;
        public PropertyGetString CustomAppVersion = GetStringEmpty.Create;
    }
}