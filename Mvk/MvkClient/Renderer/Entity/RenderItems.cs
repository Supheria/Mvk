using MvkServer.Item;
using MvkServer.Item.List;
using MvkServer.World.Block;

namespace MvkClient.Renderer.Entity
{
    /// <summary>
    /// Рендер предметов для мира
    /// </summary>
    public class RenderItems
    {
        /// <summary>
        /// Массив всех блоков
        /// </summary>
        private readonly RenderBlock[] blocks;
        /// <summary>
        /// Массив всех предметов
        /// </summary>
        private readonly RenderItem[] items;

        public RenderItems()
        {
            blocks = new RenderBlock[BlocksCount.COUNT + 1];
            for (int i = 0; i <= BlocksCount.COUNT; i++)
            {
                blocks[i] = new RenderBlock((EnumBlock)i);
            }
            items = new RenderItem[ItemsCount.COUNT + 1];
            for (int i = 1; i <= ItemsCount.COUNT; i++)
            {
                items[i] = new RenderItem((EnumItem)i);
            }
        }

        public void Render(ItemStack stack) => Render(stack.Item);
        public void Render(ItemBase item)
        {
            if (item == null) return;

            if (item.EItem == EnumItem.Block && item is ItemBlock itemBlock)
            {
                RenderBlock renderBlock = blocks[(int)itemBlock.Block.EBlock];
                renderBlock.Render();
            }
            else
            {
                RenderItem renderItem = items[(int)item.EItem];
                renderItem.Render();
            }
        }
    }
}
