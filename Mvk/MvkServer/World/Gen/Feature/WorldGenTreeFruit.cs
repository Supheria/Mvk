using MvkServer.Util;
using MvkServer.World.Block;

namespace MvkServer.World.Gen.Feature
{
    /// <summary>
    /// Генерация фруктового дерева
    /// </summary>
    public class WorldGenTreeFruit : WorldGenTree
    {
        public WorldGenTreeFruit()
        {
            log = EnumBlock.LogFruit;
            leaves = EnumBlock.LeavesFruit;
            idLog = (ushort)log;
            idLeaves = (ushort)leaves;
        }

        protected override int Radius(int y0)
        {
            if (y0 == 0) return crownWidth - 1;
            if (y0 == crownHeight - 1) return 1;
            if (y0 == crownHeight - 2) return 2;
            return crownWidth;
        }

        protected override void RandSize(Rand rand)
        {
            // высота ствола дерева до кроны
            trunkHeight = rand.Next(3) + 2;
            // ширина кроны
            crownWidth = 3;// rand.Next(10) == 0 ? 3 : 2;
            // высота кроны
            crownHeight = rand.Next(3) + 8;
        }
    }
}
