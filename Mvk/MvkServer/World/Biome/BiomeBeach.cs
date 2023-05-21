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

    }
}
