using MvkServer.World.Block;
using MvkServer.World.Gen;

namespace MvkServer.World.Biome
{
    /// <summary>
    /// Биом Горы в пустыне
    /// </summary>
    public class BiomeMountainsDesert : BiomeMountains
    {
        public BiomeMountainsDesert(ChunkProviderGenerate chunkProvider) : base(chunkProvider)
        {
            Decorator.limestonePerChunk = 10;
            Decorator.cactiPerChunk = 1;
            Decorator.oakPerChunk = 0;
            Decorator.randomTree = 1;
            Decorator.brolPerChunk = 0;
            blockIdBody2 = (ushort)EnumBlock.Sandstone;
            blockIdBody = blockIdUp = (ushort)EnumBlock.Sand;
        }

        protected override int BodyHeight(float area, int yh) => 108 - (int)(area / 4f + 5f);
    }
}
