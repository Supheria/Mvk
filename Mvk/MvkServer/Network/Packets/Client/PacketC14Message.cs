using MvkServer.Command;
using MvkServer.Util;

namespace MvkServer.Network.Packets.Client
{
    /// <summary>
    /// Пакет передачии сообщения или команды на сервер
    /// </summary>
    public struct PacketC14Message : IPacket
    {
        private bool isCommand;
        private CommandSender sender;

        /// <summary>
        /// Структура отправителя комсанды
        /// </summary>
        public CommandSender GetCommandSender() => sender;
        /// <summary>
        /// Строка является ли командой
        /// </summary>
        public bool IsCommand() => isCommand;

        public PacketC14Message(string message)
        {
            sender = new CommandSender(message);
            isCommand = false;
        }

        public PacketC14Message(string message, MovingObjectPosition movingObject)
        {
            sender = new CommandSender(message, movingObject);
            isCommand = true;
        }

        public void ReadPacket(StreamBase stream)
        {
            isCommand = stream.ReadBool();
            if (isCommand)
            {
                CommandSender.EnumForWhat forWhat = (CommandSender.EnumForWhat)stream.ReadByte();
                if (forWhat == CommandSender.EnumForWhat.Block)
                {
                    sender = new CommandSender(stream.ReadString(), new BlockPos(stream.ReadInt(), stream.ReadInt(), stream.ReadInt()));
                }
                else if (forWhat == CommandSender.EnumForWhat.Entity)
                {
                    sender = new CommandSender(stream.ReadString(), stream.ReadUShort());
                }
                else
                {
                    sender = new CommandSender(stream.ReadString());
                }
            }
            else
            {
                sender = new CommandSender(stream.ReadString());
            }
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteBool(isCommand);
            if (isCommand)
            {
                stream.WriteByte((byte)sender.GetForWhat());
                stream.WriteString(sender.GetMessage());
                if (sender.GetForWhat() == CommandSender.EnumForWhat.Block)
                {
                    BlockPos blockPos = sender.GetBlockPos();
                    stream.WriteInt(blockPos.X);
                    stream.WriteInt(blockPos.Y);
                    stream.WriteInt(blockPos.Z);
                }
                else if (sender.GetForWhat() == CommandSender.EnumForWhat.Entity)
                {
                    stream.WriteUShort(sender.GetEntityId());
                }
            }
            else
            {
                stream.WriteString(sender.GetMessage());
            }
        }
    }
}
