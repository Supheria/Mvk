using MvkServer.World;

namespace MvkServer.TileEntity.List
{
    /// <summary>
    /// Объект сущности плитки печь-кострище
    /// </summary>
    public class TileEntityFurnace : TileEntityBase
    {
        public TileEntityFurnace(WorldBase world) : base(world)
        {
            Type = EnumTileEntities.Furnace;
        }
    }
}
