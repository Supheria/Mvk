using MvkServer.Util;
using MvkServer.World.Block;

namespace MvkServer.World.Gen.Feature
{
    /// <summary>
    /// Генерация дуба дерева
    /// </summary>
    public class WorldGenTreeOak : WorldGenTree
    {
        public WorldGenTreeOak()
        {
            log = EnumBlock.LogOak;
            leaves = EnumBlock.LeavesOak;
            idLog = (ushort)log;
            idLeaves = (ushort)leaves;
            isBranches = true;
        }

        protected override int Radius(int y0)
        {
            if (y0 == 0) return 1;
            if (y0 == 1) return 3;
            if (y0 == crownHeight - 1) return 2;
            if (y0 == crownHeight - 2) return 3;
            if (y0 == crownHeight - 3) return 4;
            return crownWidth;
        }

        protected override void RandSize(Rand rand)
        {
            // высота ствола дерева до кроны
            trunkHeight = rand.Next(3) + 2;
            // ширина кроны
            crownWidth = rand.Next(2) + 4;
            // высота кроны
            crownHeight = rand.Next(3) + 10;
        }
    }
}
