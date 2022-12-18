using MvkServer.Glm;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Универсальный объект твёрдых блоков
    /// </summary>
    public class BlockUniSolid : BlockBase
    {
        private readonly int hardness;
        
        public BlockUniSolid(int numberTexture, vec3 color, int hardness = 25, float resistance = 10f)
        {
            this.hardness = hardness;
            Material = EnumMaterial.Solid;
            Particle = numberTexture;
            Resistance = resistance;
            InitBoxs(numberTexture, false, color);
        }

        /// <summary>
        /// Сколько ударов требуется, чтобы сломать блок в тактах (20 тактов = 1 секунда)
        /// </summary>
        public override int Hardness(BlockState state) => hardness;
    }
}
