using MvkServer.World.Block;
using MvkServer.World.Gen;

namespace MvkServer.World.Biome
{
    /// <summary>
    /// Биом Горы в пустыне
    /// </summary>
    public class BiomeMountainsDesert : BiomeMountains
    {
        public BiomeMountainsDesert(ChunkProviderGenerateBase chunkProvider) : base(chunkProvider)
        {
            blockIdBiomDebug = (ushort)EnumBlock.Granite;
            blockIdBody2 = (ushort)EnumBlock.Sandstone;
            maxLevel = 72;
            kofAddNoise = 1f;
        }

        protected override int BodyHeight(float area, int yh) => HEIGHT_MOUNTAINS_DESERT - (int)(area / 4f + 5f);

        /// <summary>
        /// Инициализировать декорацию
        /// </summary>
        public override void InitDecorator(bool isRobinson)
        {
            base.InitDecorator(isRobinson);
            Decorator.limestonePerChunk = 10;
            Decorator.cactiPerChunk = 1;
            Decorator.oakPerChunk = 0;
            Decorator.randomTree = 1;
            Decorator.brolPerChunk = 0;
            Decorator.piecePerChunk = 0;
            blockIdBody = blockIdUp = isRobinson ? blockIdBody2 : (ushort)EnumBlock.Sand;
        }
    }
}
