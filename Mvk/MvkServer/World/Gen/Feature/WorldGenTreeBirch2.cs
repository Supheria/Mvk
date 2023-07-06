using MvkServer.World.Block;

namespace MvkServer.World.Gen.Feature
{
    /// <summary>
    /// Генерация дерева берёзы
    /// </summary>
    public class WorldGenTreeBirch2 : WorldGenTree2
    {
        public WorldGenTreeBirch2()
        {
            log = EnumBlock.LogBirch;
            leaves = EnumBlock.LeavesBirch;
            sapling = EnumBlock.SaplingBirch;
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
            trunkHeight = NextInt(12) + 12;
            // Ствол снизу без веток 
            trunkWithoutBranches = NextInt(3) + 2;
            // смещение, через какое ствол может смещаться
            trunkBias = NextInt(5) + 4;
            // Максимальное смещение ствола от пенька
            maxTrunkBias = 3; 
            // Количество секций веток для сужения
            //                _    / \
            //          _    / \  |   |
            //    _    / \  |   | |   |
            //   / \  |   | |   | |   |
            //   \ /   \ /   \ /   \ /
            //  2 |   3 |   4 |   5 |
            sectionCountBranches = 5;
            // Минимальная обязательная длинна ветки
            branchLengthMin = 1;
            // Случайная дополнительная длинна ветки к обязательной
            branchLengthRand = 2;
            // Насыщенность листвы на ветке, меньше 1 не допустимо, чем больше тем веток меньше
            foliageBranch = 64;
        }
    }
}
