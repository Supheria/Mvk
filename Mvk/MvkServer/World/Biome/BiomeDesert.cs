using MvkServer.World.Block;
using MvkServer.World.Gen;

namespace MvkServer.World.Biome
{
    /// <summary>
    /// Биом пляж
    /// </summary>
    public class BiomeDesert : BiomeBase
    {
        public BiomeDesert(ChunkProviderGenerate chunkProvider) : base(chunkProvider)
        {
            Decorator.cactiPerChunk = 10;
            blockIdBody = blockIdUp = (ushort)EnumBlock.Sand;
        }
    }
}
