using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Available Region")]
    [Category("Fusion/Available Region")]

    [Image(typeof(IconSphereOutline), ColorTheme.Type.Blue)]
    [Description("Returns a region from the available regions list.")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetStringAvailableRegion : PropertyTypeGetString
    {
        [SerializeField] private RegionSelector region = new();
        public override string Get(Args args)
        {
            return region == null ? string.Empty : region.region;
        }
        public static PropertyGetString Create() => new(new GetStringAvailableRegion());
        public override string String => $"Session Region";
    }
}