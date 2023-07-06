namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок Коренная порода
    /// </summary>
    public class BlockBedrock : BlockBase
    {
        /// <summary>
        /// Блок Коренная порода
        /// </summary>
        public BlockBedrock()
        {
            Particle = 1;
            Material = EnumMaterial.Bedrock;
            Resistance = 6000000;
            InitQuads(1);
        }

        /// <summary>
        /// Сколько ударов требуется, чтобы сломать блок в тактах (20 тактов = 1 секунда)
        /// </summary>
        public override int Hardness(BlockState state) => 6000000;
    }
}
