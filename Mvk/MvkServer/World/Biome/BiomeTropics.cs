using MvkServer.World.Block;
using MvkServer.World.Gen;

namespace MvkServer.World.Biome
{
    /// <summary>
    /// Биом тропико
    /// </summary>
    public class BiomeTropics : BiomeBase
    {
        public BiomeTropics(ChunkProviderGenerate chunkProvider) : base(chunkProvider)
        {
            Decorator.cactiPerChunk = 1;
            Decorator.palmPerChunk = 1;
            blockIdBody = blockIdUp = (ushort)EnumBlock.Sand;
        }
    }
}
