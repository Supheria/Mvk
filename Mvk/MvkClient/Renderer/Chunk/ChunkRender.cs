using MvkClient.Renderer.Block;
using MvkClient.Util;
using MvkClient.World;
using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Block;
using MvkServer.World.Chunk;
using System;
using System.Collections.Generic;

namespace MvkClient.Renderer.Chunk
{
    /// <summary>
    /// Объект рендера чанка
    /// </summary>
    public class ChunkRender : ChunkBase
    {
        /// <summary>
        /// Клиентский объект мира
        /// </summary>
        public WorldClient ClientWorld { get; protected set; }

        /// <summary>
        /// Сетка чанка сплошных блоков
        /// </summary>
        private ChunkMesh meshDense = new ChunkMesh();
        /// <summary>
        /// Буфер сетки секций чанков
        /// </summary>
        private readonly ChunkSectionMesh[] meshSectionDense = new ChunkSectionMesh[COUNT_HEIGHT];
        /// <summary>
        /// Массив какие секции сплошных чанков надо рендерить, для потока где идёт рендер
        /// </summary>
        private readonly bool[] isRenderingSectionDense = new bool[COUNT_HEIGHT];
        /// <summary>
        /// Сетка чанка альфа блоков
        /// </summary>
        private readonly ChunkMesh meshAlpha = new ChunkMesh();
        /// <summary>
        /// Массив блоков которые разрушаются
        /// </summary>
        private List<DestroyBlockProgress> destroyBlocks = new List<DestroyBlockProgress>();
        /// <summary>
        /// Количество альфа блоков в чанке
        /// </summary>
        private int countAlpha;
        /// <summary>
        /// Количество альфа блоков в секции
        /// </summary>
        private readonly int[] countSectionAlpha = new int[COUNT_HEIGHT];
        /// <summary>
        /// Соседние чанки, заполняются перед рендером
        /// </summary>
        private readonly ChunkRender[] chunks = new ChunkRender[8];

        public ChunkRender(WorldClient worldIn, vec2i pos) : base(worldIn, pos)
        {
            ClientWorld = worldIn;
            for (int y = 0; y < COUNT_HEIGHT; y++)
            {
                meshSectionDense[y] = new ChunkSectionMesh();
                meshSectionDense[y].Init();
                isRenderingSectionDense[y] = false;
                countSectionAlpha[y] = 0;
            }
        }

        /// <summary>
        /// Пометить что надо перерендерить сетку чанка
        /// </summary>
        public override void ModifiedToRender(int y)
        {
            meshDense.SetModifiedRender();
            if (y >= 0 && y < COUNT_HEIGHT) meshSectionDense[y].isModifiedRender = true;
        }

        /// <summary>
        /// Проверка, нужен ли рендер этого псевдо чанка
        /// </summary>
        public bool IsModifiedRender() => meshDense.IsModifiedRender;

