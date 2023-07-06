using MvkServer.World.Block;

namespace MvkServer.World.Gen.Feature
{
    /// <summary>
    /// Генерация дуба дерева
    /// </summary>
    public class WorldGenTreeOak2 : WorldGenTree2
    {
        public WorldGenTreeOak2()
        {
            log = EnumBlock.LogOak;
            leaves = EnumBlock.LeavesOak;
            sapling = EnumBlock.SaplingOak;
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
            trunkHeight = NextInt(11) + 12;
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
            sectionCountBranches = 4;
            // Минимальная обязательная длинна ветки
            branchLengthMin = 4;
            // Случайная дополнительная длинна ветки к обязательной
            branchLengthRand = 4;
            // Насыщенность листвы на ветке, меньше 1 не допустимо, чем больше тем веток меньше
            foliageBranch = NextInt(8) == 0 ? NextInt(8) + 1 : 32;
        }
    }
}
