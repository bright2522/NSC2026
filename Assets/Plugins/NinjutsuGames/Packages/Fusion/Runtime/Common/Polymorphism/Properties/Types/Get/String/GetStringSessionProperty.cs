using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Session Property")]
    [Category("Fusion/Session/Session Property")]

    [Image(typeof(IconCode), ColorTheme.Type.Green)]
    [Description("Returns the value of a session property in the current session")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetStringSessionProperty : PropertyTypeGetString
    {
        [SerializeField] private PropertyGetString propertyName = GetStringString.Create;
        
        public override string Get(Args args)
        {
            if(NetworkManager.IsConnected)
            {
                return NetworkManager.Runner.SessionInfo.Properties.TryGetValue(propertyName.Get(args.Target), out var value) ? value : string.Empty;
            }

            var sessionItem = args.Target.Get<SessionItemUI>();
            var result = string.Empty;
            if (!sessionItem) return result;
            
            sessionItem.SessionInfo.Properties.TryGetValue(propertyName.Get(args.Target), out var sessionProperty);
            if (sessionProperty != null) result = sessionProperty.PropertyValue.ToString();
            
            return result;
        }
        public override string String => $"Session Property";
    }
}