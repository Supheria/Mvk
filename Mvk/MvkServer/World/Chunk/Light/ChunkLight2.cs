using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Block;
using System.Collections.Generic;
using System.Diagnostics;

namespace MvkServer.World.Chunk.Light
{
    /// <summary>
    /// Данные и обработки освещения
    /// </summary>
    public class ChunkLight2 : ChunkHeir
    {
        /// <summary>
        /// Карта высот по чанку, z << 4 | x
        /// </summary>
        public byte[] heightMap = new byte[256];
        /// <summary>
        /// Карта высот по чанку с учётом прозрачности, для стартового бокового освещения, z << 4 | x
        /// </summary>
        private readonly byte[] heightMapOpacity = new byte[256];
        /// <summary>
        /// Высотная карта самый высокий блок в чанке от неба
        /// </summary>
        public int heightMapMax = 0;
        /// <summary>
        /// Была ли обработка освещения при старте чанка
        /// </summary>
        private bool isChunkLight = false;
        /// <summary>
        /// Список блоков которые светятся
        /// </summary>
        private List<vec3i> lightBlocks = new List<vec3i>();

        public ChunkLight2(ChunkBase chunk) : base(chunk) { }

        /// <summary>
        /// Добавить блок с блочным освещением
        /// </summary>
        public void SetLightBlock(vec3i lightBlock) => lightBlocks.Add(lightBlock);
        public void SetLightBlocks(vec3i[] lightBlocks) => this.lightBlocks.AddRange(lightBlocks);
        /// <summary>
        /// Может ли видеть небо
        /// </summary>
        /// <param name="pos">глобальная координата мира</param>
        //public bool IsAgainstSky(BlockPos pos) => pos.Y >= heightMap[(pos.Z & 15) << 4 | (pos.X & 15)];
        public bool IsAgainstSky(int x, int y, int z) => y >= heightMap[(z & 15) << 4 | (x & 15)];

        #region HeightMap

        /// <summary>
        /// Возвращает значение карты высот в этой координате x, z в чанке. 
        /// </summary>
        public byte GetHeight(int x, int z) => heightMap[z << 4 | x];
        /// <summary>
        /// Копия высот
        /// </summary>
        public byte[] CloneHeightMap()
        {
            byte[] map = new byte[256];
            for (int i = 0; i < 256; i++) map[i] = heightMap[i];
            return map;
        }

        /// <summary>
        /// Карта высот по чанку с учётом прозрачности, для стартового бокового освещения, z << 4 | x
        /// </summary>
        public byte GetHeightOpacity(int x, int z) => heightMapOpacity[z << 4 | x];

