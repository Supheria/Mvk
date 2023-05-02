using MvkServer.World.Block;
using MvkServer.World.Gen;

namespace MvkServer.World.Biome
{
    /// <summary>
    /// Биом тропико
    /// </summary>
    public class BiomeTropics : BiomeBase
    {
        public BiomeTropics(ChunkProviderGenerateBase chunkProvider) : base(chunkProvider)
        {
            blockIdBody = blockIdUp = (ushort)EnumBlock.Sand;
            blockIdBiomDebug = (ushort)EnumBlock.Terracotta;
            isBlockBody = true;
        }

        /// <summary>
        /// Инициализировать декорацию
        /// </summary>
        public override void InitDecorator(bool isRobinson)
        {
            base.InitDecorator(isRobinson);
            Decorator.cactiPerChunk = 1;
            Decorator.palmPerChunk = 1;
        }
    }
}
