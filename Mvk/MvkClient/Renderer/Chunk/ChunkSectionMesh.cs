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
        /// Инициализировать объект
        /// </summary>
        public void Init() => Clear();

        /// <summary>
        /// Очистить буфер
        /// </summary>
        public void Clear()
        {
            bufferData = new byte[] { };
            isModifiedRender = false;
        }
    }
}

