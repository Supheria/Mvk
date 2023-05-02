using MvkServer.World.Block;
using MvkServer.World.Gen;

namespace MvkServer.World.Biome
{
    /// <summary>
    /// Биом пляж
    /// </summary>
    public class BiomeDesert : BiomeBase
    {
        public BiomeDesert(ChunkProviderGenerateBase chunkProvider) : base(chunkProvider)
        {
            blockIdBiomDebug = blockIdBody = blockIdUp = (ushort)EnumBlock.Sand;
            isBlockBody = true;
        }

        /// <summary>
        /// Инициализировать декорацию
        /// </summary>
        public override void InitDecorator(bool isRobinson)
        {
            base.InitDecorator(isRobinson);
            Decorator.cactiPerChunk = 10;
        }
    }
}
