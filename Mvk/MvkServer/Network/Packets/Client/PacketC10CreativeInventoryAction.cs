using MvkServer.Item;

namespace MvkServer.Network.Packets.Client
{
    /// <summary>
    /// Пакет действий инвентаря креатива
    /// </summary>
    public struct PacketC10CreativeInventoryAction : IPacket
    {
        private int slotId;
        private ItemStack stack;

        public int GetSlotId() => slotId;
        public ItemStack GetStack() => stack;

        public PacketC10CreativeInventoryAction(int slotId, ItemStack stack)
        {
            this.slotId = slotId;
            this.stack = stack?.Copy();
        }

        public void ReadPacket(StreamBase stream)
        {
            slotId = stream.ReadShort();
            stack = ItemStack.ReadStream(stream);
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteShort((short)slotId);
            ItemStack.WriteStream(stack, stream);
        }
    }
}
