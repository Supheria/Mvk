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
            InitBoxs(numberTexture, false, color);
        }
    }
}
