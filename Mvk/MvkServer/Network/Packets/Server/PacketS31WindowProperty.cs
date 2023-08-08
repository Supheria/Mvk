namespace MvkServer.Network.Packets.Server
{
    /// <summary>
    /// Пакет опций окна
    /// </summary>
    public struct PacketS31WindowProperty : IPacket
    {
        private int[] recipe;
        private EnumAction action;

        public EnumAction GetAction() => action;
        public int[] GetRecipe() => recipe;

        public PacketS31WindowProperty(EnumAction action)
        {
            this.action = action;
            recipe = new int[0];
        }
        public PacketS31WindowProperty(int[] recipe)
        {
            action = EnumAction.ArrayRecipe;
            this.recipe = recipe;
        }

        public void ReadPacket(StreamBase stream)
        {
            action = (EnumAction)stream.ReadByte();
            if (action == EnumAction.ArrayRecipe)
            {
                int count = stream.ReadShort();
                recipe = new int[count];
                for (int i = 0; i < count; i++)
                {
                    recipe[i] = stream.ReadShort();
                }
            }
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteByte((byte)action);
            if (action == EnumAction.ArrayRecipe)
            {
                int count = recipe.Length;
                stream.WriteShort((short)count);
                for(int i = 0; i < count; i++)
                {
                    stream.WriteShort((short)recipe[i]);
                }
            }
        }

        /// <summary>
        /// Варианты действия
        /// </summary>
        public enum EnumAction
        {
            /// <summary>
            /// Закрыть окно, сервер требует
            /// </summary>
            CloseWindow = 1,
            /// <summary>
            /// Остановить крафт
            /// </summary>
            CraftStop = 2,
            /// <summary>
            /// Передать массив доступных рецептов для крафта
            /// </summary>
            ArrayRecipe = 3,
        }
    }
}
