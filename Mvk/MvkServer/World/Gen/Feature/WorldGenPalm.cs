using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Block;
using MvkServer.World.Chunk;

namespace MvkServer.World.Gen.Feature
{
    public class WorldGenPalm : WorldGenerator
    {
        public override bool GenerateArea(WorldBase world, ChunkBase chunk, Rand rand, BlockPos blockPos)
        {
            int h = rand.Next(5) + 4;
            int v = rand.Next(4);
            int h2 = rand.Next(5) + 4;
            EnumBlock enumBlock = world.GetBlockState(blockPos).GetEBlock();

            if ((enumBlock == EnumBlock.Air || enumBlock == EnumBlock.LogPalm)
                && world.GetBlockState(blockPos.OffsetDown()).GetEBlock() == EnumBlock.Sand)
            {
                vec2i posCh = chunk.Position;

                for (int i = 0; i < h; i++)
                {
                    SetBlock(world, blockPos, posCh, 24);
                    blockPos = blockPos.OffsetUp();
                }

                vec2i vec = MvkStatic.AreaOne4[v];
                for (int i = 0; i < h2; i++)
                {
                    blockPos = blockPos.Offset(vec.x, 0, vec.y);
                    SetBlock(world, blockPos, posCh, 34);
                }
                return true;
            }
            return false;
        }
    }
}
