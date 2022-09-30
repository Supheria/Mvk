using MvkServer.Util;
using MvkServer.World.Block;

namespace MvkServer.World.Gen.Feature
{
    /// <summary>
    /// Генерация дерева берёзы
    /// </summary>
    public class WorldGenTreeBirch : WorldGenTree
    {
        public WorldGenTreeBirch()
        {
            log = EnumBlock.LogBirch;
            leaves = EnumBlock.LeavesBirch;
            sapling = EnumBlock.SaplingBirch;
            idLog = (ushort)log;
            idLeaves = (ushort)leaves;
            idSapling = (ushort)sapling;
        }

        protected override int Radius(int y0)
        {
            if (y0 == 0) return crownWidth - 1;
            if (y0 == crownHeight - 1) return 0;
            if (y0 == crownHeight - 2) return 1;
            if (y0 == crownHeight - 3) return 2;
            return crownWidth;
        }

        protected override void RandSize(Rand rand)
        {
            // высота ствола дерева до кроны
            trunkHeight = rand.Next(3) + 2;
            // ширина кроны
            crownWidth = rand.Next(10) == 0 ? 3 : 2;
            // высота кроны
            crownHeight = rand.Next(3) + 9;
        }
    }
}
