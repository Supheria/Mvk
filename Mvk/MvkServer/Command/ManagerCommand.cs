using MvkServer.Util;
using MvkServer.World;
using System.Collections.Generic;

namespace MvkServer.Command
{
    /// <summary>
    /// Менеджер отвечающий за выполнение команд
    /// </summary>
    public class ManagerCommand
    {
        /// <summary>
        /// Серверный объект мира
        /// </summary>
        private readonly WorldServer world;
        /// <summary>
        /// Перечень всех доступных команд
        /// </summary>
        private readonly Dictionary<string, CommandBase> commands = new Dictionary<string, CommandBase>();

        public ManagerCommand(WorldServer world)
        {
            this.world = world;
            Initialize();
        }

        private void Initialize()
        {
            Registration(new CommandKill(world));
            Registration(new CommandTime(world));
            Registration(new CommandGameMode(world));
            Registration(new CommandTeleport(world));
        }

        /// <summary>
        /// Регистрация команды
        /// </summary>
        private void Registration(CommandBase command)
        {
            if (command.NameMin != "") commands.Add(command.NameMin, command);
            commands.Add(command.Name, command);
        }

        /// <summary>
        /// Выполнение команды
        /// </summary>
        /// <param name="sender">Отправитель команды</param>
        /// <returns>Возвращает строку для оповищения в чате консоля конкретному игроку</returns>
        public string ExecutionCommand(CommandSender sender)
        {
            string commandName = sender.GetCommandName();
            
            if (commandName != "" && commands.ContainsKey(commandName))
            {
                // Команда такая имеется, выполняем
                return commands[commandName].UseCommand(sender);
            }
            // Команды такой нет, оповещаем
            // TODO::2023-05-17 столкнулся с локализацией, она работает только у клиента, сервер о ней не знает, как оповещать серверу пока не ясно!!!
            return ChatStyle.Red + "commands.generic.notFound [" + sender.GetMessage() + "]";
        }
    }
}
