using MvkServer.Item;

namespace MvkServer.Network.Packets.Server
{
    /// <summary>
    /// Пакет окна списка предметов
    /// </summary>
    public struct PacketS30WindowItems : IPacket
    {
        private bool isInventory;
        private ItemStack[] stacks;

        public bool IsInventory() => isInventory;
        public ItemStack[] GetStacks() => stacks;

        public PacketS30WindowItems(bool isInventory, ItemStack[] stacks)
        {
            this.isInventory = isInventory;
            this.stacks = stacks;
        }

        public void ReadPacket(StreamBase stream)
        {
            isInventory = stream.ReadBool();
            int count = stream.ReadByte();
            stacks = new ItemStack[count];
            for (int i = 0; i < count; i++)
            {
                stacks[i] = ItemStack.ReadStream(stream);
            }
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteBool(isInventory);
            int count = stacks.Length;
            stream.WriteByte((byte)count);
            for (int i = 0; i < count; i++)
            {
                ItemStack.WriteStream(stacks[i], stream);
            }
        }
    }
}
