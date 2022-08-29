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
            Decorator.oakPerChunk = 1;
            Decorator.birchPerChunk = 1;
            Decorator.sprucePerChunk = 1;
            Decorator.fruitPerChunk = 1;
            Decorator.randomTree = 2;

            Decorator.grassPerChunk = 16;
            Decorator.flowersPerChunk = 1;
        }
    }
}
