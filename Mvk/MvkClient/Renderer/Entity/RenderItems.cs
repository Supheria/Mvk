using MvkServer.Item;
using MvkServer.Item.List;
using MvkServer.World.Block;
using System.Collections.Generic;

namespace MvkClient.Renderer.Entity
{
    /// <summary>
    /// Рендер предметов для мира
    /// </summary>
    public class RenderItems
    {
        /// <summary>
        /// Карта всех блоков
        /// </summary>
        private readonly Dictionary<EnumBlock, RenderBlock> blocks = new Dictionary<EnumBlock, RenderBlock>();
        /// <summary>
        /// Карта всех предметов
        /// </summary>
        private readonly Dictionary<EnumItem, RenderItem> items = new Dictionary<EnumItem, RenderItem>();

        public void Render(ItemStack stack) => Render(stack.Item);
        public void Render(ItemBase item)
        {
            if (item == null) return;

            if (item.EItem == EnumItem.Block && item is ItemBlock itemBlock)
            {
                RenderBlock renderBlock = GetRenderBlock(itemBlock.Block.EBlock);
                renderBlock.Render();
            }
            else
            {
                RenderItem renderItem = GetRenderItem(item.EItem);
                renderItem.Render();
            }
        }

        /// <summary>
        /// Получить рендер блока, если его ещё не создавали, создать
        /// </summary>
        private RenderBlock GetRenderBlock(EnumBlock enumBlock)
        {
            if (blocks.ContainsKey(enumBlock))
            {
                return blocks[enumBlock];
            }
            else
            {
                RenderBlock renderBlock = new RenderBlock(enumBlock);
                blocks.Add(enumBlock, renderBlock);
                return renderBlock;
            }
        }
        /// <summary>
        /// Получить рендер блока, если его ещё не создавали, создать
        /// </summary>
        private RenderItem GetRenderItem(EnumItem enumItem)
        {
            if (items.ContainsKey(enumItem))
            {
                return items[enumItem];
            }
            else
            {
                RenderItem renderItem = new RenderItem(enumItem);
                items.Add(enumItem, renderItem);
                return renderItem;
            }
        }
    }
}
