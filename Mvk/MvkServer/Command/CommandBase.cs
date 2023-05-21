using MvkServer.World;

namespace MvkServer.Command
{

    /// <summary>
    /// Абстрактный объект команды
    /// </summary>
    public abstract class CommandBase
    {
        /// <summary>
        /// Серверный объект мира
        /// </summary>
        protected readonly WorldServer world;

        /// <summary>
        /// Название команды в нижнем регистре
        /// </summary>
        public string Name { get; protected set; } = "";
        /// <summary>
        /// Название команды в упрощёном виде
        /// </summary>
        public string NameMin { get; protected set; } = "";

        public CommandBase(WorldServer world) => this.world = world;

        /// <summary>
        /// Возвращает true, если отправителю данной команды разрешено использовать эту команду
        /// </summary>
        public virtual string UseCommand(CommandSender sender) => "";
    }
}
