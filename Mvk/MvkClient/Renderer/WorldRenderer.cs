﻿using MvkAssets;
using MvkClient.Entity;
using MvkClient.Gui;
using MvkClient.Renderer.Block;
using MvkClient.Renderer.Chunk;
using MvkClient.Renderer.Shaders;
using MvkClient.Util;
using MvkClient.World;
using MvkServer;
using MvkServer.Entity;
using MvkServer.Glm;
using MvkServer.Item;
using MvkServer.Util;
using MvkServer.World;
using MvkServer.World.Block;
using SharpGL;
using System;
using System.Collections.Generic;
using System.Threading;

namespace MvkClient.Renderer
{
    /// <summary>
    /// Объект рендера мира
    /// </summary>
    public class WorldRenderer
    {
        /// <summary>
        /// Основной клиент
        /// </summary>
        public Client ClientMain { get; private set; }
        /// <summary>
        /// Клиентский объект мира
        /// </summary>
        public WorldClient World { get; private set; }
        /// <summary>
        /// Объект основного игрового скрина
        /// </summary>
        public ScreenInGame ScreenGame { get; private set; }

        /// <summary>
        /// Буфер сплошных блоков всего ряда (8)
        /// </summary>
        public readonly ArrayMvk<byte> bufferHeight = new ArrayMvk<byte>(66060288);
        /// <summary>
        /// Буфер уникальных блоков всего ряда (8)
        /// </summary>
        public readonly ArrayMvk<byte> bufferHeightUnique = new ArrayMvk<byte>(66060288);
        /// <summary>
        /// Буфер уникальных блоков с обоих сторон всего ряда (8)
        /// </summary>
        public readonly ArrayMvk<byte> bufferHeightUniqueBothSides = new ArrayMvk<byte>(66060288);
        /// <summary>
        /// Буфер альфа блоков одного сектора
        /// </summary>
        public readonly ArrayMvk<byte> bufferAlpha = new ArrayMvk<byte>(4128768);
        /// <summary>
        /// Буфер сплошных блоков одного сектора
        /// </summary>
        public readonly ArrayMvk<byte> bufferDense = new ArrayMvk<byte>(4128768);
        /// <summary>
        /// Буфер уникальных блоков одного сектора
        /// </summary>
        public readonly ArrayMvk<byte> bufferUnique = new ArrayMvk<byte>(4128768);
        /// <summary>
        /// Буфер уникальных блоков с обоих сторон одного сектора
        /// </summary>
        public readonly ArrayMvk<byte> bufferUniqueBothSides = new ArrayMvk<byte>(4128768);
        /// <summary>
        /// Буфер одного кэш блока
        /// </summary>
        public readonly ArrayMvk<byte> bufferCache = new ArrayMvk<byte>(6048);
        /// <summary>
        /// Буефр одного альфа кэш блока
        /// </summary>
        public readonly ArrayMvk<byte> bufferAlphaCache = new ArrayMvk<byte>(4032);
        /// <summary>
        /// Объект рендера целого блока с эффектом размытия (Mipmap)
        /// </summary>
        public readonly BlockRenderFull blockRenderFull = new BlockRenderFull();
        /// <summary>
        /// Объект рендера уникального блока с эффектом контраста (Not Mipmap)
        /// </summary>
        public readonly BlockRenderUnique blockRenderUnique = new BlockRenderUnique();
        /// <summary>
        /// Объект рендера уникального блока с эффектом контраста c двух сторон (Not Mipmap && Disable(GL_CULL_FACE))
        /// </summary>
        public readonly BlockRenderUnique blockRenderUniqueBothSides = new BlockRenderUnique();
        /// <summary>
        /// Объект рендера жидкого блока с эффектом контраста (Not Mipmap)
        /// </summary>
        public readonly BlockRenderLiquid blockRenderLiquid = new BlockRenderLiquid();
        /// <summary>
        /// Объект рендера целого Alpha блока с эффектом контраста (Not Mipmap)
        /// </summary>
        public readonly BlockRenderFull blockAlphaRenderFull = new BlockRenderFull();
        /// <summary>
        /// Объект рендера уникального Alpha блока с эффектом контраста (Not Mipmap)
        /// </summary>
        public readonly BlockRenderUnique blockAlphaRenderUnique = new BlockRenderUnique();
        /// <summary>
        /// Объект рендера жидкого Alpha блока с эффектом контраста (Not Mipmap)
        /// </summary>
        public readonly BlockRenderLiquid blockAlphaRenderLiquid = new BlockRenderLiquid();

        /// <summary>
        /// Дополнительный счётчик, для повторной проверки, если камера не двигается, а чанки догружаются
        /// Возможноcть обрабатывать только структуру чанка чтоб догрузить в FC чанк
        /// </summary>
        private int addInitFrustumCulling = 0;
        /// <summary>
        /// DL курсора блока
        /// </summary>
        private RenderBlockCursor renderBlockCursor;
        /// <summary>
        /// DL курсора чанков
        /// </summary>
        private RenderChunkCursor renderChunkCursor;
        
