using MvkServer.Util;
using MvkServer.World.Chunk;

namespace MvkServer.World.Gen.Feature
{
    /// <summary>
    /// Генерация брола
    /// </summary>
    public class WorldGenBrol : WorldGenerator
    {
        public override bool Generate(WorldBase world, Rand rand, BlockPos blockPos)
        {
            ChunkBase chunk = world.GetChunk(blockPos);
            chunk.StorageArrays[blockPos.Y >> 4].SetData(
                (blockPos.Y & 15) << 8 | (blockPos.Z & 15) << 4 | (blockPos.X & 15), 44
            );
            chunk.Light.SetLightBlock(blockPos.GetPosition0());

            return true;
        }
    }
}
