using GameCreator.Runtime.Common;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    public class FusionSettings : AssetRepository<FusionRepository>
    {
        public override IIcon Icon => new IconPhysics(ColorTheme.Type.TextLight);
        public override string Name => "Fusion";
    }
}