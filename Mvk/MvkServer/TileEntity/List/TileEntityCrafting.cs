using MvkServer.Item;
using MvkServer.World;

namespace MvkServer.TileEntity.List
{
    /// <summary>
    /// Объект сущности плитки для крафт столов
    /// </summary>
    public class TileEntityCrafting : TileEntityStorage
    {
        public TileEntityCrafting(WorldBase world) : base(world, 1)
            => Type = EnumTileEntities.Crafting;

        /// <summary>
        /// Проверяем можно ли установить данный стак в определённой ячейке склада
        /// </summary>
        public override bool CanPutItemStack(ItemStack stack) 
            => stack != null && stack.Item != null && stack.Item.IsTool();
    }
}