        /// <summary>
        /// id DL звёзд
        /// </summary>
        private uint dlStar = 0;
        /// <summary>
        /// Карта всех блоков для GUI
        /// </summary>
        private readonly Dictionary<EnumBlock, RenderBlockGui> mapBlocksGui = new Dictionary<EnumBlock, RenderBlockGui>();
        /// <summary>
        /// Карта всех предметов для GUI
        /// </summary>
        private readonly Dictionary<EnumItem, RenderItemGui> mapItemsGui = new Dictionary<EnumItem, RenderItemGui>();

        /// <summary>
        /// Угол солнца
        /// </summary>
        private float celestialAngle;
        /// <summary>
        /// Цвет тумана
        /// </summary>
        private vec3 colorFog;
        /// <summary>
        /// Яркость неба
        /// </summary>
        private float skyLight;
        /// <summary>
        /// Яркость солнца
        /// </summary>
        private float sunLight;
        /// <summary>
        /// Текстурная карта освещения
        /// </summary>
        private readonly TextureLightMap textureLightMap = new TextureLightMap();

        /// <summary>
        /// Флаг потока рендера чанков
        /// </summary>
        private bool renderLoopRunning = false;
        /// <summary>
        /// Массив очередей чанков для рендера
        /// </summary>
        private readonly DoubleList<ChunkRender> renderQueues = new DoubleList<ChunkRender>();
        /// <summary>
        /// Массив очередей чанков для рендера альфа
        /// </summary>
        private readonly DoubleList<ChunkRender> renderAlphaQueues = new DoubleList<ChunkRender>();
        /// <summary>
        /// Метод чанков для прорисовки
        /// </summary>
        private readonly ArrayMvk<ChunkRender> arrayChunkRender = new ArrayMvk<ChunkRender>(6000);

        public WorldRenderer(WorldClient world)
        {
            World = world;
            ClientMain = world.ClientMain;
            ScreenGame = new ScreenInGame(ClientMain);

            // Инициализация буффер массивов для рендера блоков
            blockRenderFull.blockUV.buffer = blockRenderLiquid.blockUV.buffer = bufferDense;
            blockRenderUnique.blockUV.buffer = bufferUnique;
            blockRenderUniqueBothSides.blockUV.buffer = bufferUniqueBothSides;

            blockAlphaRenderFull.blockUV.buffer = blockAlphaRenderUnique.blockUV.buffer 
                = blockAlphaRenderLiquid.blockUV.buffer = bufferAlphaCache;

            blockRenderFull.blockUV.bufferCache = blockRenderUnique.blockUV.bufferCache 
                = blockRenderUniqueBothSides.blockUV.bufferCache = blockRenderLiquid.blockUV.bufferCache 
                = blockAlphaRenderFull.blockUV.bufferCache = blockAlphaRenderUnique.blockUV.bufferCache 
                = blockAlphaRenderLiquid.blockUV.bufferCache = bufferCache;

            renderBlockCursor = new RenderBlockCursor(ClientMain);
            renderChunkCursor = new RenderChunkCursor { IsHidden = true };

            for (int i = 0; i <= BlocksCount.COUNT; i++)
            {
                EnumBlock enumBlock = (EnumBlock)i;
                RenderBlockGui renderBlock = new RenderBlockGui(enumBlock);
                mapBlocksGui.Add(enumBlock, renderBlock);
                renderBlock.Render();
            }
            for (int i = 1; i <= ItemsCount.COUNT; i++)
            {
                EnumItem enumItem = (EnumItem)i;
                RenderItemGui renderItem = new RenderItemGui(enumItem);
                mapItemsGui.Add(enumItem, renderItem);
                renderItem.Render();
            }

            
            // Отладочный блок
            //  mapBlocksGui.Add(EnumBlock.Debug, new RenderBlockGui(EnumBlock.Debug));

            // создаём DL звёзд
            RenderStar();
            // создаём DL неба
            //RenderClouds();

            // Запускаем отдельный поток для рендера
            renderLoopRunning = true;
            new Thread(RenderLoop).Start();
        }

        /// <summary>
        /// Инициализация объектов генерации блоков
        /// </summary>
        public void InitChunkRender(ChunkRender chunkRender)
        {
            blockRenderFull.InitChunk(chunkRender);
            blockRenderUnique.InitChunk(chunkRender);
            blockRenderUniqueBothSides.InitChunk(chunkRender);
            blockRenderLiquid.InitChunk(chunkRender);
        }

        /// <summary>
        /// Инициализация объектов генерации альфа блоков
        /// </summary>
        public void InitChunkRenderAlpha(ChunkRender chunkRender)
        {
            blockAlphaRenderFull.InitChunk(chunkRender);
            blockAlphaRenderUnique.InitChunk(chunkRender);
            blockAlphaRenderLiquid.InitChunk(chunkRender);
        }

        /// <summary>
        /// Остановить поток рендера чанков
        /// </summary>
        public void StopRender() => renderLoopRunning = false;

        /// <summary>
        /// Поток рендера чанков
        /// </summary>
        private void RenderLoop()
        {
            try
            {
                while (renderLoopRunning && World != null)
                {
                    RenderQueues(true, renderQueues);
                    RenderQueues(false, renderAlphaQueues);
                    Thread.Sleep(1);
                }
            }
            catch (Exception ex)
            {
                Logger.Crach(ex);
            }
        }

