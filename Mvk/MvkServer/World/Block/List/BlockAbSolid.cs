using MvkServer.Glm;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Абстрактный объект твёрдых блоков
    /// </summary>
    public abstract class BlockAbSolid : BlockBase
    {
        public BlockAbSolid(int numberTexture, vec3 color)
        {
            Material = EnumMaterial.Solid;
            Particle = numberTexture;
            Resistance = 10f;
            InitBoxs(numberTexture, false, color);
        }

        /// <summary>
        /// Сколько ударов требуется, чтобы сломать блок в тактах (20 тактов = 1 секунда)
        /// </summary>
        public override int Hardness(BlockState state) => 25;
    }
}
