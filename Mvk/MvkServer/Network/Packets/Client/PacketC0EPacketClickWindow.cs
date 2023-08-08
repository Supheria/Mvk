namespace MvkServer.Network.Packets.Client
{
    /// <summary>
    /// Пакет кликов по окну инвентаря, крафта и других подобных окон
    /// </summary>
    public struct PacketC0EPacketClickWindow : IPacket
    {
        private EnumAction action;
        private bool one;
        private int number;
        private bool isShift;
        private bool isRight;

        public EnumAction GetAction() => action;
        public int GetNumber() => number;
        public bool IsShift() => isShift;
        public bool IsRight() => isRight;

        public PacketC0EPacketClickWindow(EnumAction action)
        {
            one = true;
            this.action = action;
            isShift = false;
            isRight = false;
            number = 0;
        }

        public PacketC0EPacketClickWindow(EnumAction action, bool isShift, bool isRight, int number)
        {
            one = false;
            this.action = action;
            this.isShift = isShift;
            this.isRight = isRight;
            this.number = number;
        }

        public void ReadPacket(StreamBase stream)
        {
            action = (EnumAction)stream.ReadByte();
            one = stream.ReadBool();
            if (!one)
            {
                isShift = stream.ReadBool();
                isRight = stream.ReadBool();
                number = stream.ReadShort();
            }
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteByte((byte)action);
            stream.WriteBool(one);
            if (!one)
            {
                stream.WriteBool(isShift);
                stream.WriteBool(isRight);
                stream.WriteShort((short)number);
            }
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
            /// Кликнули на слот инвентаря
            /// </summary>
            ClickSlot = 3,
            /// <summary>
            /// Запрос на сделать крафт предмета(ов)
            /// </summary>
            Craft = 4
        }
    }
}
