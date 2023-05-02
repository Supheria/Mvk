using MvkServer.World.Block;
using MvkServer.World.Gen;

namespace MvkServer.World.Biome
{
    /// <summary>
    /// Биом пляжа
    /// </summary>
    public class BiomeBeach : BiomeBase
    {
        public BiomeBeach(ChunkProviderGenerateBase chunkProvider) : base(chunkProvider)
        {
            blockIdBiomDebug = blockIdBody = blockIdUp = (ushort)EnumBlock.Sand;
            isBlockBody = true;
        }

        /// <summary>
        /// Получить уровень высоты
        /// </summary>
        /// <param name="height">Высота -1..0..1</param>
        /// <param name="river">Определение центра реки 1..0..1</param>
        protected override int GetLevelHeight(int x, int z, float height, float river) 
            => HEIGHT_WATER_MINUS + (int)(height * HEIGHT_HILL_BEACH);

        /// <summary>
        /// Получить уровень множителя высоты
        /// </summary>
        /// <param name="x">X 0..15</param>
        /// <param name="z">Z 0..15</param>
        /// <param name="height">Высота в блоках, средняя рекомендуемая</param>
        /// <param name="heightNoise">Высота -1..0..1</param>
        /// <param name="addNoise">Диапазон -1..0..1</param>
        protected override int GetLevelHeightRobinson(int x, int z, int height, float heightNoise, float addNoise)
            => height + (int)(heightNoise * (heightNoise < 0 ? .5f : 6f));


    }
}
