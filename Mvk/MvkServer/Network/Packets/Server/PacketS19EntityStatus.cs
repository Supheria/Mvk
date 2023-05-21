namespace MvkServer.Network.Packets.Server
{
    /// <summary>
    /// Пакет статуса сущности, умирает, урон и прочее
    /// </summary>
    public struct PacketS19EntityStatus : IPacket
    {
        private byte status;
        private string text;

        public EnumStatus GetStatus() => (EnumStatus)status;
        public string GetText() => text;

        public PacketS19EntityStatus(EnumStatus status, string text)
        {
            this.status = (byte)status;
            this.text = text;
        }

        public void ReadPacket(StreamBase stream)
        {
            status = stream.ReadByte();
            text = stream.ReadString();
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteByte(status);
            stream.WriteString(text);
        }

        public enum EnumStatus
        {
            /// <summary>
            /// Умирает
            /// </summary>
            Die = 1,
            /// <summary>
            /// Нанесён урон
            /// </summary>
            //Damage = 2
        }
    }
}
