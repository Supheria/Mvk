using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Gen;
using System;

namespace MvkServer.World.Biome
{
    /// <summary>
    /// Биом отладки
    /// </summary>
    public class BiomeDebug : BiomeBase
    {
        public BiomeDebug(ChunkProviderGenerateBase chunkProvider) : base(chunkProvider) 
        {
        }

        /// <summary>
        /// Получить уровень высоты
        /// </summary>
        /// <param name="height">Высота -1..0..1</param>
        /// <param name="river">Определение центра реки 1..0..1</param>
        protected override int GetLevelHeight(int x, int z, float height, float river)
        {
            float riv = 1f - river;
            int yh = HEIGHT_WATER + (int)(height * HEIGHT_HILL * 4);
            // Уменьшаем рельеф где вода
            if (riv > .5f) riv = glm.cos(river * glm.pi) * .5f + .5f;
            yh = yh - (int)((yh - HEIGHT_WATER_MINUS_2 + height * HEIGHT_HILL) * riv);

            return yh;
        }

        /// <summary>
        /// Возращаем сгенерированный столбец
        /// </summary>
        /// <param name="x">X 0..15</param>
        /// <param name="z">Z 0..15</param>
        /// <param name="height">Высота -1..0..1</param>
        /// <param name="river">Определение центра реки 1..0..1</param>
        public override void Column(int x, int z, float height, float river)
        {
            int yh = GetLevelHeight(x, z, height, river);
            if (yh < 2) yh = 2;
            chunk.heightMap[x << 4 | z] = yh;
            int index = x << 12 | z << 8;
            int y = 0;

            try
            {
                if (isBlockBody)
                {
                    // Определяем высоту тела по шуму (3 - 6)
                    int bodyHeight = (int)(Provider.AreaNoise[x << 4 | z] / 4f + 5f);

                    int yb = yh - bodyHeight;
                    if (yb < 2) yb = 2;

                    // заполняем камнем
                    for (y = 3; y < yb; y++) chunk.id[index | y] = 3;
                    // заполняем тело
                    for (y = yb; y < yh; y++) chunk.id[index | y] = blockIdBody;
                    chunk.id[x << 12 | z << 8 | yh] = yh < HEIGHT_WATER ? blockIdBody : blockIdUp;
                }
                else
                {
                    // заполняем камнем
                    for (y = 3; y <= yh; y++) chunk.id[index | y] = 3;
                }
            }
            catch (Exception ex)
            {
                Logger.Crach(ex, "yh:{0} y:{1} index:{2}", yh, y, index);
            }
            if (yh < HEIGHT_WATER)
            {
                // меньше уровня воды
                yh++;
                for (y = yh; y < HEIGHT_WATER_PLUS; y++)
                {
                    // заполняем водой
                    chunk.id[index | y] = 13;
                }
            }
        }
    }
}
