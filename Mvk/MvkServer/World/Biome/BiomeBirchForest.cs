using MvkServer.World.Gen;

namespace MvkServer.World.Biome
{
    /// <summary>
    /// Биом Берёзовый лес
    /// </summary>
    public class BiomeBirchForest : BiomeBase
    {
        public BiomeBirchForest(ChunkProviderGenerate chunkProvider) : base(chunkProvider)
        {
            Decorator.birchPerChunk = 4;
            Decorator.grassPerChunk = 8;
            Decorator.flowersPerChunk = 5;
        }
    }
}
