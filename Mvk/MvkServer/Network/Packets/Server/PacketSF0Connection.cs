namespace MvkServer.Network.Packets.Server
{
    public struct PacketSF0Connection : IPacket
    {
        private string cause;
        private bool isBegin;

        /// <summary>
        /// Если причина не указана, то клиент соединился, отправляем пинг, если есть причина, то дисконект
        /// </summary>
        public PacketSF0Connection(string cause)
        {
            this.cause = cause;
            isBegin = false;
        }
        public PacketSF0Connection(bool isBegin)
        {
            cause = "";
            this.isBegin = isBegin;
        }

        public bool IsConnect() => isBegin ? false : cause == "";
        public string GetCause() => cause;
        public bool IsBegin() => isBegin;

        public void ReadPacket(StreamBase stream)
        {
            isBegin = stream.ReadBool();
            if (!isBegin)
            {
                cause = stream.ReadString();
            }
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteBool(isBegin);
            if (!isBegin)
            {
                stream.WriteString(cause);
            }
        }
    }
}
