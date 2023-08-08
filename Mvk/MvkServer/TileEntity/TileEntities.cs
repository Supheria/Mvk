using MvkServer.TileEntity.List;
using MvkServer.World;
using System;

namespace MvkServer.TileEntity
{
    /// <summary>
    /// Объект всех сущностей плитки
    /// </summary>
    public class TileEntities
    {
        /// <summary>
        /// Создать объект сущности плитки
        /// </summary>
        public static TileEntityBase CreateTileEntityByEnum(WorldBase world, EnumTileEntities enumTileEntities)
        {
            switch (enumTileEntities)
            {
                case EnumTileEntities.Furnace: return new TileEntityFurnace(world);
                case EnumTileEntities.Chest: return new TileEntityChest(world);
                case EnumTileEntities.Crafting: return new TileEntityCrafting(world);
            }
            throw new Exception("Объекта сущности плитки [" + enumTileEntities.ToString() + "] не существует");
        }
    }
}
