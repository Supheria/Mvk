using MvkServer.World.Block;

namespace MvkServer.Util
{
    /// <summary>
    /// Структура кэша блока, для генерации будущих элементов
    /// </summary>
    public struct BlockCache
    {
        /// <summary>
        /// Глобальная координата X
        /// </summary>
        public int x;
        /// <summary>
        /// Глобальная координата Y
        /// </summary>
        public int y;
        /// <summary>
        /// Глобальная координата Z
        /// </summary>
        public int z;
        /// <summary>
        /// ID блока
        /// </summary>
        public ushort id;
        /// <summary>
        /// Дополнительные данные
        /// </summary>
        public ushort met;
        /// <summary>
        /// Тело, доп параметр
        /// </summary>
        public bool body;

        public BlockCache(BlockPos blockPos, EnumBlock enumBlock, ushort met = 0, bool body = false)
        {
            x = blockPos.X;
            y = blockPos.Y;
            z = blockPos.Z;
            id = (ushort)enumBlock;
            this.met = 0;
            this.body = body;
        }
        public BlockCache(int x, int y, int z, ushort id, ushort met = 0, bool body = false)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.id = id;
            this.met = met;
            this.body = body;
        }
    }
}
