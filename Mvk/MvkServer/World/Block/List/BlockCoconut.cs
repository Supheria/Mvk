using MvkServer.Glm;
using MvkServer.Item;
using MvkServer.Item.List;
using MvkServer.Sound;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок кокоса
    /// </summary>
    public class BlockCoconut : BlockBase
    {
        /// <summary>
        /// Блок кокоса
        /// </summary>
        public BlockCoconut()
        {
            Resistance = .2f;
            Particle = 164;
            Combustibility = true;
            IgniteOddsSunbathing = 5;
            BurnOdds = 10;
            Material = Materials.GetMaterialCache(EnumMaterial.VegetableProtein);
            samplesPut = samplesBreak = new AssetsSample[] { AssetsSample.DigWood1, AssetsSample.DigWood2, AssetsSample.DigWood3, AssetsSample.DigWood4 };
            samplesStep = new AssetsSample[] { AssetsSample.StepWood1, AssetsSample.StepWood2, AssetsSample.StepWood3, AssetsSample.StepWood4 };
            InitQuads(Particle);
        }

        /// <summary>
        /// Сколько ударов требуется, чтобы сломать блок в тактах (20 тактов = 1 секунда)
        /// </summary>
        public override int Hardness(BlockState state) => 15;

        /// <summary>
        /// Получите предмет, который должен выпасть из этого блока при сборе.
        /// </summary>
        protected override ItemBase GetItemDropped(BlockState state, Rand rand, ItemAbTool itemTool) 
            => Items.GetItemCache(EnumItem.Coconut);

        /// <summary>
        /// Смена соседнего блока
        /// </summary>
        public override void NeighborBlockChange(WorldBase worldIn, BlockPos blockPos, BlockState neighborState, BlockBase neighborBlock)
        {
            if (worldIn.GetBlockState(blockPos.OffsetUp()).GetEBlock() != EnumBlock.LeavesPalm)
            {
                if (worldIn.Rnd.Next(2) == 0)
                {
                    // 50% шанса, что дропнется кокос при разрушении листвы
                    DropBlockAsItem(worldIn, blockPos, neighborState);
                }
                worldIn.SetBlockToAir(blockPos, 15);
            }
        }
    }
}
