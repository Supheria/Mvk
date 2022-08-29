using MvkServer.World.Gen;

namespace MvkServer.World.Biome
{
    /// <summary>
    /// Биом Хвойный лес
    /// </summary>
    public class BiomeConiferousForest : BiomeBase
    {
        public BiomeConiferousForest(ChunkProviderGenerate chunkProvider) : base(chunkProvider)
        {
            Decorator.sprucePerChunk = 4;
            Decorator.grassPerChunk = 12;
        }
    }
}
