using MvkServer.Glm;
using MvkServer.Item;
using MvkServer.Item.List;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Универсальный объект твёрдых блоков
    /// </summary>
    public class BlockUniSolid : BlockBase
    {
        private readonly int hardness;
        private readonly EnumItem dropItem;

        public BlockUniSolid(int numberTexture, vec3 color, EnumItem dropItem, int hardness = 25, float resistance = 10f)
        {
            this.hardness = hardness;
            this.dropItem = dropItem;
            Material = Materials.GetMaterialCache(EnumMaterial.Solid);
            Particle = numberTexture;
            Resistance = resistance;
            InitQuads();
        }

        protected virtual void InitQuads() => InitQuads(Particle);

        /// <summary>
        /// Сколько ударов требуется, чтобы сломать блок в тактах (20 тактов = 1 секунда)
        /// </summary>
        public override int Hardness(BlockState state) => hardness;

        /// <summary>
        /// Получите предмет, который должен выпасть из этого блока при сборе.
        /// </summary>
        protected override ItemBase GetItemDropped(BlockState state, Rand rand, ItemAbTool itemTool)
        {
            if (dropItem == EnumItem.Block)
            {
                return base.GetItemDropped(state, rand, itemTool);
            }
            return Items.GetItemCache(dropItem);
        }
            
    }
}
