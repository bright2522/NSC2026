using System;
using GameCreator.Runtime.Common;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Generated Session Code")]
    [Category("Fusion/Generated Session Code")]

    [Image(typeof(IconCode), ColorTheme.Type.Blue)]
    [Description("Returns a generated human readable random code to be shared with other players.")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetStringSessionCode : PropertyTypeGetString
    {
        public override string Get(Args args)
        {
            var codeGenerator = FusionRepository.Get.SessionCodeGenerator;
            var selectedRegion = string.IsNullOrEmpty(NetworkManager.ConnectionArgs.SelectedRegion) ? string.Empty : NetworkManager.ConnectionArgs.SelectedRegion;
            var code = string.IsNullOrEmpty(selectedRegion)
                ? codeGenerator.Create()
                : codeGenerator.EncodeRegion(codeGenerator.Create(), NetworkManager.ConnectionArgs.SelectedRegionIndex);
            return code;
        }               
        public static PropertyGetString Create() => new(new GetStringSessionCode());
        public override string String => $"Generated Session Code";
    }
}