using MvkServer.World.Block;
using MvkServer.World.Gen;

namespace MvkServer.World.Biome
{
    /// <summary>
    /// Биом пляжа
    /// </summary>
    public class BiomeBeach : BiomeBase
    {
        public BiomeBeach(ChunkProviderGenerate chunkProvider) : base(chunkProvider)
        {
            blockIdBody = blockIdUp = (ushort)EnumBlock.Sand;
        }

        /// <summary>
        /// Получить уровень высоты
        /// </summary>
        /// <param name="height">Высота -1..0..1</param>
        /// <param name="river">Определение центра реки 1..0..1</param>
        protected override int GetLevelHeight(int x, int z, float height, float river) => 95 + (int)(height * 288f);
    }
}
