using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Block;
using MvkServer.World.Chunk;

namespace MvkServer.World.Gen.Feature
{
    /// <summary>
    /// Генерация пальмы
    /// </summary>
    public class WorldGenTreePalm : WorldGenTree
    {
        public WorldGenTreePalm()
        {
            log = EnumBlock.LogPalm;
            leaves = EnumBlock.LeavesPalm;
            idLog = (ushort)log;
            idLeaves = (ushort)leaves;
            blockPut = EnumBlock.Sand;
        }

        protected override bool IsPut(EnumBlock enumBlock) 
            => enumBlock == EnumBlock.Air || enumBlock == EnumBlock.Cactus;

        protected override int Radius(int y0) => crownWidth;

        protected override void RandSize(Rand rand)
        {
            offsetCrown = MvkStatic.AreaOne4[rand.Next(4)] * rand.Next(4);
            // высота ствола дерева до кроны
            trunkHeight = rand.Next(7) + 7;
            // ширина кроны
            crownWidth = rand.Next(3) + 2;
            // высота кроны
            crownHeight = rand.Next(2) + 1;
            // от ствола до макушки
            crownHeightUp = 1;
        }

        /// <summary>
        /// Ствол
        /// </summary>
        protected override void Trunk(vec2i posCh, BlockPos blockPos, ChunkBase chunk, int count)
        {
            int bx;
            int by = blockPos.Y;
            int bz;
            int index, y, bx2, bz2;
            ChunkStorage chunkStorage;

            int count2 = count - 1;
            for (int i = 0; i < count; i++)
            {
                y = by + i;
                index = y >> 4;
                if (index >= 0 && index < ChunkBase.COUNT_HEIGHT)
                {
                    bx = blockPos.X + offsetCrown.x * i / count2;
                    bz = blockPos.Z + offsetCrown.y * i / count2;

                    if (posCh.x == bx >> 4 && posCh.y == bz >> 4)
                    {
                        bx2 = bx & 15;
                        bz2 = bz & 15;
                        if (bx2 >> 4 == 0 && bz2 >> 4 == 0)
                        {
                            chunkStorage = chunk.StorageArrays[index];
                            index = (y & 15) << 8 | bz2 << 4 | bx2;
                            chunkStorage.SetData(index, idLog, (ushort)(i == 0 ? 6 : 0));
                        }
                    }
                }
            }
        }
    }
}
