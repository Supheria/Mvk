using MvkServer.Entity.List;
using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World;
using System.Diagnostics;

namespace MvkServer.Command
{
    /// <summary>
    /// Команда починки
    /// </summary>
    public class CommandFix : CommandBase
    {
        public CommandFix(WorldServer world) : base(world)
        {
            Name = "fix";
        }

        /// <summary>
        /// Возвращает true, если отправителю данной команды разрешено использовать эту команду
        /// </summary>
        public override string UseCommand(CommandSender sender)
        {
            EntityPlayerServer player = sender.GetPlayer();
            if (player == null)
            {
                return ChatStyle.Red + "commands.fix.notPlayer";
            }
            string[] commandParams = sender.GetCommandParams();
            if (commandParams.Length == 0)
            {
                return ChatStyle.Red + "commands.fix.notParmas";
            }

            string param = commandParams[0].ToLower();
            if (param.Equals("light") || param.Equals("l"))
            {
                // Чиним свет в чанке
                return FixLightBlock(player.GetChunkPos(), commandParams.Length > 1 ? commandParams[1] : "");
            }
            
            return ChatStyle.Red + "commands.fix.unknownParma";
        }

        /// <summary>
        /// Чиним Блочное освещение которое не затухло, как правило после огня
        /// </summary>
        private string FixLightBlock(vec2i posCh, string param2)
        {
            int radius = 1;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            string result = "";
            int count = 0;
            int countError = 0;
            int c;
            if (param2 != "")
            {
                try { radius = int.Parse(param2); } catch { }
            }
            if (radius == 1)
            {
                count = world.Light.FixChunkLightBlock(posCh);
                if (count == -1)
                {
                    count = 0;
                    countError = 1;
                }
            }
            else
            {
                int x, y;
                int x1 = posCh.x - radius;
                int y1 = posCh.y - radius;
                int x2 = posCh.x + radius;
                int y2 = posCh.y + radius;
                for (x = x1; x <= x2; x++)
                {
                    for (y = y1; y <= y2; y++)
                    {
                        c = world.Light.FixChunkLightBlock(new vec2i(x, y));
                        if (c == -1) countError++;
                        else count += c;
                    }
                }
            }
            // chunkNo = ERROR: No neighboring chunks
            result = ChatStyle.Gray + string.Format("FixLight chunk:[{0}]r:{1}  {2:0.00} ms count fix:{3} chunkNo:{4}",
                posCh, radius, stopwatch.ElapsedTicks / (float)MvkStatic.TimerFrequency, count, countError);
            world.Log.Log(result);
            return result;
        }
    }
}
