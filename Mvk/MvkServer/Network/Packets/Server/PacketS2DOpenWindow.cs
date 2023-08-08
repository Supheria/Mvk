using MvkServer.Util;

namespace MvkServer.Network.Packets.Server
{
    /// <summary>
    /// Пакет открытия окна
    /// </summary>
    public struct PacketS2DOpenWindow : IPacket
    {
        private EnumWindowType windowType;

        public EnumWindowType GetWindowType() => windowType;

        public PacketS2DOpenWindow(EnumWindowType windowType)
        {
            this.windowType = windowType;
        }

        public void ReadPacket(StreamBase stream)
        {
            windowType = (EnumWindowType)stream.ReadByte();
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteByte((byte)windowType);
        }
    }
}