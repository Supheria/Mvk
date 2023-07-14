using MvkServer.Sound;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок стоячей нефти
    /// </summary>
    public class BlockOil : BlockAbLiquid
    {
        /// <summary>
        /// Блок стоячей нефти
        /// </summary>
        public BlockOil() : base()
        {
            materialLiquid = EnumMaterial.Oil;
            eBlock = EnumBlock.Oil;
            eBlockFlowing = EnumBlock.OilFlowing;
            tickRate = 15;
            stepWave = 3;

            // Затычка, для сортировки, и прорисовки из нутри когда к примеру блок стекла
            //Translucent = true;
            Combustibility = true;
            IgniteOddsSunbathing = 100;
            BurnOdds = 100;
            LightOpacity = 1;
            Material = Materials.GetMaterialCache(EnumMaterial.Oil);
            samplesStep = new AssetsSample[0];
            samplesBreak = new AssetsSample[] { AssetsSample.BucketEmptyLava1, AssetsSample.BucketEmptyLava2, AssetsSample.BucketEmptyLava3 };
            samplesPut = new AssetsSample[] { AssetsSample.BucketFillLava1, AssetsSample.BucketFillLava2, AssetsSample.BucketFillLava3 };
            sideLiquids = new SideLiquid[]
            {
                new SideLiquid(0, 57, 0, 32, 8),
                new SideLiquid(1, 57, 0, 32, 8),
                new SideLiquid(2, 56, 0, 64, 2),
                new SideLiquid(3, 56, 0, 64, 2),
                new SideLiquid(4, 56, 0, 64, 2),
                new SideLiquid(5, 56, 0, 64, 2)
            };
        }

        /// <summary>
        /// Статус для растекания на огонь
        /// </summary>
        protected override bool IsFire(EnumMaterial eMaterial) => false;

        /// <summary>
        /// Обновить блок в такте
        /// </summary>
        public override void UpdateTick(WorldBase world, BlockPos blockPos, BlockState blockState, Rand random)
        {
            BlockPos blockPosUp = blockPos.OffsetUp();
            if (world.GetBlockState(blockPosUp).GetEBlock() == EnumBlock.Water)
            {
                // блок воды сверху, значит меняем их местами
                world.SetBlockState(blockPosUp, new BlockState(EnumBlock.Oil), 14);
                world.SetBlockState(blockPos, new BlockState(EnumBlock.Water), 14);
            }
            else
            {
                base.UpdateTick(world, blockPos, blockState, random);
            }
        }
    }
}
