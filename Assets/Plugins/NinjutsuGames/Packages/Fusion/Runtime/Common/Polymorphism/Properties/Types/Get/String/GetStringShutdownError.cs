using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Shutdown Reason Error")]
    [Category("Fusion/Reasons/Shutdown Reason Error")]

    [Image(typeof(IconShutdown), ColorTheme.Type.Red, typeof(OverlayDot))]
    [Description("Reference to the last shutdown reason.")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetStringShutdownError : PropertyTypeGetString
    {
        public enum Type
        {
            Title,
            Message,
        }

        [SerializeField] private Type display = Type.Title;
        [SerializeField] private PropertyGetString fallback = GetStringEmpty.Create;
        public override string Get(Args args)
        {
            var settings = Settings.From<FusionRepository>();
            var error = settings.ErrorMessages.ShutdownErrorList.Get(NetworkManager.LastShutdownReason);
            if (error == null) return string.Empty;
            var msg = display == Type.Title ? error.GetName(args) : error.GetMessage(args);
            return string.IsNullOrEmpty(msg) ? fallback.Get(args) : msg;
        }
        public override string String => $"Shutdown Reason {display}";
    }
}