        /// <summary>
        /// Запуск рендера чанка из очередей в отдельном потоке
        /// </summary>
        /// <param name="isDense">Флаг сплошных блоков</param>
        /// <param name="list">Список очередей</param>
        private void RenderQueues(bool isDense, DoubleList<ChunkRender> list)
        {
            int count, i;
            ChunkRender renderQueue;
            list.Step();
            count = list.CountBackward;
            for (i = 0; i < count; i++)
            {
                renderQueue = list.GetNext();
                renderQueue.UpBufferChunks();
                renderQueue.Render(isDense);
                renderQueue.ClearBufferChunks();
                if (!renderLoopRunning) break;
            }
        }

        /// <summary>
        /// Прорисовка мира
        /// </summary>
        public void Draw(float timeIndex)
        {
            celestialAngle = World.CalculateCelestialAngle(timeIndex);
            sunLight = World.GetSunLight(celestialAngle);
            skyLight = World.GetSkyLight(celestialAngle);
            colorFog = World.GetFogColor(skyLight);

            textureLightMap.Update(sunLight, MvkStatic.LightMoonPhase[World.GetIndexMoonPhase()]);

            // Обновить кадр основного игрока, камера и прочее
            ClientMain.Player.UpdateFrame(timeIndex);

            if (ClientMain.Player.View == null) ClientMain.Player.UpView(timeIndex);

            // Матрица камеры
            ClientMain.Player.MatrixProjection(0);

            // Небо
            DrawSky(timeIndex);

            // Биндим чанки вокселей и готовим массив чанков
            BindChunkVoxel();

            // Рисуем воксели сплошных блоков VBO
            DrawVoxelDense(timeIndex);
            DrawVoxelUnique(timeIndex);
            DrawVoxelUniqueBothSides(timeIndex);

            GLRender.BlendEnable();
            // Сущности DisplayList
            DrawEntities(timeIndex);

            GLRender.TextureLightmapDisable();

            // Рендер и прорисовка курсора выбранного блока по AABB
            renderBlockCursor.Render(ClientMain.Player, skyLight);
            
            // Курсор чанка
            renderChunkCursor.Render(ClientMain.World.RenderEntityManager.CameraOffset);

            // Эффекты
            ClientMain.EffectRender.Render(timeIndex);

            // Прорисовка вид не с руки, а видим себя
            if (ClientMain.Player.ViewCamera != EnumViewCamera.Eye)
                World.RenderEntityManager.RenderEntity(ClientMain.Player, timeIndex);

            // Облака
            DrawClouds(timeIndex);

            // Рисуем воксели альфа VBO
            DrawVoxelAlpha(timeIndex);

            // Прорисовка руки
            if (!ClientMain.Player.IsInvisible() && ClientMain.Player.ViewCamera == EnumViewCamera.Eye)
            {
                // Матрица камеры
                ClientMain.Player.MatrixProjection(0);
                World.RenderEntityManager.RenderEntity(ClientMain.Player, timeIndex);
            }
            GLRender.TextureLightmapDisable();

            // GUI во время игры без доп окон, так же эффекты
            ScreenGame.Draw(timeIndex);

            // Чистка сетки чанков при необходимости
            World.ChunkPrClient.RemoteMeshChunks();
        }

        /// <summary>
        /// Смена видимости курсора чанка
        /// </summary>
        public void ChunkCursorHiddenShow() => renderChunkCursor.IsHidden = !renderChunkCursor.IsHidden;

        /// <summary>
        /// Связывание чанки вокселей и готовим массив чанков
        /// </summary>
        private void BindChunkVoxel()
        {
            arrayChunkRender.Clear();
            FrustumStruct[] fss = ClientMain.Player.ChunkFC;
            int count = fss.Length;
            int i;
            FrustumStruct fs;
            ChunkRender chunk;

            // Пробегаем по всем чанкам которые видим FrustumCulling
            for (i = 0; i < count; i++)
            {
                fs = fss[i];
                if (fs.IsChunk())
                {
                    chunk = fs.GetChunk();
                    if (chunk == null || !chunk.IsChunkPresent) continue;

                    // Проверяем надо ли рендер для псевдо чанка, и возможно ли по времени
                    if (chunk.IsModifiedRender() && renderQueues.CountForward < MvkGlobal.COUNT_RENDER_CHUNK_FRAME)
                    {
                        // Проверяем занят ли чанк уже рендером
                        if (chunk.IsMeshDenseWait() && chunk.IsMeshAlphaWait())
                        {
                            // Обновление рендера псевдочанка
                            Debug.CountUpdateChunck++;
                            chunk.StartRendering();
                            renderQueues.Add(chunk);
                        }
                    }
                    // Проверяем надо ли рендер для псевдо чанка, и возможно ли по времени
                    else if (chunk.IsModifiedRenderAlpha() && renderAlphaQueues.CountForward < MvkGlobal.COUNT_RENDER_CHUNK_FRAME_ALPHA)
                    {
                        // Проверяем занят ли чанк уже рендером
                        if (chunk.IsMeshDenseWait() && chunk.IsMeshAlphaWait())
                        {
                            // Обновление рендера псевдочанка
                            Debug.CountUpdateChunckAlpha++;
                            chunk.StartRenderingAlpha();
                            renderAlphaQueues.Add(chunk);
                        }
                    }

                    // Занести буфер сплошных блоков псевдо чанка если это требуется
                    if (chunk.IsMeshDenseBinding()) chunk.BindBufferDense();
                    // Занести буфер альфа блоков псевдо чанка если это требуется
                    if (chunk.IsMeshAlphaBinding()) chunk.BindBufferAlpha();

                    arrayChunkRender.Add(chunk);
                }
                else
                {
                    addInitFrustumCulling++;
                }
            }

            // Если хоть один чанк не догружен, то продолжаем счётчик
            if (addInitFrustumCulling > 0) addInitFrustumCulling++;
            // Или много не догруженных чанков, либо прошло время для повторной проверки
            if (addInitFrustumCulling > 100)
            {
                addInitFrustumCulling = 0;
                ClientMain.Player.CheckChunkFrustumCulling();
            }
        }

