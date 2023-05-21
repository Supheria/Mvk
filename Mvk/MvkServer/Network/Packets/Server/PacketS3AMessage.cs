namespace MvkServer.Network.Packets.Server
{
    /// <summary>
    /// Пакет получения сообщения с сервера
    /// </summary>
    public struct PacketS3AMessage : IPacket
    {
        private bool isConsole;
        private string message;

        public bool IsConsole() => isConsole;
        public string GetMessage() => message;

        public PacketS3AMessage(string message, bool isConsole)
        {
            this.message = message;
            this.isConsole = isConsole;
        }

        public void ReadPacket(StreamBase stream)
        {
            isConsole = stream.ReadBool();
            message = stream.ReadString();
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteBool(isConsole);
            stream.WriteString(message);
        }
    }
}
