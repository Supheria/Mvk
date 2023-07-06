using MvkServer.Glm;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Абстрактный объект руды
    /// </summary>
    public abstract class BlockAbOre : BlockBase
    {
        public BlockAbOre(int numberTexture)
        {
            Material = EnumMaterial.Ore;
            Particle = numberTexture;
            Resistance = 5.0f;
            InitQuads();
        }

        protected virtual void InitQuads() => InitQuads(Particle);
    }
}
