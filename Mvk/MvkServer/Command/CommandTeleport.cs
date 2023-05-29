using MvkServer.Entity.List;
using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World;

namespace MvkServer.Command
{
    /// <summary>
    /// Команда телепортация игрока
    /// </summary>
    public class CommandTeleport : CommandBase
    {
        public CommandTeleport(WorldServer world) : base(world)
        {
            Name = "tp";
        }

        /// <summary>
        /// Возвращает true, если отправителю данной команды разрешено использовать эту команду
        /// </summary>
        public override string UseCommand(CommandSender sender)
        {
            EntityPlayerServer player = sender.GetPlayer();
            if (player == null)
            {
                return ChatStyle.Red + "commands.tp.notPlayer";
            }
            string[] commandParams = sender.GetCommandParams();
            if (commandParams.Length < 3)
            {
                return ChatStyle.Red + "commands.tp.notParams";
            }
            int[] param = new int[3];
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    param[i] = int.Parse(commandParams[i]);
                }
                catch
                {
                    return ChatStyle.Red + "commands.tp.errorParmas";
                }
            }
            player.SetPositionServer(new vec3(param[0], param[1], param[2]));
            return ChatStyle.Gray + "commands.tp";
        }
    }
}
