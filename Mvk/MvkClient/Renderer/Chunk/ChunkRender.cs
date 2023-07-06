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
        public WorldClient ClientWorld { get; private set; }

        /// <summary>
        /// Сетка чанка сплошных блоков
        /// </summary>
        private readonly ChunkMesh meshDense = new ChunkMesh();
        /// <summary>
        /// Сетка чанков уникальных блоков
        /// </summary>
        private readonly ChunkMesh meshUnique = new ChunkMesh();
        /// <summary>
        /// Сетка чанков уникальных блоков
        /// </summary>
        private readonly ChunkMesh meshUniqueBothSides = new ChunkMesh();
        /// <summary>
        /// Сетка чанка альфа блоков
        /// </summary>
        private readonly ChunkMesh meshAlpha = new ChunkMesh();
        /// <summary>
        /// Буфер сетки секций чанков сплошных блоков
        /// </summary>
        private readonly ChunkSectionMesh[] meshSectionDense = new ChunkSectionMesh[COUNT_HEIGHT];
        /// <summary>
        /// Массив какие секции сплошных чанков надо рендерить, для потока где идёт рендер
        /// </summary>
        private readonly bool[] isRenderingSectionDense = new bool[COUNT_HEIGHT];
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
            meshUnique.Delete();
            meshUniqueBothSides.Delete();
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
                int cbY, realY, realZ, index, yb, x, z;

                BlockBase block;
                BlockRenderBase blockRender;
                if (isDense)
                {
                    ClientWorld.WorldRender.InitChunkRender(this);
                }
                ClientWorld.WorldRender.InitChunkRenderAlpha(this);

                // Флаг нужен ли в сплошных блоках в текущей секции рендер
                bool isDenseSection;
                int cAlpha = 0;

                for (cbY = 0; cbY < COUNT_HEIGHT; cbY++)
                {
                    isDenseSection = isRenderingSectionDense[cbY] && isDense;
                    if (isDenseSection || countSectionAlpha[cbY] > 0)
                    {
                        // Нужен ли в текущей секции рендер, от сплошных или альфа блоков
                        chunkStorage = StorageArrays[cbY];
                        if (chunkStorage.data != null && !chunkStorage.IsEmptyData())
                        {
                            // Имекется хоть один блок
                            for (yb = 0; yb < 16; yb++)
                            {
                                realY = cbY << 4 | yb;
                                for (z = 0; z < 16; z++)
                                {
                                    for (x = 0; x < 16; x++)
                                    {
                                        index = yb << 8 | z << 4 | x;
                                        data = chunkStorage.data[index];
                                        // Если блок воздуха, то пропускаем рендер сразу
                                        if (data == 0 || data == 4096) continue;
                                        // Определяем id блока
                                        id = (ushort)(data & 0xFFF);
                                        // Определяем met блока
                                        met = Blocks.blocksAddMet[id] ? chunkStorage.addMet[(ushort)index] : (ushort)(data >> 12);
                                        // Определяем объект блока
                                        block = Blocks.blocksInt[id];

                                        if (block.Translucent)
                                        {
                                            // Рендер альфа блоков
                                            if (block.IsUnique)
                                            {
                                                // Уникальный блок
                                                blockRender = ClientWorld.WorldRender.blockAlphaRenderUnique;
                                            }
                                            else if (block.FullBlock)
                                            {
                                                // Сплошной блок
                                                blockRender = ClientWorld.WorldRender.blockAlphaRenderFull;
                                            }
                                            else
                                            {
                                                // Жидкость
                                                blockRender = ClientWorld.WorldRender.blockAlphaRenderLiquid;
                                            }
                                            blockRender.blockState.id = id;
                                            blockRender.met = blockRender.blockState.met = met;
                                            blockRender.blockState.lightBlock = chunkStorage.lightBlock[index];
                                            blockRender.blockState.lightSky = chunkStorage.lightSky[index];
                                            blockRender.posChunkX = x;
                                            blockRender.posChunkY = realY;
                                            blockRender.posChunkZ = z;
                                            blockRender.block = block;
                                            blockRender.RenderMesh();

                                            if (ClientWorld.WorldRender.bufferAlphaCache.count > 0)
                                            {
                                                // Если имеются данные значит имеются альфа блоки
                                                cAlpha++;
                                                realZ = cbZ | z;
                                                alphas.Add(new BlockBuffer()
                                                {
                                                    buffer = ClientWorld.WorldRender.bufferAlphaCache.ToArray(),
                                                    distance = glm.distance(posPlayer, new vec3i(cbX | x, realY, realZ))
                                                });
                                                ClientWorld.WorldRender.bufferAlphaCache.Clear();
                                            }
                                        }
                                        else if (isDense)
                                        {
                                            // Рендер сплошных, не прозрачных блоков
                                            if (block.IsUnique)
                                            {
                                                // Уникальный блок
                                                blockRender = block.BothSides 
                                                    ? ClientWorld.WorldRender.blockRenderUniqueBothSides
                                                    : ClientWorld.WorldRender.blockRenderUnique;
                                            }
                                            else if (block.FullBlock)
                                            {
                                                // Сплошной блок
                                                blockRender = ClientWorld.WorldRender.blockRenderFull;
                                            }
                                            else
                                            {
                                                // Жидкость
                                                blockRender = ClientWorld.WorldRender.blockRenderLiquid;
                                            }
                                            blockRender.blockState.id = id;
                                            blockRender.met = blockRender.blockState.met = met;
                                            blockRender.blockState.lightBlock = chunkStorage.lightBlock[index];
                                            blockRender.blockState.lightSky = chunkStorage.lightSky[index];
                                            blockRender.posChunkX = x;
                                            blockRender.posChunkY = realY;
                                            blockRender.posChunkZ = z;
                                            blockRender.block = block;
                                            blockRender.RenderMesh();
                                        }
                                    }
                                }
                            }
                            if (isDense)
                            {
                                // Если работаем со сплашными блоками, обновляем буферы секций
                                countSectionAlpha[cbY] = cAlpha;
                                if (isDenseSection)
                                {
                                    countSectionAlpha[cbY] = cAlpha;
                                    meshSectionDense[cbY].bufferData = ClientWorld.WorldRender.bufferDense.ToArray();
                                    meshSectionDense[cbY].bufferDataUnique = ClientWorld.WorldRender.bufferUnique.ToArray();
                                    meshSectionDense[cbY].bufferDataUniqueBothSides = ClientWorld.WorldRender.bufferUniqueBothSides.ToArray();
                                }
                                ClientWorld.WorldRender.bufferDense.Clear();
                                ClientWorld.WorldRender.bufferUnique.Clear();
                                ClientWorld.WorldRender.bufferUniqueBothSides.Clear();
                            }
                        }
                        else
                        {
                            // Нет блоков, все блоки воздуха очищаем буфера
                            meshSectionDense[cbY].bufferData = meshSectionDense[cbY].bufferDataUnique 
                                = meshSectionDense[cbY].bufferDataUniqueBothSides = new byte[0];
                        }
                    }
                    cAlpha = 0;
                    if (isDense)
                    {
                        // Если работаем со сплашными блоками, клеим секции
                        ClientWorld.WorldRender.bufferHeight.Combine(meshSectionDense[cbY].bufferData);
                        ClientWorld.WorldRender.bufferHeightUnique.Combine(meshSectionDense[cbY].bufferDataUnique);
                        ClientWorld.WorldRender.bufferHeightUniqueBothSides.Combine(meshSectionDense[cbY].bufferDataUniqueBothSides);
                    }
                }

                if (isDense)
                {
                    // Если работаем со сплашными блоками, вносим буфер
                    meshUnique.SetBuffer(ClientWorld.WorldRender.bufferHeightUnique.ToArray());
                    ClientWorld.WorldRender.bufferHeightUnique.Clear();
                    meshUniqueBothSides.SetBuffer(ClientWorld.WorldRender.bufferHeightUniqueBothSides.ToArray());
                    ClientWorld.WorldRender.bufferHeightUniqueBothSides.Clear();
                    meshDense.SetBuffer(ClientWorld.WorldRender.bufferHeight.ToArray());
                    ClientWorld.WorldRender.bufferHeight.Clear();
                }
                // Вносим буфер для альфа
                countAlpha = alphas.Count;
                ToBufferAlphaY(alphas);
                meshAlpha.SetBuffer(ClientWorld.WorldRender.bufferAlpha.ToArray());
                ClientWorld.WorldRender.bufferAlpha.Clear();

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
                    ClientWorld.WorldRender.bufferAlpha.Combine(alphas[i].buffer);
                }
            }
        }

        /// <summary>
        /// Прорисовка сплошных блоков псевдо чанка
        /// </summary>
        public void DrawDense() => meshDense.Draw();
        /// <summary>
        /// Прорисовка уникальных блоков псевдо чанка
        /// </summary>
        public void DrawUnique() => meshUnique.Draw();
        /// <summary>
        /// Прорисовка уникальных блоков псевдо чанка
        /// </summary>
        public void DrawUniqueBothSides() => meshUniqueBothSides.Draw();
        /// <summary>
        /// Прорисовка альфа блоков псевдо чанка
        /// </summary>
        public void DrawAlpha() => meshAlpha.Draw();

        /// <summary>
        /// Занести буфер сплошных блоков псевдо чанка если это требуется
        /// </summary>
        public void BindBufferDense()
        {
            meshUnique.BindBuffer();
            meshUniqueBothSides.BindBuffer();
            meshDense.BindBuffer();
        }
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
        /// Статсус не пустой для рендера уникальных блоков
        /// </summary>
        public bool NotNullMeshUnique() => meshUnique.Status != ChunkMesh.StatusMesh.Null;
        /// <summary>
        /// Статсус не пустой для рендера уникальных блоков
        /// </summary>
        public bool NotNullMeshUniqueBothSides() => meshUniqueBothSides.Status != ChunkMesh.StatusMesh.Null;
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
