using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Selected Region")]
    [Category("Fusion/Selected Region")]

    [Image(typeof(IconSphereOutline), ColorTheme.Type.Green)]
    [Description("Returns the selected region via UI dropdown menu.")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetStringSelectedRegion : PropertyTypeGetString
    {
        [SerializeField] private PropertyGetString fallbackTo = GetStringAvailableRegion.Create();
        public override string Get(Args args)
        {
            return string.IsNullOrEmpty(NetworkManager.ConnectionArgs.SelectedRegion) ? fallbackTo.Get(args) : NetworkManager.ConnectionArgs.SelectedRegion;
        }
        public static PropertyGetString Create => new(new GetStringSelectedRegion());
        public override string String => $"Selected Region";
    }
}