using System;
using GameCreator.Runtime.Common;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Error Message")]
    [Category("Fusion/Reasons/Error Message")]

    [Image(typeof(IconString), ColorTheme.Type.Red)]
    [Description("Reference to the last error message if there as any.")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetStringLastErrorMessage : PropertyTypeGetString
    {
        public override string Get(Args args)
        {
            return NetworkManager.LastErrorMessage;
        }
        public override string String => $"Last Error Message";
    }
}