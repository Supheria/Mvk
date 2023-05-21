using MvkServer.Entity.List;
using MvkServer.Util;
using System;

namespace MvkServer.Command
{
    /// <summary>
    /// Структура отправителя команды
    /// </summary>
    public struct CommandSender
    {
        private readonly string message;
        private readonly EnumForWhat forWhat;
        private readonly BlockPos blockPos;
        private readonly ushort entityId;
        private EntityPlayerServer player;

        public CommandSender(string message)
        {
            this.message = message;
            forWhat = EnumForWhat.Null;
            entityId = 0;
            blockPos = new BlockPos();
            player = null;
        }

        public CommandSender(string message, ushort entityId)
        {
            this.message = message;
            forWhat = EnumForWhat.Entity;
            this.entityId = entityId;
            blockPos = new BlockPos();
            player = null;
        }
        public CommandSender(string message, BlockPos blockPos)
        {
            this.message = message;
            forWhat = EnumForWhat.Entity;
            entityId = 0;
            this.blockPos = blockPos;
            player = null;
        }

        public CommandSender(string message, MovingObjectPosition movingObject)
        {
            this.message = message;
            player = null;
            if (movingObject.IsBlock())
            {
                forWhat = EnumForWhat.Block;
                entityId = 0;
                blockPos = movingObject.BlockPosition;

            }
            else if (movingObject.IsEntity())
            {
                forWhat = EnumForWhat.Entity;
                entityId = movingObject.Entity.Id;
                blockPos = new BlockPos();
            }
            else
            {
                forWhat = EnumForWhat.Null;
                entityId = 0;
                blockPos = new BlockPos();
            }
        }

        /// <summary>
        /// Строка сообщения
        /// </summary>
        public string GetMessage() => message;
        /// <summary>
        /// На что попадает курсор того кто отправлял команду
        /// </summary>
        public EnumForWhat GetForWhat() => forWhat;
        /// <summary>
        /// Координата блока на который попал курсор
        /// </summary>
        public BlockPos GetBlockPos() => blockPos;
        /// <summary>
        /// ID сущности на которую попал курсор
        /// </summary>
        public ushort GetEntityId() => entityId;
        /// <summary>
        /// Получить объект игрока который использует команду
        /// </summary>
        public EntityPlayerServer GetPlayer() => player;
        /// <summary>
        /// Задать объект игрока который использует команду
        /// </summary>
        public CommandSender SetPlayer(EntityPlayerServer entity)
        {
            player = entity;
            return this;
        }

        /// <summary>
        /// Получить название команды в нижнем регистре
        /// </summary>
        public string GetCommandName()
        {
            int index = message.IndexOf(" ");
            if (index == -1) return message.ToLower();
            return message.Substring(0, index).ToLower();
        }
        /// <summary>
        /// Получить массив команд
        /// </summary>
        public string[] GetCommandParams()
        {
            string[] vs = message.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            int count = vs.Length;
            if (count < 2) return new string[0];

            string[] vs2 = new string[count - 1];
            for (int i = 1; i < count; i++)
            {
                vs2[i - 1] = vs[i];
            }
            return vs2;
        }

        public override string ToString() => "[" + forWhat + "] " + message;

        /// <summary>
        /// На что попадает луч
        /// </summary>
        public enum EnumForWhat
        {
            Null = 0,
            Block = 1,
            Entity = 2
        }
    }
}
