namespace MvkClient.Renderer.Chunk
{
    /// <summary>
    /// Объект секции буфер сетки чанка
    /// </summary>
    public struct ChunkSectionMesh
    {
        /// <summary>
        /// Пометка изменения
        /// </summary>
        public bool isModifiedRender;
        /// <summary>
        /// Объект буфера
        /// </summary>
        public byte[] bufferData;

        /// <summary>
        /// Объект буфера уникальных блоков
        /// </summary>
        public byte[] bufferDataUnique;

        /// <summary>
        /// Объект буфера уникальных блоков с прорисовкой с двух сторон
        /// </summary>
        public byte[] bufferDataUniqueBothSides;

        /// <summary>
        /// Инициализировать объект
        /// </summary>
        public void Init() => Clear();

        /// <summary>
        /// Очистить буфер
        /// </summary>
        public void Clear()
        {
            bufferData = new byte[] { };
            bufferDataUnique = new byte[] { };
            bufferDataUniqueBothSides = new byte[] { };
            isModifiedRender = false;
        }
    }
}

