using MvkServer.Entity.List;
using MvkServer.Item;
using MvkServer.NBT;
using MvkServer.Util;
using MvkServer.World;
using MvkServer.World.Block;
using MvkServer.World.Chunk;

namespace MvkServer.TileEntity
{
    /// <summary>
    /// Базовый объект сущности плитки, так сказать для доп данных и действий блока
    /// </summary>
    public abstract class TileEntityBase
    {
        /// <summary>
        /// Объект мира
        /// </summary>
        public WorldBase World { get; private set; }
        /// <summary>
        /// Позиция к какому блоку принадлежит плитка
        /// </summary>
        public BlockPos Position { get; private set; }
        /// <summary>
        /// Объект блока принадлежит плитка
        /// </summary>
        public BlockBase Block { get; private set; }
        /// <summary>
        /// Дополнительные параметры блока 4 bita или если IsAddMet то 16 bit;
        /// </summary>
        public ushort BlockMet { get; private set; }
        /// <summary>
        /// Недействительна плитка блока
        /// </summary>
        public bool IsInvalid { get; private set; } = false;
        /// <summary>
        /// Тип сущности плитки
        /// </summary>
        public EnumTileEntities Type { get; protected set; } = EnumTileEntities.None;

        public TileEntityBase(WorldBase world)
        {
            World = world;
        }

        /// <summary>
        /// Делает недействительным объект плитки
        /// </summary>
        public void Invalidate() => IsInvalid = true;
        /// <summary>
        /// Делает действительным объект плитки
        /// </summary>
        public void Validate() => IsInvalid = false;

        /// <summary>
        /// Задать позицию плитки
        /// </summary>
        public void SetBlockPosition(BlockState blockState, BlockBase block, BlockPos pos)
        {
            Position = pos;
            Block = block;
            BlockMet = blockState.met;
        }

        /// <summary>
        /// Задать блок плитки
        /// </summary>
        public void InitBlock(ChunkBase chunk)
        {
            BlockState blockState = chunk.GetBlockState(Position.X & 15, Position.Y, Position.Z & 15);
            Block = blockState.GetBlock();
            BlockMet = blockState.met;
        }

        /// <summary>
        /// Открыли окно, вызывается объектом EntityPlayerServer
        /// </summary>
        public virtual void OpenWindow(WorldServer worldServer, EntityPlayerServer entityPlayer) { }

        #region ItemStack

        /// <summary>
        /// Получить стак в конкретном слоте
        /// </summary>
        public virtual ItemStack GetStackInSlot(int slotIn) => null;

        /// <summary>
        /// Задать стак в конкретном слоте
        /// </summary>
        public virtual void SetStackInSlot(int slotIn, ItemStack stack) { }

        /// <summary>
        /// Заспавнить предметы при разрушении блока
        /// </summary>
        public virtual void SpawnAsEntityOnBreakBlock() { }

        /// <summary>
        /// Добавляет стек предметов в инвентарь, возвращает false, если это невозможно.
        /// </summary>
        public virtual bool AddItemStackToInventory(ItemStack stack) => false;

        /// <summary>
        /// Проверяем можно ли установить данный стак в определённой ячейке склада
        /// </summary>
        public virtual bool CanPutItemStack(ItemStack stack) => true;

        #endregion

        /// <summary>
        /// Отметить что было изменение, для перезаписи на сервере в файл сохранения
        /// </summary>
        public void MarkDirty() => World.ChunkModified(Position);

        public virtual void WriteToNBT(TagCompound nbt)
        {
            nbt.SetByte("Id", (byte)Type);
            nbt.SetInt("X", Position.X);
            nbt.SetInt("Y", Position.Y);
            nbt.SetInt("Z", Position.Z);
        }

        public virtual void ReadFromNBT(TagCompound nbt)
        {
            Position = new BlockPos(nbt.GetInt("X"), nbt.GetInt("Y"), nbt.GetInt("Z"));
        }
    }
}
