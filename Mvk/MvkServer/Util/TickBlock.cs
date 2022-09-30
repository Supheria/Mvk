namespace MvkServer.Util
{
    /// <summary>
    /// Блок для мгновенного тика
    /// </summary>
    public struct TickBlock
    {
        /// <summary>
        /// Позиция блока в чанке X
        /// </summary>
        public int x;
        /// <summary>
        /// Позиция блока в чанке Y
        /// </summary>
        public int y;
        /// <summary>
        /// Позиция блока в чанке Z
        /// </summary>
        public int z;
        /// <summary>
        /// Время когда этот блок должен сработать
        /// </summary>
        public uint scheduledTime;
        /// <summary>
        /// Приоритет
        /// </summary>
        public bool priority;

        public override string ToString()
        {
            return string.Format("{0},{1},{2} = {3}", x, y, z, scheduledTime);
        }
    }
}