        /// <summary>
        /// Рисуем воксели сплошных блоков
        /// </summary>
        private void DrawVoxelDense(float timeIndex)
        {
            if (Debug.IsDrawVoxelLine)
            {
                GLWindow.gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_LINE);
                GLRender.CullDisable();
            }
            else
            {
                GLRender.CullEnable();
                GLWindow.gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_FILL);
            }
            ShaderVoxel shader = VoxelsBegin(timeIndex, GLWindow.Shaders.ShVoxel);
            GLRender.BlendEnable();

            int count = arrayChunkRender.count;
            int i;
            ChunkRender chunk;

            // Пробегаем по всем чанкам которые видим FrustumCulling
            for (i = 0; i < count; i++)
            {
                chunk = arrayChunkRender[i];
                // Прорисовка сплошных блоков псевдо чанка
                if (chunk.NotNullMeshDense())
                {
                    VoxelsShaderChunk(shader, chunk.Position);
                    chunk.DrawDense();
                }
            }

            shader.Unbind(GLWindow.gl);
            if (Debug.IsDrawVoxelLine)
            {
                // Дебаг должен прорисовать текстуру по этому сетка тут не уместна
                GLRender.CullEnable();
                GLWindow.gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_FILL);
            }
        }

        /// <summary>
        /// Рисуем воксели уникальных блоков
        /// </summary>
        private void DrawVoxelUnique(float timeIndex)
        {
            GLRender.CullEnable();
            //GLRender.CullDisable();
            ShaderVoxel shader = VoxelsBegin(timeIndex, GLWindow.Shaders.ShVoxel, true);
            GLRender.DepthEnable();
            GLRender.BlendEnable();

            int count = arrayChunkRender.count;
            int i;
            ChunkRender chunk;

            // Пробегаем по всем чанкам которые видим FrustumCulling
            for (i = 0; i < count; i++)
            {
                chunk = arrayChunkRender[i];
                // Прорисовка сплошных блоков псевдо чанка
                if (chunk.NotNullMeshUnique())
                {
                    VoxelsShaderChunk(shader, chunk.Position);
                    chunk.DrawUnique();
                }
            }

            shader.Unbind(GLWindow.gl);
        }

        /// <summary>
        /// Рисуем воксели уникальных блоков
        /// </summary>
        private void DrawVoxelUniqueBothSides(float timeIndex)
        {
            GLRender.CullDisable();
            ShaderVoxel shader = VoxelsBegin(timeIndex, GLWindow.Shaders.ShVoxelWind, true, true);
            GLRender.DepthEnable();
            GLRender.BlendEnable();

            int count = arrayChunkRender.count;
            int i;
            ChunkRender chunk;
            for (i = 0; i < count; i++)
            {
                chunk = arrayChunkRender[i];
                // Прорисовка сплошных блоков псевдо чанка
                if (chunk.NotNullMeshUniqueBothSides())
                {
                    VoxelsShaderChunk(shader, chunk.Position);
                    chunk.DrawUniqueBothSides();
                }
            }

            shader.Unbind(GLWindow.gl);
            GLRender.CullEnable();
        }

        /// <summary>
        /// Прорисовка вокселей альфа цвета
        /// </summary>
        private void DrawVoxelAlpha(float timeIndex)
        {
            GLRender.CullEnable();
            ShaderVoxel shader = VoxelsBegin(timeIndex, GLWindow.Shaders.ShVoxelWave, false, false, true);
            GLRender.DepthEnable();
            GLRender.BlendEnable();

            int count = arrayChunkRender.count - 1;
            int i;
            ChunkRender chunk;

            // Пробегаем по всем чанкам которые видим FrustumCulling
            for (i = count; i >= 0; i--)
            {
                chunk = arrayChunkRender[i];
                // Прорисовка альфа блоков псевдо чанка
                if (chunk.NotNullMeshAlpha())
                {
                    VoxelsShaderChunk(shader, chunk.Position);
                    chunk.DrawAlpha();
                }
            }

            shader.Unbind(GLWindow.gl);
        }

        /// <summary>
        /// Запуск шейдеров и текстуры для прорисовки вокселей
        /// </summary>
        /// <returns></returns>
        private ShaderVoxel VoxelsBegin(float timeIndex, ShaderVoxel shader, bool isUnique = false, bool isWind = false, bool isWave = false)
        {
            shader.Bind(GLWindow.gl);
            shader.SetUniformMatrix4(GLWindow.gl, "view", ClientMain.Player.View);
            shader.SetUniform1(GLWindow.gl, "takt", ClientMain.TickCounter); // & 31
            if (isWind)
            {
                // Ветер, значение от -1 до 1
                int wind = (int)ClientMain.TickCounter & 0x7F;
                shader.SetUniform1(GLWindow.gl, "wind", glm.cos((wind + timeIndex) * .049f) * .16f);
            }
            if (isWave)
            {
                // Ветер, значение от -1 до 1
                int wind = (int)ClientMain.TickCounter & 0xFF;
                shader.SetUniform1(GLWindow.gl, "wind", glm.cos((wind + timeIndex) * .0245f) * .08f);
            }

            float overview;
            vec3 cfog;

            if (ClientMain.Player.WhereEyesEff == EntityPlayerSP.WhereEyes.Air)
            {
                overview = Debug.IsDrawOrto > 0 ? 10000 : ClientMain.Player.OverviewChunk * 16f;
                cfog = colorFog;
            }
            else if (ClientMain.Player.WhereEyesEff == EntityPlayerSP.WhereEyes.Water)
            {
                overview = 24f;
                cfog = ScreenInGame.colorWaterEff;
            }
            else if (ClientMain.Player.WhereEyesEff == EntityPlayerSP.WhereEyes.Lava)
            {
                overview = 4f;
                cfog = ScreenInGame.colorLavaEff;
            }
            else
            {
                overview = 4f;
                cfog = ScreenInGame.colorOilEff;
            }

            shader.SetUniform1(GLWindow.gl, "overview", overview);
            shader.SetUniform3(GLWindow.gl, "colorfog", cfog.x, cfog.y, cfog.z);

            int atlas = shader.GetUniformLocation(GLWindow.gl, "atlas");
            int lightMap = shader.GetUniformLocation(GLWindow.gl, "light_map");
            GLWindow.Texture.BindTexture(isUnique ? AssetsTexture.AtlasBlocksUnique : AssetsTexture.AtlasBlocks);
            GLWindow.gl.Uniform1(atlas, 0);
            GLRender.TextureLightmapEnable();
            GLWindow.gl.Uniform1(lightMap, 1);
            GLRender.TextureLightmapDisable();

            return shader;
        }

        private void VoxelsShaderChunk(ShaderVoxel shader, vec2i chunkPos)
        {
            int x = chunkPos.x << 4;
            int z = chunkPos.y << 4;

            vec3 pos = ClientMain.Player.Position + ClientMain.Player.PositionCamera;
            shader.SetUniform3(GLWindow.gl, "pos",
                x - World.RenderEntityManager.CameraOffset.x,
                -World.RenderEntityManager.CameraOffset.y,
                z - World.RenderEntityManager.CameraOffset.z);
            shader.SetUniform3(GLWindow.gl, "camera", pos.x - x, pos.y, pos.z - z);
        }

        private void DrawEntities2(MapListEntity[] entities, float timeIndex)
        {
            for (int i = 0; i < entities.Length; i++)
            {
                if (!entities[i].IsEmpty())
                {
                    for (int j = 0; j < entities[i].Count; j++)
                    {
                        EntityBase entity = entities[i].GetAt(j);
                        int playerId = ClientMain.Player.Id;
                        int entityId = entity.Id;
                        if (entityId != playerId)
                        {
                            World.RenderEntityManager.RenderEntity(entity, timeIndex);// entity.TimeIndex());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Прорисовка сущностей DisplayList
        /// </summary>
        private void DrawEntities(float timeIndex)
        {
            World.CountEntitiesShowBegin();

            int count = arrayChunkRender.count;
            int i;

            // Пробегаем по всем чанкам которые видим FrustumCulling
            for (i = 0; i < count; i++)
            {
                DrawEntities2(arrayChunkRender[i].ListEntities, timeIndex);
            }

            // Основной игрок, вид сзади или спереди
            //if (ClientMain.Player.Type == EnumEntities.PlayerHand)
            //{
            //    World.RenderEntityManager.RenderHand(timeIndex);
            //}
            //else
            //if (ClientMain.Player.ViewCamera != EnumViewCamera.Eye)
            //{
            //    World.RenderEntityManager.RenderEntity(ClientMain.Player, timeIndex);
            //}
        }

        /// <summary>
        /// Прорисовка неба
        /// </summary>
        private void DrawSky(float timeIndex)
        {
            // Угол в радианах
            float angleRad = celestialAngle * glm.pi360;
            // Цвет неба
            vec3 colorSky = World.GetSkyColor(skyLight);
            // Яркость звёзд 0.0 - 0.75
            float starBrightness = World.GetStarBrightness(celestialAngle);
            // Цвет заката и рассвета
            float[] colors = World.CalcSunriseSunsetColors(celestialAngle);

            TextureStruct ts;
            GLRender.DepthMask(false);

            GLWindow.gl.ClearColor(colorFog.x, colorFog.y, colorFog.z, 1f);

            // Включаем туман
            GLRender.FogEnable();
            GLWindow.gl.Fog(OpenGL.GL_FOG_COORDINATE_SOURCE_EXT, OpenGL.GL_CURRENT_FOG_COORDINATE_EXT);
            GLWindow.gl.Fog(OpenGL.GL_FOG_COLOR, new float[] { colorFog.x, colorFog.y, colorFog.z, 1f });
            GLWindow.gl.Hint(OpenGL.GL_FOG_HINT, OpenGL.GL_DONT_CARE);
            GLWindow.gl.Fog(OpenGL.GL_FOG_MODE, OpenGL.GL_LINEAR);
            GLWindow.gl.Fog(OpenGL.GL_FOG_START, 0);
            GLWindow.gl.Fog(OpenGL.GL_FOG_END, 128f);

            // Верхняя часть неба
            GLRender.Texture2DDisable();
            GLRender.Color(colorSky.x, colorSky.y, colorSky.z);
            GLRender.Begin(OpenGL.GL_TRIANGLE_STRIP);
            GLRender.Vertex(448, 24, -384);
            GLRender.Vertex(448, 24, 448);
            GLRender.Vertex(-384, 24, -384);
            GLRender.Vertex(-384, 24, 448);
            GLRender.End();
            GLRender.FogDisable();

            GLRender.BlendEnable();
            GLRender.AlphaDisable();
            //GLRender.Texture2DEnable();

            // Закат и рассвет
            if (colors.Length > 0)
            {
                GLWindow.gl.BlendFuncSeparate(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA, OpenGL.GL_ONE, OpenGL.GL_ZERO);
                GLRender.Texture2DDisable();
                GLRender.PushMatrix();
                GLRender.Rotate(90f, 1f, 0, 0);
                GLRender.Rotate(glm.sin(angleRad) < 0 ? 180f : 0f, 0, 0, 1f);
                GLRender.Rotate(90f, 0, 0, 1f);
                GLRender.Begin(OpenGL.GL_TRIANGLE_FAN);
                GLRender.Color(colors[0], colors[1], colors[2], colors[3]);
                GLRender.Vertex(0, 128f, 0);
                GLRender.Color(colors[0], colors[1], colors[2], 0f);

                for (int i = 0; i <= 16; i++)
                {
                    float f1 = (float)i * glm.pi360 / 16f;
                    float fx = glm.sin(f1);
                    float fy = glm.cos(f1);
                    GLRender.Vertex(fx * 120f, fy * 120f - 16f, -fy * 40f * colors[3]);
                }
                GLRender.End();
                GLRender.PopMatrix();
            }

            // Солнце и луна
            GLWindow.gl.BlendFuncSeparate(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE, OpenGL.GL_ONE, OpenGL.GL_ZERO);
            GLRender.Texture2DEnable();

            // Солнце
            if (sunLight > 0)
            {
                GLRender.PushMatrix();
                ts = GLWindow.Texture.GetData(AssetsTexture.Sun);
                GLWindow.Texture.BindTexture(ts.GetKey());
                GLRender.Color(1f, 1f, 1f, sunLight);
                GLRender.Rotate(-90f, 0, 1, 0);
                GLRender.Rotate(-15f, 0, 0, 1);
                GLRender.Rotate(celestialAngle * 360f, 1, 0, 0);
                GLRender.Begin(OpenGL.GL_TRIANGLE_STRIP);
                GLRender.VertexWithUV(30, 100, -30, 1, 0);
                GLRender.VertexWithUV(30, 100, 30, 1, 1);
                GLRender.VertexWithUV(-30, 100, -30, 0, 0);
                GLRender.VertexWithUV(-30, 100, 30, 0, 1);
                GLRender.End();
                GLRender.PopMatrix();
            }

            // Звёзды и луна
            if (starBrightness > 0)
            {
                GLRender.PushMatrix();
                ts = GLWindow.Texture.GetData(AssetsTexture.StarBrightness);
                GLWindow.Texture.BindTexture(ts.GetKey());
                GLRender.Color(starBrightness, starBrightness, starBrightness, starBrightness);
                GLRender.Rotate(-90f, 0, 1, 0);
                GLRender.Rotate(-25f, 0, 0, 1);
                GLRender.Rotate(celestialAngle * 360f, 1, 0, 0);
                GLRender.ListCall(dlStar);
                
                GLRender.Color(1f, 1f, 1f, starBrightness + .15f);
                // Вращаем луну, чтоб она всегда смотрела верно
                GLRender.Rotate(celestialAngle * 90f + 235f, 0, 1, 0);
                // Луна с фазами
                ts = GLWindow.Texture.GetData(AssetsTexture.MoonPhases);
                GLWindow.Texture.BindTexture(ts.GetKey());
                int phase = World.GetIndexMoonPhase();
                int phaseV = phase % 4;
                int phaseH = phase / 4 % 2;
                float u1 = phaseV / 4f;
                float v1 = phaseH / 2f;
                float u2 = (phaseV + 1) / 4f;
                float v2 = (phaseH + 1) / 2f;
                GLRender.Begin(OpenGL.GL_TRIANGLE_STRIP);
                GLRender.VertexWithUV(20, -100, 20, u1, v2);
                GLRender.VertexWithUV(20, -100, -20, u1, v1);
                GLRender.VertexWithUV(-20, -100, 20, u2, v2);
                GLRender.VertexWithUV(-20, -100, -20, u2, v1);
                GLRender.End();
                GLRender.PopMatrix();
            }

            GLRender.BlendDisable();
            
            GLWindow.gl.BlendFuncSeparate(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA, OpenGL.GL_ONE, OpenGL.GL_ZERO);
            GLRender.AlphaEnable();
            GLRender.FogEnable();

            // Если игрок ниже уровня моря, то низ делаем тёмный, для защиты от бликов, при удалении блоков
            if (ClientMain.Player.Position.y < MvkServer.World.Biome.BiomeBase.HEIGHT_WATER)
            {
                GLRender.PushMatrix();
                GLRender.Texture2DDisable();
                GLRender.Color(0);
                GLRender.Begin(OpenGL.GL_TRIANGLE_STRIP);
                GLRender.Vertex(448, -4, -384);
                GLRender.Vertex(-384, -4, -384);
                GLRender.Vertex(448, -4, 448);
                GLRender.Vertex(-384, -4, 448);
                GLRender.End();
                GLRender.PopMatrix();
            }
            GLRender.FogDisable();
            GLRender.DepthMask(true);
        }

        /// <summary>
        /// Рендер звёзд
        /// </summary>
        private void RenderStar()
        {
            if (dlStar > 0) GLRender.ListDelete(dlStar);
            dlStar = GLRender.ListBegin();
            Rand random = new Rand(10842);
            GLRender.Begin(OpenGL.GL_QUADS);

            // Предполагаемое количество звёзд 1500
            for (int i = 0; i < 1500; i++)
            {
                float x = random.NextFloat() * 2f - 1f;
                float y = random.NextFloat() * 2f - 1f;
                float z = random.NextFloat() * 2f - 1f;
                float size = .15f + random.NextFloat() * .1f;
                float distance = x * x + y * y + z * z;

                // будет 1234 согласно рандома seed 10842
                if (distance < 1.5f)
                {
                    distance = 1f / Mth.Sqrt(distance);
                    x *= distance;
                    y *= distance;
                    z *= distance;
                    float x2 = x * 100f;
                    float y2 = y * 100f;
                    float z2 = z * 100f;
                    float angle = glm.atan2(x, z);
                    float sa1 = glm.sin(angle);
                    float ca1 = glm.cos(angle);
                    angle = glm.atan2(Mth.Sqrt(x * x + z * z), y);
                    float sa2 = glm.sin(angle);
                    float ca2 = glm.cos(angle);
                    angle = random.NextFloat() * glm.pi360;
                    float sa3 = glm.sin(angle);
                    float ca3 = glm.cos(angle);

                    int colorI = random.Next(214);
                    colorI = colorI < 208 ? colorI / 8 : colorI - 182;
                    float color = colorI / 32f;

                    for (int j = 0; j < 4; j++)
                    {
                        float u = (j & 2) / 64f;
                        float v = (j == 1 || j == 2) ? 1f : 0;

                        float f1 = ((j & 2) - 1) * size;
                        float f2 = ((j + 1 & 2) - 1) * size;
                        float f3 = f1 * ca3 - f2 * sa3;
                        float f4 = f2 * ca3 + f1 * sa3;
                        float f5 = f3 * ca2;

                        float x3 = -f5 * sa1 - f4 * ca1;
                        float y3 = f3 * sa2;
                        float z3 = f4 * sa1 - f5 * ca1;
                        GLRender.VertexWithUV(x2 + x3, y2 + y3, z2 + z3, u + color, v);
                    }
                }
            }
            GLRender.End();
            GLRender.ListEnd();
        }

        /// <summary>
        /// Облака неба
        /// </summary>
        private void DrawClouds(float timeIndex)
        {
            ClientMain.Player.MatrixProjection(1450f);

            GLRender.CullDisable();
            GLRender.BlendEnable();
            // TODO::2022-05-27 туман облаков, в зависимости от уровня ландшафта, примерно прикинул на 96
            GLWindow.gl.Fog(OpenGL.GL_FOG_START, 386f); // 386
            GLWindow.gl.Fog(OpenGL.GL_FOG_END, 512f); // 512
            GLRender.FogEnable();
            GLWindow.gl.BlendFuncSeparate(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA, OpenGL.GL_ONE, OpenGL.GL_ZERO);

            // Цвет облак
            vec3 colorCloud = World.GetCloudColor(skyLight);

            vec3 pos = ClientMain.Player.GetPositionFrame(timeIndex);
            float speed = ClientMain.CloudTickCounter + timeIndex;

            //float y = 256f - pos.y;
            float y = 128f - pos.y;
            float x0 = pos.x + speed * WorldBase.SPEED_CLOUD;
            
            float f1 = WorldBase.CLOUD_SIZE_TEXTURE;
            float f = 1f / WorldBase.CLOUD_SIZE_TEXTURE;
            x0 = (x0 - Mth.Floor(x0 / f1) * f1) * f;
            float z0 = (pos.z - Mth.Floor(pos.z / f1) * f1) * f;
            int dis = 1024;
            int step = 128;

            GLRender.Color(colorCloud.x, colorCloud.y, colorCloud.z, .8f);
            GLRender.Texture2DEnable();
            TextureStruct ts = GLWindow.Texture.GetData(AssetsTexture.Clouds);
            GLWindow.Texture.BindTexture(ts.GetKey());
            GLRender.Begin(OpenGL.GL_QUADS);
            for (int x = -dis; x < dis; x += step)
            {
                for (int z = -dis; z < dis; z += step)
                {
                    GLRender.VertexWithUV(x, y, z + step, x * f + x0, (z + step) * f + z0);
                    GLRender.VertexWithUV(x + step, y, z + step, (x + step) * f + x0, (z + step) * f + z0);
                    GLRender.VertexWithUV(x + step, y, z, (x + step) * f + x0, z * f + z0);
                    GLRender.VertexWithUV(x, y, z, x * f + x0, z * f + z0);
                }
            }
            GLRender.End();
            GLRender.FogDisable();
            GLRender.CullEnable();
        }

        /// <summary>
        /// Получить рендовый объект блока для GUI
        /// </summary>
        public RenderBlockGui GetBlockGui(EnumBlock enumBlock) => mapBlocksGui[enumBlock];

        /// <summary>
        /// Получить рендовый объект предмета для GUI
        /// </summary>
        public RenderItemGui GetItemGui(EnumItem enumItem) => mapItemsGui[enumItem];

        /// <summary>
        /// Рендер и прорисовка курсора выбранного блока по AABB
        /// DisplayList
        /// </summary>
        //private void DrawCursorVoxel()
        //{
        //    if (ClientMain.Player.SelectBlock != null)
        //    {
        //        renderBlockCursor.Render(ClientMain.Player.SelectBlock);
        //    }
        //}


        //private LineMesh hitboxPlayer = new LineMesh();


        //private void RenderHitBoxEntitis(float timeIndex)
        //{
        //    List<float> buffer = new List<float>();
        //    if (World.Player.ViewCamera != EnumViewCamera.Eye) buffer.AddRange(RenderHitBox(World.Player, timeIndex));
        //    foreach(EntityPlayerClient entity in World.PlayerEntities.Values)
        //    {
        //        if (entity.Name != World.Player.Name) buffer.AddRange(RenderHitBox(entity, entity.TimeIndex()));
        //    }
        //    hitboxPlayer.BindBuffer(buffer);
        //}

        //private List<float> RenderHitBox(EntityPlayerClient entity, float timeIndex)
        //{
        //    vec3 pos = entity.GetPositionFrame(timeIndex);// entity.PositionFrame;
        //    float w = entity.Width; // .6
        //    float w2 = w * 2f;
        //    float h = entity.Height; // 3.6
        //    float e = entity.GetEyeHeight(); 
        //    float y = pos.y + h / 2f;
        //    List<float> buffer = new List<float>();
        //    buffer.AddRange(hitboxPlayer.Box(pos.x, y + 0.01f, pos.z, w2, h, w2, 0, 1, 1, 1));
        //    y = pos.y + e;
        //    vec4 col = new vec4(1, .5f, 1, 1);
        //    buffer.AddRange(hitboxPlayer.Line(pos.x - w, y, pos.z + w, pos.x + w, y, pos.z + w, col));
        //    buffer.AddRange(hitboxPlayer.Line(pos.x - w, y, pos.z - w, pos.x + w, y, pos.z - w, col));
        //    buffer.AddRange(hitboxPlayer.Line(pos.x + w, y, pos.z - w, pos.x + w, y, pos.z + w, col));
        //    buffer.AddRange(hitboxPlayer.Line(pos.x - w, y, pos.z - w, pos.x - w, y, pos.z + w, col));

        //    // RED тело
        //    col = new vec4(1f, 0, 0, 1);
        //    vec3 rayBody = entity.GetLookBodyFrame(timeIndex);
        //    rayBody.y = 0;
        //    rayBody = rayBody.normalize();
        //    buffer.AddRange(hitboxPlayer.Line(pos.x, pos.y + .01f, pos.z, pos.x + rayBody.x, pos.y + .01f, pos.z + rayBody.z, col));

        //    // YELOUW голова
        //    col = new vec4(1f, 1f, 0, 1);
        //    vec3 rayHead = entity.GetLookFrame(timeIndex);
        //    buffer.AddRange(hitboxPlayer.Line(pos.x, y, pos.z, pos.x + rayHead.x, y + rayHead.y, pos.z + rayHead.z, col));
        //    vec3 rayHead2 = new vec3(rayHead.x, 0, rayHead.z);
        //    rayHead2 = rayHead2.normalize();
        //    buffer.AddRange(hitboxPlayer.Line(
        //        pos.x + rayHead2.x, pos.y + .01f, pos.z + rayHead2.z, 
        //        pos.x + rayHead2.x * 2f, pos.y + .01f, pos.z + rayHead2.z * 2f, col));

        //    pos += new vec3(rayHead.x, 0, rayHead.z) * 2f;
        //    float xz = glm.cos(entity.LimbSwing * 0.6662f) * 1.4f * entity.GetLimbSwingAmountFrame(timeIndex);

        //    buffer.AddRange(hitboxPlayer.Line(pos.x, y, pos.z, pos.x, y + xz, pos.z, col));

        //    return buffer;
        //}

        ///// <summary>
        ///// Прорисовка линий 3д
        ///// </summary>
        //protected void DrawLine()
        //{
        //    ShaderLine shader = GLWindow.Shaders.ShLine;
        //    shader.Bind(GLWindow.gl);
        //    shader.SetUniformMatrix4(GLWindow.gl, "projection", World.Player.Projection);
        //    shader.SetUniformMatrix4(GLWindow.gl, "lookat", World.Player.LookAt);

        //    hitboxPlayer.DrawLine();
        //    //WorldLineM.Draw();

        //    shader.Unbind(GLWindow.gl);
        //}


    }
}
