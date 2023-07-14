using MvkServer.Sound;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок стоячей лавы
    /// </summary>
    public class BlockLava : BlockAbLiquid
    {
        /// <summary>
        /// Блок стоячей лавы
        /// </summary>
        public BlockLava() : base()
        {
            materialLiquid = EnumMaterial.Lava;
            eBlock = EnumBlock.Lava;
            eBlockFlowing = EnumBlock.LavaFlowing;
            tickRate = 30;
            stepWave = 4;
            Particle = 60;

            NeedsRandomTick = true;
            // Затычка, для сортировки, и прорисовки из нутри когда к примеру блок стекла
            // Translucent = true; 
            // LightOpacity = 4;
            LightValue = 15;
            Material = Materials.GetMaterialCache(EnumMaterial.Lava);
            samplesStep = new AssetsSample[0];
            samplesBreak = new AssetsSample[] { AssetsSample.BucketEmptyLava1, AssetsSample.BucketEmptyLava2, AssetsSample.BucketEmptyLava3 };
            samplesPut = new AssetsSample[] { AssetsSample.BucketFillLava1, AssetsSample.BucketFillLava2, AssetsSample.BucketFillLava3 };
            sideLiquids = new SideLiquid[]
            {
                new SideLiquid(0, 60, 0, 32, 4),
                new SideLiquid(1, 60, 0, 32, 4),
                new SideLiquid(2, 59, 0, 64, 1),
                new SideLiquid(3, 59, 0, 64, 1),
                new SideLiquid(4, 59, 0, 64, 1),
                new SideLiquid(5, 59, 0, 64, 1)
            };
        }

        /// <summary>
        /// Случайный эффект блока, для сервера
        /// </summary>
        public override void RandomTick(WorldBase world, BlockPos blockPos, BlockState blockState, Rand random) 
            => SetFireTo(world, blockPos, random);
    }
}
