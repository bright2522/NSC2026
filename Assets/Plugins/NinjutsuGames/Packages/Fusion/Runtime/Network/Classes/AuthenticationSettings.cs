using System;
using Fusion.Photon.Realtime;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Serializable]
    public class AuthenticationSettings
    {
        public CustomAuthenticationType authType = CustomAuthenticationType.None;
        public StringList values = new();

        public AuthenticationValues AuthValues
        {
            get
            {
                var authentication = new AuthenticationValues
                {
                    AuthType = authType
                };
                foreach (StringItem value in values)
                {
                    if (string.IsNullOrEmpty(value.GetValue()))
                    {
                        Debug.LogWarning($"[AuthenticationSettings] AuthValues: {value.Name} is empty");
                        continue;
                    }
                    authentication.AddAuthParameter(value.Name, value.GetValue());
                }
                return authentication;
            }
        }
    }
}