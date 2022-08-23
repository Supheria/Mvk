using MvkServer.World.Gen;

namespace MvkServer.World.Biome
{
    /// <summary>
    /// Биом Сешанный лес
    /// </summary>
    public class BiomeMixedForest : BiomeBase
    {
        public BiomeMixedForest(ChunkProviderGenerate chunkProvider) : base(chunkProvider)
        {
            Decorator.grassPerChunk = 10;
            Decorator.flowersPerChunk = 128;
        }
    }
}
