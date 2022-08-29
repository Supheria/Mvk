using MvkServer.World.Gen;

namespace MvkServer.World.Biome
{
    /// <summary>
    /// Биом ровнины
    /// </summary>
    public class BiomePlain : BiomeBase
    {
        public BiomePlain(ChunkProviderGenerate chunkProvider) : base(chunkProvider)
        {
            Decorator.grassPerChunk = 24;
            Decorator.flowersPerChunk = 3;
            Decorator.fruitPerChunk = 1;
            Decorator.oakPerChunk = 1;
            Decorator.randomTree = 32;
        }
    }
}
