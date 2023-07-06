using MvkServer.World.Block;

namespace MvkServer.World.Gen.Feature
{
    /// <summary>
    /// Генерация фруктового дерева
    /// </summary>
    public class WorldGenTreeFruit2 : WorldGenTree2
    {
        public WorldGenTreeFruit2()
        {
            log = EnumBlock.LogFruit;
            leaves = EnumBlock.LeavesFruit;
            sapling = EnumBlock.SaplingFruit;
            idLog = (ushort)log;
            idLeaves = (ushort)leaves;
            idSapling = (ushort)sapling;
        }

        /// <summary>
        /// Случайные атрибуты дерева
        /// </summary>
        protected override void RandSize()
        {
            // Высота ствола дерева до кроны
            trunkHeight = NextInt(8) + 6;
            // Ствол снизу без веток 
            trunkWithoutBranches = NextInt(3) + 2;
            // смещение, через какое ствол может смещаться
            trunkBias = NextInt(8) + 2;
            // Максимальное смещение ствола от пенька
            maxTrunkBias = 6;
            // Количество секций веток для сужения
            //                _    / \
            //          _    / \  |   |
            //    _    / \  |   | |   |
            //   / \  |   | |   | |   |
            //   \ /   \ /   \ /   \ /
            //  2 |   3 |   4 |   5 |
            sectionCountBranches = 3;
            // Минимальная обязательная длинна ветки
            branchLengthMin = 3;
            // Случайная дополнительная длинна ветки к обязательной
            branchLengthRand = 3;
            // Насыщенность листвы на ветке, меньше 1 не допустимо, чем больше тем веток меньше
            foliageBranch = 64;
        }
    }
}
