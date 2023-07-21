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
            IsDamagedBlockBlack = !translucent;
            LightOpacity = (byte)(translucent ? 2 : 0);
            АmbientOcclusion = false;
            BlocksNotSame = false;
            base.color = color;
            AllSideForcibly = true;
            UseNeighborBrightness = true;
            Particle = numberTexture;
            Resistance = .6f;
            canDropPresent = false;
            Material = Materials.GetMaterialCache(EnumMaterial.Glass);
            samplesBreak = new AssetsSample[] { AssetsSample.DigGlass1, AssetsSample.DigGlass2, AssetsSample.DigGlass3 };

            quads = new QuadSide[][] { new QuadSide[] {
                new QuadSide(4).SetTexture(numberTexture).SetSide(Pole.Up),
                new QuadSide(4).SetTexture(numberTexture).SetSide(Pole.Down),
                new QuadSide(4).SetTexture(numberTexture).SetSide(Pole.East),
                new QuadSide(4).SetTexture(numberTexture).SetSide(Pole.West),
                new QuadSide(4).SetTexture(numberTexture).SetSide(Pole.North),
                new QuadSide(4).SetTexture(numberTexture).SetSide(Pole.South)
            } };
        }

        /// <summary>
        /// Сколько ударов требуется, чтобы сломать блок в тактах (20 тактов = 1 секунда)
        /// </summary>
        public override int Hardness(BlockState state) => 10;

        /// <summary>
        /// Получите предмет, который должен выпасть из этого блока при сборе.
        /// </summary>
        //public override ItemBase GetItemDropped(BlockState state, Random rand, int fortune) 
        //    => new ItemBlock(Blocks.GetBlockCache(EnumBlock.Glass));
    }
}
