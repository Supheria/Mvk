namespace MvkServer.Network.Packets.Client
{
    public struct PacketC02LoginStart : IPacket
    {
        /// <summary>
        /// Имя игрока
        /// </summary>
        private string name;
        /// <summary>
        /// Проверка дубликата
        /// </summary>
        private bool isCheckDuplicate;

        /// <summary>
        /// Имя игрока
        /// </summary>
        public string GetName() => name;
        /// <summary>
        /// Проверка дубликата
        /// </summary>
        public bool IsCheckDuplicate() => isCheckDuplicate;

        public PacketC02LoginStart(string name, bool isCheckDuplicate)
        {
            this.name = name;
            this.isCheckDuplicate = isCheckDuplicate;
        }

        public void ReadPacket(StreamBase stream)
        {
            isCheckDuplicate = stream.ReadBool();
            name = stream.ReadString();
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteBool(isCheckDuplicate);
            stream.WriteString(name);
        }
    }
}
