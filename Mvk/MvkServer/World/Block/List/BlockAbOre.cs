using MvkServer.Glm;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Абстрактный объект руды
    /// </summary>
    public abstract class BlockAbOre : BlockBase
    {
        public BlockAbOre(int numberTexture, vec3 color)
        {
            Material = EnumMaterial.Ore;
            Color = color;
            Particle = numberTexture;
            InitBoxs();
        }

        protected virtual void InitBoxs()
        {
            InitBoxs(Particle, false, Color);
        }
    }
}
