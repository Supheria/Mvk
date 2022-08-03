using MvkServer.Glm;

namespace MvkClient.World
{
    /// <summary>
    /// Структура очереди чанка
    /// </summary>
    public struct ChunkQueue
    {
        public vec2i pos;
        public byte[] buffer;
        public int flagsYAreas;
        public bool biom;

        /// <summary>
        /// Буффер псевдо чанка
        /// </summary>
        public byte[] GetBuffer() => buffer;
        /// <summary>
        /// Позиция
        /// </summary>
        public vec2i GetPos() => pos;
        /// <summary>
        /// Удалить чанк
        /// </summary>
        public bool IsRemoved() => flagsYAreas == 0;
        /// <summary>
        /// Данные столбца биома, как правило при первой загрузке
        /// </summary>
        public bool IsBiom() => biom;
        /// <summary>
        /// Флаг псевдо чанков
        /// </summary>
        public int GetFlagsYAreas() => flagsYAreas;
    }
}
