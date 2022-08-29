using MvkServer.World.Block;
using MvkServer.World.Gen;

namespace MvkServer.World.Biome
{
    /// <summary>
    /// Биом моря
    /// </summary>
    public class BiomeSea : BiomeBase
    {
        public BiomeSea(ChunkProviderGenerate chunkProvider) : base(chunkProvider)
        {
            Decorator.sandPancakePerChunk = 2;
            Decorator.dirtPancakePerChunk = 1;
            blockIdBody = blockIdUp = (ushort)EnumBlock.Gravel;
        }

        /// <summary>
        /// Получить уровень высоты
        /// </summary>
        /// <param name="height">Высота -1..0..1</param>
        /// <param name="river">Определение центра реки 1..0..1</param>
        protected override int GetLevelHeight(int x, int z, float height, float river) => 95 + (int)(height * 384f);
    }
}
