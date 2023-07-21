using MvkServer.Item;

namespace MvkServer.Network.Packets.Server
{
    public struct PacketS2FSetSlot : IPacket
    {
        private int slot;
        private ItemStack itemStack;

        public int GetSlot() => slot;
        public ItemStack GetItemStack() => itemStack;

        public PacketS2FSetSlot(int slot, ItemStack itemStack)
        {
            this.slot = slot;
            this.itemStack = itemStack;
        }

        public void ReadPacket(StreamBase stream)
        {
            slot = stream.ReadByte();
            bool body = stream.ReadBool();
            itemStack = body ? ItemStack.ReadStream(stream) : null; 
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteByte((byte)slot);
            bool body = itemStack != null;
            stream.WriteBool(body);
            if (body)
            {
                ItemStack.WriteStream(itemStack, stream);
            }
        }
    }
}
