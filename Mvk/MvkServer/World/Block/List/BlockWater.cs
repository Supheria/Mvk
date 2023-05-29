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
            Material = EnumMaterial.Water;
            samplesBreak = new AssetsSample[] { AssetsSample.BucketEmptyWater1, AssetsSample.BucketEmptyWater2, AssetsSample.BucketEmptyWater3 };
            samplesPut = new AssetsSample[] { AssetsSample.BucketFillWater1, AssetsSample.BucketFillWater2, AssetsSample.BucketFillWater3 };
            faces = new Face[]
            {
                new Face(63, true).SetAnimation(32, 2).SetBiomeColor(),
                new Face(62, true).SetAnimation(64, 1).SetBiomeColor()
            };
        }
    }
}
