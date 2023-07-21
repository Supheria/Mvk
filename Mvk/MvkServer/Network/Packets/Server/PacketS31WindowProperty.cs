namespace MvkServer.Network.Packets.Server
{
    /// <summary>
    /// Пакет опций окна
    /// </summary>
    public struct PacketS31WindowProperty : IPacket
    {
        private int[] recipe;
        private int window;
        private EnumAction action;

        public EnumAction GetAction() => action;
        public int[] GetRecipe() => recipe;
        public int GetWindow() => window;

        public PacketS31WindowProperty(EnumAction action)
        {
            this.action = action;
            recipe = new int[0];
            window = 0;
        }
        public PacketS31WindowProperty(int[] recipe)
        {
            action = EnumAction.ArrayRecipe;
            this.recipe = recipe;
            window = 0;
        }
        public PacketS31WindowProperty(int window)
        {
            action = EnumAction.CraftOpen;
            recipe = new int[0];
            this.window = window;
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
            else if (action == EnumAction.CraftOpen)
            {
                window = stream.ReadByte();
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
            else if (action == EnumAction.CraftOpen)
            {
                stream.WriteByte((byte)window);
            }
        }

        /// <summary>
        /// Варианты действия
        /// </summary>
        public enum EnumAction
        {
            /// <summary>
            /// Остановить крафт
            /// </summary>
            CraftStop = 1,
            /// <summary>
            /// Открыть крафт
            /// </summary>
            CraftOpen = 2,
            /// <summary>
            /// Передать массив доступных рецептов для крафта
            /// </summary>
            ArrayRecipe = 3
            
        }

    }
}
