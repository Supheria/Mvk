using MvkServer.Entity.List;
using MvkServer.Util;
using MvkServer.World;

namespace MvkServer.Command
{
    /// <summary>
    /// Команда удалить игрока
    /// </summary>
    public class CommandKill : CommandBase
    {
        public CommandKill(WorldServer world) : base(world)
        {
            Name = "kill";
        }

        /// <summary>
        /// Возвращает true, если отправителю данной команды разрешено использовать эту команду
        /// </summary>
        public override string UseCommand(CommandSender sender)
        {
            string[] commandParams = sender.GetCommandParams();
            EntityPlayerServer player = sender.GetPlayer();
            if (commandParams.Length == 0)
            {
                // Убиваем себя
                if (player == null)
                {
                    return ChatStyle.Red + "commands.kill.notPlayer";
                }
                player.OnDeathPlayerServer(world, EnumDamageSource.Kill, player);
                return "";
            }

            // Убиваем игрока по пораметру
            EntityPlayerServer entity = (EntityPlayerServer)world.GetPlayerToName(commandParams[0]);
            if (entity == null)
            {
                return ChatStyle.Red + "commands.kill.notPlayer [" + commandParams[0] + "]";
            }
            entity.OnDeathPlayerServer(world, EnumDamageSource.Kill, player);
            return "";
        }
    }
}
