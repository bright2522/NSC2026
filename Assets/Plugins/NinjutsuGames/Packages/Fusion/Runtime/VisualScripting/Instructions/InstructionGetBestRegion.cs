using System;
using System.Linq;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Select Best Region")]
    [Description("Selects the best region based on ping and availability")]
    [Category("Fusion/Select Best Region")]
    [Parameter("Set Region", "Stores the best region name")]
    [Parameter("Set Ping", "Stores the best region ping")]
    [Image(typeof(IconSphereOutline), ColorTheme.Type.Green, typeof(OverlayTick))]
    [Keywords("Region", "Game", "Best", "Fusion")]
    [Serializable]
    public class InstructionGetBestRegion : Instruction
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private PropertySetString setRegion = SetStringNone.Create;
        [SerializeField] private PropertySetString setPing = SetStringNone.Create;

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Select Best Region";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override async Task Run(Args args)
        {
            var regionTask = NetworkManager.PingRegions();
            await regionTask;

            if (!regionTask.IsCompletedSuccessfully) return;

            var regions = regionTask.Result;
            var availableRegions = FusionRepository.Get.Regions.RegionList.GetAvailable();

            // Get the best region based on ping and if it is available
            var bestRegion = regions
                .Where(region => availableRegions.Any(r => r.Token == region.RegionCode))
                .OrderBy(region => region.RegionPing)
                .FirstOrDefault();

            var regionIndex = Array.FindIndex(availableRegions, region => region.Token == bestRegion.RegionCode);
            NetworkManager.Instance.SetRegion(bestRegion.RegionCode, regionIndex);
            setRegion.Set(bestRegion.RegionCode, args);
            setPing.Set(bestRegion.RegionPing.ToString(), args);
        }
    }
}