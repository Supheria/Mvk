using MvkServer.Entity.List;
using MvkServer.Inventory;
using MvkServer.Item;
using MvkServer.NBT;
using MvkServer.Network.Packets.Server;
using MvkServer.World;
using System.Collections.Generic;

namespace MvkServer.TileEntity.List
{
    /// <summary>
    /// Объект сущности плитки хранилища
    /// </summary>
    public abstract class TileEntityStorage : TileEntityBase
    {
        protected ItemStack[] stacks;

        /// <summary>
        /// Управление контейнером для передачи пачками
        /// </summary>
        private ConteinerManagement conteiner = new ConteinerManagement();

        public TileEntityStorage(WorldBase world, int count) : base(world)
        {
            stacks = new ItemStack[count];
            conteiner.SendSetSlot += Conteiner_SendSetSlot;
        }

        private void Conteiner_SendSetSlot(object sender, StackEventArgs e)
        {
            // Отправляем пакет на склад
            World.SendToAllPlayersUseTileEntity(new PacketS2FSetSlot(e.Slot + 100, e.Stack), Position);
            if (Type == EnumTileEntities.Crafting)
            {
                // Отправить список крафта
                World.SendToAllPlayersUseTileEntityListCraft(Position);
            }
            MarkDirty();
        }

        /// <summary>
        /// Получить стак в конкретном слоте
        /// </summary>
        public override ItemStack GetStackInSlot(int slotIn) => stacks[slotIn];

        /// <summary>
        /// Задать стак в конкретном слоте
        /// </summary>
        public override void SetStackInSlot(int slotIn, ItemStack stack)
        {
            stacks[slotIn] = stack;
            MarkDirty();
        }

        /// <summary>
        /// Добавляет стек предметов в инвентарь, возвращает false, если это невозможно.
        /// </summary>
        public override bool AddItemStackToInventory(ItemStack stack) => conteiner.AddItemStackToInventory(stacks, stack);

        /// <summary>
        /// Открыли окно, вызывается объектом EntityPlayerServer
        /// </summary>
        public override void OpenWindow(WorldServer worldServer, EntityPlayerServer entityPlayer) 
            => entityPlayer.SendPacket(new PacketS30WindowItems(false, stacks));

        /// <summary>
        /// Заспавнить предметы при разрушении блока
        /// </summary>
        public override void SpawnAsEntityOnBreakBlock()
        {
            foreach(ItemStack stack in stacks)
            {
                if (stack != null)
                {
                    ItemStack.SpawnAsEntity(World, Position, stack);
                }
            }
        }

        public override void WriteToNBT(TagCompound nbt)
        {
            base.WriteToNBT(nbt);

            NBTTools.ItemStacksWriteToNBT(nbt, "Stacks", stacks);
        }

        public override void ReadFromNBT(TagCompound nbt)
        {
            base.ReadFromNBT(nbt);

            int count = stacks.Length;
            for (int i = 0; i < count; i++) stacks[i] = null;
            Dictionary<int, ItemStack> map = NBTTools.ItemStacksReadFromNBT(nbt, "Stacks");
            foreach (KeyValuePair<int, ItemStack> entry in map)
            {
                if (entry.Key < count)
                {
                    stacks[entry.Key] = entry.Value;
                }
            }
        }
    }
}
