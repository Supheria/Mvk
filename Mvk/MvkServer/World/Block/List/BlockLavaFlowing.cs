using MvkServer.Sound;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок тикучей лавы
    /// </summary>
    public class BlockLavaFlowing : BlockAbLiquidFlowing
    {
        /// <summary>
        /// Блок тикучей лавы
        /// </summary>
        public BlockLavaFlowing() : base()
        {
            material = EnumMaterial.Lava;
            eBlock = EnumBlock.Lava;
            eBlockFlowing = EnumBlock.LavaFlowing;
            tickRate = 30;
            stepWave = 4;
            LightOpacity = 0;

            // Затычка, для сортировки, и прорисовки из нутри когда к примеру блок стекла
            // Translucent = true; 
            NeedsRandomTick = true;
            LightValue = 15;
            Material = EnumMaterial.Lava;
            samplesStep = new AssetsSample[0];
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
