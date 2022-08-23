using MvkServer.Util;
using MvkServer.World.Block;

namespace MvkServer.World.Gen.Feature
{
    public class WorldGenCactus : WorldGenerator
    {
        public override bool Generate(WorldBase world, Rand rand, BlockPos blockPos)
        {
            if (rand.Next(20) == 0)
            {
                while (true)
                {
                    EnumBlock enumBlock = world.GetBlockState(blockPos).GetEBlock();
                    if (enumBlock != EnumBlock.Cactus) break;
                    blockPos = blockPos.OffsetUp();
                }
                int h = rand.Next(5) + 1;
                for (int i = 0; i < h; i++)
                {
                    if (Blocks.GetBlockCache(40).CanBlockStay(world, blockPos))
                    {
                        world.SetBlockState(blockPos, new BlockState(40), 0);
                        blockPos = blockPos.OffsetUp();
                    }
                    else
                    {
                        break;
                    }
                }
                return true;
            }
            return true;
        }
    }
}
