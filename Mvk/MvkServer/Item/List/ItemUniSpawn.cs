using MvkServer.Entity.List;
using MvkServer.World;

namespace MvkServer.Item.List
{
    /// <summary>
    /// Предмет кусочек
    /// </summary>
    public class ItemUniSpawn : ItemUniPiece
    {
        public ItemUniSpawn(EnumItem enumItem, int numberTexture) : base(enumItem, numberTexture, 0) { }

        /// <summary>
        /// Действие при броске
        /// </summary>
        protected override void OnAction(WorldBase worldIn, EntityPlayer playerIn, EnumItem enumItem)
        {
            EntitySpawn entity = new EntitySpawn(worldIn, playerIn);
            entity.SetItem(enumItem);
            worldIn.SpawnEntityInWorld(entity);
        }
    }
}
