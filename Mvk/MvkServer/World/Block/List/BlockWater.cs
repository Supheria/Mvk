using MvkServer.Sound;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок стоячей воды
    /// </summary>
    public class BlockWater : BlockAbLiquid
    {
        /// <summary>
        /// Блок стоячей воды
        /// </summary>
        public BlockWater() : base()
        {
            Translucent = true;
            BiomeColor = true;
            LightOpacity = 1;
            Material = Materials.GetMaterialCache(EnumMaterial.Water);
            samplesBreak = new AssetsSample[] { AssetsSample.BucketEmptyWater1, AssetsSample.BucketEmptyWater2, AssetsSample.BucketEmptyWater3 };
            samplesPut = new AssetsSample[] { AssetsSample.BucketFillWater1, AssetsSample.BucketFillWater2, AssetsSample.BucketFillWater3 };

            sideLiquids = new SideLiquid[]
            {
                new SideLiquid(0, 63, 3, 32, 2),
                new SideLiquid(1, 63, 3, 32, 2),
                new SideLiquid(2, 62, 3, 64, 1),
                new SideLiquid(3, 62, 3, 64, 1),
                new SideLiquid(4, 62, 3, 64, 1),
                new SideLiquid(5, 62, 3, 64, 1)
            };
        }
    }
}
