using MvkServer.Entity.List;
using MvkServer.Util;
using MvkServer.World;

namespace MvkServer.Command
{
    /// <summary>
    /// Команда режим игры
    /// </summary>
    public class CommandGameMode : CommandBase
    {
        public CommandGameMode(WorldServer world) : base(world)
        {
            Name = "gamemode";
            NameMin = "gm";
        }

        /// <summary>
        /// Возвращает true, если отправителю данной команды разрешено использовать эту команду
        /// </summary>
        public override string UseCommand(CommandSender sender)
        {
            EntityPlayerServer player = sender.GetPlayer();
            if (player == null)
            {
                return ChatStyle.Red + "commands.gamemode.notPlayer";
            }
            string[] commandParams = sender.GetCommandParams();
            if (commandParams.Length == 0)
            {
                return ChatStyle.Red + "commands.gamemode.notParams";
            }
            string param = commandParams[0].ToLower();
            string result = "";
            if (param.Equals("survival") || param.Equals("s") || param.Equals("0"))
            {
                // режим выживания
                player.SetGameMode(0);
                result = ChatStyle.Yellow + "commands.gamemode.survival";
            }
            else if (param.Equals("creative") || param.Equals("c") || param.Equals("1"))
            {
                // творческий режим
                player.SetGameMode(1);
                result = ChatStyle.Green + "commands.gamemode.creative";
            }
            else if (param.Equals("spectator") || param.Equals("sp") || param.Equals("2"))
            {
                // режим наблюдателя
                player.SetGameMode(2);
                result = ChatStyle.Gray + "commands.gamemode.spectator";
            }
            else
            {
                return ChatStyle.Red + "commands.gamemode.errorParmas";
            }
            return result;
        }
    }
}
