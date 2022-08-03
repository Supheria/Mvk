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
            Hardness = 6000000;
            Material = EnumMaterial.Bedrock;
            InitBoxs(1);
        }
    }
}
