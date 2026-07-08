using System;
using GameCreator.Runtime.Common;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Selected Model")]
    [Category("Fusion/Models/Selected Model")]

    [Image(typeof(IconCharacter), ColorTheme.Type.Yellow)]
    [Description("Returns the last selected model.")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetStringLastSelectedModel : PropertyTypeGetString
    {
        public override string Get(Args args)
        {
            return NetworkManager.ConnectionArgs.SelectedModel;
        }
        public static PropertyGetString Create => new(new GetStringLastSelectedModel());

        public override string String => $"Selected Model";
    }
}