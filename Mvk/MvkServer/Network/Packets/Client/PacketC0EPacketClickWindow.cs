namespace MvkServer.Network.Packets.Client
{
    /// <summary>
    /// Пакет кликов по окну инвентаря, крафта и других подобных окон
    /// </summary>
    public struct PacketC0EPacketClickWindow : IPacket
    {
        private int number;
        private EnumAction action;

        public int GetNumber() => number;
        public EnumAction GetAction() => action;

        public PacketC0EPacketClickWindow(EnumAction action, int number = 0)
        {
            this.number = number;
            this.action = action;
        }

        public void ReadPacket(StreamBase stream)
        {
            number = stream.ReadShort();
            action = (EnumAction)stream.ReadByte();
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteShort((short)number);
            stream.WriteByte((byte)action);
        }

        /// <summary>
        /// Варианты действия
        /// </summary>
        public enum EnumAction
        {
            /// <summary>
            /// Открыто окно
            /// </summary>
            Open = 0,
            /// <summary>
            /// Закрыть окно
            /// </summary>
            Close = 1,
            /// <summary>
            /// Выкинуть предмет из рук что в кэше
            /// </summary>
            ThrowOutAir = 2,
            /// <summary>
            /// Левый кликнули на слот инвентаря
            /// </summary>
            ClickLeftSlot = 3,
            /// <summary>
            /// Правый кликнули на слот инвентаря
            /// </summary>
            ClickRightSlot = 4,
            /// <summary>
            /// Запрос на сделать крафт одного предмета
            /// </summary>
            CraftOne = 5,
            /// <summary>
            /// Запрос на сделать крафт стака предметов
            /// </summary>
            CraftMax = 6,
        }
    }
}
