using MvkClient.Renderer.Chunk;

namespace MvkClient.Renderer.Block
{
    /// <summary>
    /// Объект рендера альфа блоков
    /// </summary>
    public class BlockAlphaRender : BlockRender
    {
        /// <summary>
        /// Создание блока генерации для мира
        /// </summary>
        public BlockAlphaRender(ChunkRender chunkRender) : base(chunkRender) { }
    }
}
