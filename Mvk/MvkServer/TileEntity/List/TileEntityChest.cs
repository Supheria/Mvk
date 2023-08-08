using MvkServer.World;

namespace MvkServer.TileEntity.List
{
    /// <summary>
    /// Объект сущности плитки сундука
    /// </summary>
    public class TileEntityChest : TileEntityStorage
    {
        public TileEntityChest(WorldBase world) : base(world, 4) 
            => Type = EnumTileEntities.Chest;
    }
}
