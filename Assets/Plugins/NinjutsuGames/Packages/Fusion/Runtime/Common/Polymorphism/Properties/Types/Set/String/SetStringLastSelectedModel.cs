using System;
using GameCreator.Runtime.Common;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Selected Model")]
    [Category("Fusion/Selected Model")] 

    [Image(typeof(IconCharacter), ColorTheme.Type.Yellow)]
    [Description("Set selected model.")]

    [Serializable]
    public class SetStringLastSelectedModel : PropertyTypeSetString
    {
        public override void Set(string value, Args args)
        {
            NetworkManager.ConnectionArgs.SelectedModel = value;
        }

        public override string Get(Args args) => NetworkManager.ConnectionArgs.SelectedModel; 

        public override string String => $"Selected Model";
    }
}