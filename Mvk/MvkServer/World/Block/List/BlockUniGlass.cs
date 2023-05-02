using MvkServer.Glm;
using MvkServer.Sound;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Универсальный объект стекляных блоков
    /// </summary>
    public class BlockUniGlass : BlockBase
    {
        public BlockUniGlass(int numberTexture, vec3 color, bool translucent = true)
        {
            Translucent = translucent;
            LightOpacity = (byte)(translucent ? 2 : 0);
            АmbientOcclusion = false;
            BlocksNotSame = false;
            Color = color;
            AllSideForcibly = true;
            UseNeighborBrightness = true;
            Particle = numberTexture;
            Resistance = .6f;
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