        /// <summary>
        /// Пометить что надо перерендерить сетку чанка альфа блоков
        /// </summary>
        public bool ModifiedToRenderAlpha(int y)
        {
            //if (countAlpha > 0)
            if (y >= 0 && y < COUNT_HEIGHT && countSectionAlpha[y] > 0)
            {
                meshAlpha.SetModifiedRender();
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Проверка, нужен ли рендер этого псевдо чанка  альфа блоков
        /// </summary>
        public bool IsModifiedRenderAlpha() => meshAlpha.IsModifiedRender;

        /// <summary>
        /// Удалить сетки
        /// </summary>
        public void MeshDelete()
        {
            meshDense.Delete();
            meshAlpha.Delete();
            countAlpha = 0;
            for (int y = 0; y < COUNT_HEIGHT; y++)
            {
                meshSectionDense[y].Clear();
                countSectionAlpha[y] = 0;
                isRenderingSectionDense[y] = false;
            }
        }

        /// <summary>
        /// Старт рендеринга
        /// </summary>
        public void StartRendering()
        {
            meshDense.StatusRendering();
            for (int y = 0; y < COUNT_HEIGHT; y++)
            {
                isRenderingSectionDense[y] = meshSectionDense[y].isModifiedRender;
                meshSectionDense[y].isModifiedRender = false;
            }
            meshAlpha.StatusRendering();
        }

        /// <summary>
        /// Старт рендеринга только альфа
        /// </summary>
        public void StartRenderingAlpha() => meshAlpha.StatusRendering();

        /// <summary>
        /// Заполнить буфе боковых чанков
        /// </summary>
        public void UpBufferChunks()
        {
            for (int i = 0; i < 8; i++)
            {
                chunks[i] = World.ChunkPr.GetChunk(Position + MvkStatic.AreaOne8[i]) as ChunkRender;
            }
        }

        /// <summary>
        /// Очистить буфер соседних чанков
        /// </summary>
        public void ClearBufferChunks()
        {
            for (int i = 0; i < 8; i++) chunks[i] = null;
        }

        /// <summary>
        /// Получить соседний чанк, где x и y -1..1
        /// </summary>
        public ChunkRender Chunk(int x, int y) => chunks[MvkStatic.GetAreaOne8(x, y)];

        /// <summary>
        /// Рендер псевдо чанка, сплошных и альфа блоков
        /// </summary>
        public void Render(bool isDense)
        {
            try
            {
                long timeBegin = GLWindow.stopwatch.ElapsedTicks;
                ChunkStorage chunkStorage;
                // буфер альфа блоков
                List<BlockBuffer> alphas = new List<BlockBuffer>();
                vec3i posPlayer = ClientWorld.ClientMain.Player.PositionAlphaBlock;
                ushort data, id, met;
                int cbX = Position.x << 4;
                int cbZ = Position.y << 4;
                int cbY, realY, realZ, i, yb;
                
                BlockAlphaRender blockAlphaRender = new BlockAlphaRender(this);
                BlockRender blockRender = new BlockRender(this);
                blockAlphaRender.blockUV.buffer = ClientWorld.WorldRender.bufferAlphaCache;
                blockRender.blockUV.buffer = ClientWorld.WorldRender.buffer;
                blockAlphaRender.blockUV.bufferCache = blockRender.blockUV.bufferCache 
                    = ClientWorld.WorldRender.bufferCache;
                bool isDense2;
                int cAlpha = 0;
                try
                {
                    for (cbY = 0; cbY < COUNT_HEIGHT; cbY++)
                    //for (cbY = 4; cbY < COUNT_HEIGHT; cbY++)
                    {
                        isDense2 = isRenderingSectionDense[cbY];
                        if (isDense2 || countSectionAlpha[cbY] > 0)
                        {
                            chunkStorage = StorageArrays[cbY];
                            if (chunkStorage.data != null && !chunkStorage.IsEmptyData())
                            {
                                for (yb = 0; yb < 16; yb++)
                                {
                                    realY = cbY << 4 | yb;

                                    for (int z = 0; z < 16; z++)
                                    {

                                        for (int x = 0; x < 16; x++)
                                        {
                                            i = yb << 8 | z << 4 | x;
                                            data = chunkStorage.data[i];
                                            if (data == 0 || data == 4096) continue;
                                            id = (ushort)(data & 0xFFF);
                                            met = Blocks.blocksAddMet[id] ? chunkStorage.addMet[(ushort)i] : (ushort)(data >> 12);

                                            blockRender.block = Blocks.blocksInt[id];

                                            if (blockRender.block.Translucent)
                                            {
                                                // Альфа!
                                                blockAlphaRender.blockState.id = id;
                                                blockAlphaRender.met = blockAlphaRender.blockState.met = met;
                                                blockAlphaRender.blockState.lightBlock = chunkStorage.lightBlock[i];
                                                blockAlphaRender.blockState.lightSky = chunkStorage.lightSky[i];
                                                blockAlphaRender.posChunkX = x;
                                                blockAlphaRender.posChunkY = realY;
                                                blockAlphaRender.posChunkZ = z;
                                                blockAlphaRender.posChunkY0 = realY;
                                                blockAlphaRender.block = blockRender.block;
                                                blockAlphaRender.RenderMesh();
                                                if (blockAlphaRender.blockUV.buffer.count > 0)
                                                {
                                                    cAlpha++;
                                                    realZ = cbZ | z;
                                                    alphas.Add(new BlockBuffer()
                                                    {
                                                        buffer = blockAlphaRender.blockUV.buffer.ToArray(),
                                                        distance = glm.distance(posPlayer, new vec3i(cbX | x, realY, realZ))
                                                    });
                                                    blockAlphaRender.blockUV.buffer.Clear();
                                                }
                                            }
                                            else if (isDense)
                                            {
                                                // Сплошной блок
                                                blockRender.blockState.id = id;
                                                blockRender.met = blockRender.blockState.met = met;
                                                blockRender.blockState.lightBlock = chunkStorage.lightBlock[i];
                                                blockRender.blockState.lightSky = chunkStorage.lightSky[i];
                                                blockRender.posChunkX = x;
                                                blockRender.posChunkY0 = realY;
                                                blockRender.posChunkY = realY;
                                                blockRender.posChunkZ = z;
                                                blockRender.RenderMesh();
                                            }
                                        }
                                    }
                                }
                            }
                            if (isDense2 && isDense)
                            {
                                countSectionAlpha[cbY] = cAlpha;
                                meshSectionDense[cbY].bufferData = ClientWorld.WorldRender.buffer.ToArray();
                                ClientWorld.WorldRender.buffer.Clear();
                            }
                        }
                        cAlpha = 0;
                        if (isDense)
                        {
                            ClientWorld.WorldRender.bufferHeight.Combine(meshSectionDense[cbY].bufferData);
                        }
                    }
                }
                catch (Exception ex)
                {
                    string s = ex.Message;
                }

                if (isDense)
                {
                    meshDense.SetBuffer(ClientWorld.WorldRender.bufferHeight.ToArray());
                    ClientWorld.WorldRender.bufferHeight.Clear();
                }
                
                countAlpha = alphas.Count;
                ToBufferAlphaY(alphas);
                meshAlpha.SetBuffer(ClientWorld.WorldRender.buffer.ToArray());
                ClientWorld.WorldRender.buffer.Clear();

                // Для отладочной статистики
                float time = (GLWindow.stopwatch.ElapsedTicks - timeBegin) / (float)MvkStatic.TimerFrequency;
                if (isDense) 
                {
                    Debug.RenderChunckTime8 = (Debug.RenderChunckTime8 * 7f + time) / 8f;
                }
                else
                {
                    Debug.RenderChunckTimeAlpha8 = (Debug.RenderChunckTimeAlpha8 * 7f + time) / 8f;
                }
            }
            catch (Exception ex)
            {
                Logger.Crach(ex);
            }
        }

        /// <summary>
        /// Вернуть массив буфера альфа
        /// </summary>
        public void ToBufferAlphaY(List<BlockBuffer> alphas)
        {
            int count = alphas.Count;
            if (count > 0)
            {
                alphas.Sort();
                for (int i = count - 1; i >= 0; i--)
                {
                    ClientWorld.WorldRender.buffer.Combine(alphas[i].buffer);
                }
            }
        }

        /// <summary>
        /// Прорисовка сплошных блоков псевдо чанка
        /// </summary>
        public void DrawDense() => meshDense.Draw();
        /// <summary>
        /// Прорисовка альфа блоков псевдо чанка
        /// </summary>
        public void DrawAlpha() => meshAlpha.Draw();

        /// <summary>
        /// Занести буфер сплошных блоков псевдо чанка если это требуется
        /// </summary>
        public void BindBufferDense() => meshDense.BindBuffer();
        //// <summary>
        /// Занести буфер альфа блоков псевдо чанка если это требуется
        /// </summary>
        public void BindBufferAlpha() => meshAlpha.BindBuffer();

        #region DestroyBlock

        /// <summary>
        /// Занести разрушение блока
        /// </summary>
        /// <param name="breakerId">Id сущности игрока</param>
        /// <param name="blockPos">позиция блока</param>
        /// <param name="progress">процесс разрушения</param>
        public void DestroyBlockSet(int breakerId, BlockPos blockPos, int progress)
        {
            DestroyBlockProgress destroy = null;
            for (int i = 0; i < destroyBlocks.Count; i++)
            {
                if (destroyBlocks[i].BreakerId == breakerId)
                {
                    destroy = destroyBlocks[i];
                    break;
                }
            }
            if (destroy == null)
            {
                destroy = new DestroyBlockProgress(breakerId, blockPos);
                destroyBlocks.Add(destroy);
            }
            destroy.SetPartialBlockDamage(progress);
            destroy.SetCloudUpdateTick(ClientWorld.ClientMain.TickCounter);
        }

        /// <summary>
        /// Удалить разрушение блока
        /// </summary>
        /// <param name="breakerId">Id сущности игрока</param>
        public void DestroyBlockRemove(int breakerId)
        {
            for (int i = destroyBlocks.Count - 1; i >= 0; i--)
            {
                if (destroyBlocks[i].BreakerId == breakerId)
                {
                    destroyBlocks.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Проверить есть ли на тикущем блоке разрушение
        /// </summary>
        /// <param name="x">локальная позиция X блока 0..15</param>
        /// <param name="y">локальная позиция Y блока</param>
        /// <param name="z">локальная позиция Z блока 0..15</param>
        /// <returns>-1 нет разрушения, 0-9 разрушение</returns>
        public int GetDestroyBlocksValue(int x, int y, int z)
        {
            if (destroyBlocks.Count > 0)
            {
                for (int i = 0; i < destroyBlocks.Count; i++)
                {
                    DestroyBlockProgress destroy = destroyBlocks[i];
                    if (destroy.Position.EqualsPosition0(x, y, z))
                    {
                        return destroy.PartialBlockProgress;
                    }
                }
            }
            return -1;
        }

        #endregion

        #region Status

        /// <summary>
        /// Статсус возможности для рендера сплошных блоков
        /// </summary>
        public bool IsMeshDenseWait() => meshDense.Status == ChunkMesh.StatusMesh.Wait || meshDense.Status == ChunkMesh.StatusMesh.Null;
        /// <summary>
        /// Статсус возможности для рендера альфа блоков
        /// </summary>
        public bool IsMeshAlphaWait() => meshAlpha.Status == ChunkMesh.StatusMesh.Wait || meshAlpha.Status == ChunkMesh.StatusMesh.Null;
        /// <summary>
        /// Статсус связывания сетки с OpenGL для рендера сплошных блоков
        /// </summary>
        public bool IsMeshDenseBinding() => meshDense.Status == ChunkMesh.StatusMesh.Binding;
        /// <summary>
        /// Статсус связывания сетки с OpenGL для рендера альфа блоков
        /// </summary>
        public bool IsMeshAlphaBinding() => meshAlpha.Status == ChunkMesh.StatusMesh.Binding;
        /// <summary>
        /// Статсус не пустой для рендера сплошных блоков
        /// </summary>
        public bool NotNullMeshDense() => meshDense.Status != ChunkMesh.StatusMesh.Null;
        /// <summary>
        /// Статсус не пустой для рендера альфа блоков
        /// </summary>
        public bool NotNullMeshAlpha() => meshAlpha.Status != ChunkMesh.StatusMesh.Null;
        /// <summary>
        /// Изменить статус на отмена рендеринга альфа блоков
        /// </summary>
        public void NotRenderingAlpha() => meshAlpha.NotRendering();

        #endregion
    }
}
