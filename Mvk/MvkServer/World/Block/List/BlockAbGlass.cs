using MvkServer.Glm;
using MvkServer.Sound;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Абстрактный объект стекляных блоков
    /// </summary>
    public abstract class BlockAbGlass : BlockBase
    {
        public BlockAbGlass(int numberTexture, vec3 color)
        {
            Translucent = true;
            АmbientOcclusion = false;
            Color = color;
            AllSideForcibly = true;
            UseNeighborBrightness = true;
            Particle = numberTexture;
            LightOpacity = 2;
            Material = EnumMaterial.Glass;
            samplesBreak = new AssetsSample[] { AssetsSample.DigGlass1, AssetsSample.DigGlass2, AssetsSample.DigGlass3 };
            InitBoxs(numberTexture, true, color);
        }

        /// <summary>
        /// Сколько ударов требуется, чтобы сломать блок в тактах (20 тактов = 1 секунда)
        /// </summary>
        public override int Hardness(BlockState state) => 10;

        /// <summary>
        /// Спавн предмета при разрушении этого блока
        /// </summary>
        public override void DropBlockAsItemWithChance(WorldBase worldIn, BlockPos blockPos, BlockState state, float chance, int fortune) { }

        /// <summary>
        /// Получите предмет, который должен выпасть из этого блока при сборе.
        /// </summary>
        //public override ItemBase GetItemDropped(BlockState state, Random rand, int fortune) 
        //    => new ItemBlock(Blocks.GetBlockCache(EnumBlock.Glass));
    }
}