        public void CheckHeightMap(BlockPos blockPos, byte lightOpacity)
        {
            int x = blockPos.X & 15;
            int z = blockPos.Z & 15;
            int index = z << 4 | x;
            int yNew = blockPos.Y + 1;
            int yOld = heightMap[index];

            if (yNew == heightMapMax)
            {
                // обновляем максимальные высоты
                GenerateHeightMap();
            }
            else if (yNew >= yOld)
            {
                // обновляем только столб высот
                ChunkStorage chunkStorage;
                int y, y1;
                heightMap[index] = 0;
                for (y = yNew; y > 0; y--)
                {
                    y1 = y - 1;
                    chunkStorage = Chunk.StorageArrays[y1 >> 4];
                    if (chunkStorage.countBlock != 0 && Blocks.blocksLightOpacity[chunkStorage.data[(y1 & 15) << 8 | z << 4 | x] & 0xFFF] >> 4 != 0)
                    {
                        // первый блок препятствия сверху
                        heightMap[index] = (byte)y;
                        if (heightMapMax < y) heightMapMax = y;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Создает карту высот для блока с нуля
        /// </summary>
        public void GenerateHeightMap()
        {
            ChunkStorage chunkStorage;
            heightMapMax = 0;
            int yb = Chunk.GetTopFilledSegment() + 17;
            if (yb > ChunkBase.COUNT_HEIGHT_BLOCK + 1) yb = ChunkBase.COUNT_HEIGHT_BLOCK + 1;
            int x, y, z, y1;
            yb--;
            for (x = 0; x < 16; x++)
            {
                for (z = 0; z < 16; z++)
                {
                    heightMap[z << 4 | x] = 0;
                    for (y = yb; y > 0; y--)
                    {
                        y1 = y - 1;
                        chunkStorage = Chunk.StorageArrays[y1 >> 4];
                        if (chunkStorage.countBlock != 0 && Blocks.blocksLightOpacity[chunkStorage.data[(y1 & 15) << 8 | z << 4 | x] & 0xFFF] >> 4 != 0)
                        {
                            // первый блок препятствия сверху
                            heightMap[z << 4 | x] = (byte)y;
                            if (heightMapMax < y) heightMapMax = y;
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Создает карту высот для блока с нуля и тут же осветляет небесные блоки
        /// </summary>
        public void GenerateHeightMapSky()
        {
            heightMapMax = 0;
            //int yb = Chunk.GetTopFilledSegment() + 17;
            //if (yb > ChunkBase.COUNT_HEIGHT_BLOCK + 1) yb = ChunkBase.COUNT_HEIGHT_BLOCK + 1;
            int x, y, z, y1, opacity, light, y2, index;
            int yb = 255;
            int y1c = ChunkBase.COUNT_HEIGHT15;
            ChunkStorage chunkStorage = Chunk.StorageArrays[y1c];
            for (x = 0; x < 16; x++)
            {
                for (z = 0; z < 16; z++)
                {
                    heightMap[z << 4 | x] = 0;
                    heightMapOpacity[z << 4 | x] = 0;
                    for (y = yb; y > 0; y--)
                    {
                        y1 = y - 1;
                        if (y1c != y1 >> 4)
                        {
                            y1c = y1 >> 4;
                            chunkStorage = Chunk.StorageArrays[y1c];
                        }
                        if (chunkStorage.countBlock != 0) opacity = Blocks.blocksLightOpacity[chunkStorage.data[(y1 & 15) << 8 | z << 4 | x] & 0xFFF] >> 4;
                        else opacity = 0;
                        if (opacity == 0)
                        {
                            // Небо, осветляем
                            Chunk.StorageArrays[y >> 4].lightSky[(y & 15) << 8 | z << 4 | x] = 0xF;
                        }
                        else
                        {
                            // первый блок препятствия сверху
                            heightMap[z << 4 | x] = (byte)y;
                            if (heightMapMax < y) heightMapMax = y;
                            
                            light = 15;
                            y2 = y;

                            Chunk.StorageArrays[y2 >> 4].lightSky[(y2 & 15) << 8 | z << 4 | x] = 0xF;
                            y2--;
                            // Запускаем цикл затемнения блоков
                            while (light > 0 && y2 > 0)
                            {
                                chunkStorage = Chunk.StorageArrays[y2 >> 4];
                                index = (y2 & 15) << 8 | z << 4 | x;
                                if (chunkStorage.countBlock != 0) opacity = Blocks.blocksLightOpacity[chunkStorage.data[index] & 0xFFF] >> 4;
                                else opacity = 0;
                                light = light - opacity - 1;
                                if (light < 0) light = 0;
                                Chunk.StorageArrays[y2 >> 4].lightSky[index] = (byte)light;
                                y2--;
                            }
                            heightMapOpacity[z << 4 | x] = (byte)(y2 + 2);
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Запуск проверки бокового небесного освещения
        /// </summary>
        public void StartRecheckGaps()
        {
            if (!isChunkLight)// && World.IsAreaLoaded(Chunk.Position, 1))
            {
                //Stopwatch stopwatch = new Stopwatch();
                //stopwatch.Start();
                World.Light.ActionChunk(Chunk);
                if (!World.Light.UpChunks()) return;
                World.Light.HeavenSideLighting();
                //int c1 = World.Light.GetCountBlock();
                //long le1 = stopwatch.ElapsedTicks;
                //int c2 = 0;
                //if (Chunk.Position.x == 2 && Chunk.Position.y == 2)
                //{
                //    World.SetBlockDebug(new BlockPos(40, 160, 40), EnumBlock.Glass);
                //}

                if (lightBlocks.Count > 0)
                {
                    World.Light.CheckBrighterLightBlocks(lightBlocks);
                    lightBlocks.Clear();
                }

                //long le2 = stopwatch.ElapsedTicks;
                //if ((c1 > 0 || c2 > 0) && World is WorldServer worldServer)
                //{
                //    worldServer.Log.Log("HSL c:{1} t:{0:0.00}ms CBLB c:{3} t:{2:0.00}ms",
                //        le1 / (float)MvkStatic.TimerFrequency, c1,
                //        (le2 - le1) / (float)MvkStatic.TimerFrequency, c2);
                //}
                isChunkLight = true;
            }
        }

        #endregion

        /// <summary>
        /// Проверить небесный свет
        /// </summary>
        public void CheckLightSky(int x, int y, int z, byte lo)
        {
            int yh = GetHeight(x & 15, z & 15);
            if (lo >> 4 > 0) // Не небо
            {
                // закрываем небо
                if (y >= yh)
                {
                    // Проверка столба неба
                    CheckLightColumnSky(x, y + 1, z, yh);
                }
                else
                {
                    // Проверка блока небесного освещения
                    World.Light.CheckLightSky(x, y, z, lo);
                }
            }
            else if (y == yh - 1)
            {
                // Открываем небо, проверка столба неба
                CheckLightColumnSky(x, y, z, yh);
            }
            else
            {
                // Проверка блока небесного освещения
                World.Light.CheckLightSky(x, y, z, lo);
            }
        }

        /// <summary>
        /// Проверить небесный столб света
        /// </summary>
        private void CheckLightColumnSky(int x, int y0, int z, int yh)
        {
            int yh0 = yh;
            // Определяем нижний блок
            if (y0 > yh) yh0 = y0;

            ChunkStorage chunkStorage;
            int xb = x & 15;
            int zb = z & 15;
            int opacity = 0;
            int yh1;
            
            while (yh0 > 0 && opacity == 0)
            {
                yh1 = yh0 - 1;
                if (yh1 < 0 || yh1 > ChunkBase.COUNT_HEIGHT_BLOCK)
                {
                    opacity = 0;
                }
                else
                {
                    chunkStorage = Chunk.StorageArrays[yh1 >> 4];
                    if (chunkStorage.countBlock != 0)
                    {
                        opacity = Blocks.blocksLightOpacity[chunkStorage.data[(yh1 & 15) << 8 | zb << 4 | xb] & 0xFFF] >> 4;
                    }
                    else
                    {
                        opacity = 0;
                    }
                }
                if (opacity == 0) yh0--;
            }

            // Если блок равен высотной игнорируем
            if (yh == y0) return;

            int yh2 = yh0;
            if (yh2 > ChunkBase.COUNT_HEIGHT_BLOCK) yh2 = ChunkBase.COUNT_HEIGHT_BLOCK;
            heightMap[zb << 4 | xb] = (byte)yh2;
            if (heightMapMax < yh2) heightMapMax = yh2;

            if (yh < yh0)
            {
                // закрыли небо, надо затемнять
                int yDown = yh;// + 1;
                int yUp = yh0;// - 1;
                World.Light.DarkenLightColumnSky(x, yDown, z, yUp);
              //  for (int y = yDown; y < yUp; y++) World.SetBlockDebug(new BlockPos(x, y, z), EnumBlock.Glass);
            }
            else
            {
                // открыли небо, надо осветлять
                int yDown = yh0;
                int yUp = yh;
                // пометка что убераем вверхний блок
                bool hMax = yh == heightMapMax;
                World.Light.BrighterLightColumnSky(x, yDown, z, yUp);
               // for (int y = yDown; y < yUp; y++) World.SetBlockDebug(new BlockPos(x, y, z), EnumBlock.Glass);
                // обновляем максимальные высоты
                if (hMax) GenerateHeightMap();
            }
        }
    }
}
