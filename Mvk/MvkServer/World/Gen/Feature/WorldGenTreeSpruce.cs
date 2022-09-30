using MvkServer.Util;
using MvkServer.World.Block;

namespace MvkServer.World.Gen.Feature
{
    /// <summary>
    /// Генерация ель дерева
    /// </summary>
    public class WorldGenTreeSpruce : WorldGenTree
    {
        /// <summary>
        /// сосна
        /// </summary>
        private bool pine = true;

        public WorldGenTreeSpruce()
        {
            log = EnumBlock.LogSpruce;
            leaves = EnumBlock.LeavesSpruce;
            sapling = EnumBlock.SaplingSpruce;
            idLog = (ushort)log;
            idLeaves = (ushort)leaves;
            idSapling = (ushort)sapling;
            isBranches = true;
        }

        protected override int Radius(int y0)
        {
            if (pine)
            {
                if (y0 == 0) return 1;
                if (y0 == 1) return 3;
                if (y0 == crownHeight - 1) return 2;
                return crownWidth;
            }
            if (y0 < 3) return 4;
            if (y0 < 5) return 3;
            if (y0 >= crownHeight - 2) return 0;
            if (y0 >= crownHeight - 4) return 1;
            return 2;
        }

        protected override void RandSize(Rand rand)
        {
            // определение сосны
            pine = rand.Next(4) != 0;
            // высота ствола дерева до кроны
            trunkHeight = pine ? rand.Next(7) + 7 : rand.Next(3) + 1;
            // ширина кроны
            crownWidth = pine ? rand.Next(5) == 0 ? 5 : 4 : 4;
            // высота кроны
            crownHeight = pine ? rand.Next(3) + 4 : rand.Next(3) + 11;
            // от ствола до макушки
            crownHeightUp = pine ? 2 : 4;
            // Вверх макушка неприкасаемая для отсутствие листвы, примел ёлка
            crownHeightUntouchable = pine ? 0 : 2;
        }
    }
}
