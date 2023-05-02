using MvkServer.World.Block;
using MvkServer.World.Gen;

namespace MvkServer.World.Biome
{
    /// <summary>
    /// Абстрактный объект биома, для травяных биомов
    /// </summary>
    public abstract class BiomeAbGrass : BiomeBase
    {
        public BiomeAbGrass(ChunkProviderGenerateBase chunkProvider) : base(chunkProvider)
        {
            blockIdBiomDebug = blockIdUp = (ushort)EnumBlock.Turf;
            blockIdBody = (ushort)EnumBlock.Dirt;
            isBlockBody = true;
        }
    }
}

