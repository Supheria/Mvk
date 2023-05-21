using MvkServer.Util;
using MvkServer.World;

namespace MvkServer.Command
{
    /// <summary>
    /// Команда добавить время
    /// </summary>
    public class CommandTime : CommandBase
    {
        public CommandTime(WorldServer world) : base(world)
        {
            Name = "time";
            NameMin = "t";
        }

        /// <summary>
        /// Возвращает true, если отправителю данной команды разрешено использовать эту команду
        /// </summary>
        public override string UseCommand(CommandSender sender)
        {
            string[] commandParams = sender.GetCommandParams();
            if (commandParams.Length == 0)
            {
                return ChatStyle.Red + "commands.time.notParmas";
            }

            string param = commandParams[0].ToLower();
            uint totalWorldTime = world.GetTotalWorldTime();
            uint timeDay = totalWorldTime % WorldBase.SPEED_DAY;
            uint time = 0;
            if (param.Equals("day") || param.Equals("d"))
            {
                time = (uint)(timeDay < 1000 ? 1000 : WorldBase.SPEED_DAY + 1000) - timeDay;
                world.Players.SendToAllMessage(ChatStyle.Yellow + "Day");
            }
            else if (param.Equals("night") || param.Equals("n"))
            {
                time = (uint)(timeDay < 13000 ? 13000 : WorldBase.SPEED_DAY + 13000) - timeDay;
                world.Players.SendToAllMessage(ChatStyle.Aqua + "Night");
            }
            else
            {
                try
                {
                    time = uint.Parse(param);
                }
                catch
                {
                    return ChatStyle.Red + "commands.time.errorParmas";
                }
            }
            if (time != 0)
            {
                world.ServerMain.SetDayTime(totalWorldTime + time);
            }
            
            return "commands.time.add " + time;
        }
    }
}
