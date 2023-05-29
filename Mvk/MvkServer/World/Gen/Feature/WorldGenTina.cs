using MvkServer.Util;
using MvkServer.World.Block;
using MvkServer.World.Chunk;
namespace MvkServer.World.Gen.Feature
{
    /// <summary>
    /// Генерация тины
    /// </summary>
    public class WorldGenTina : WorldGenerator
    {
        public override bool Generate(WorldBase world, Rand rand, BlockPos blockPos)
        {
            if (world.GetBlockState(blockPos).IsAir() 
                && world.GetBlockState(blockPos.OffsetDown()).GetEBlock() == EnumBlock.Water)
            {
                ChunkBase chunk = world.GetChunk(blockPos);
                chunk.StorageArrays[blockPos.Y >> 4].SetData(
                    (blockPos.Y & 15) << 8 | (blockPos.Z & 15) << 4 | (blockPos.X & 15), 72
                );
                return true;
            }

            return false;
        }
    }
}
