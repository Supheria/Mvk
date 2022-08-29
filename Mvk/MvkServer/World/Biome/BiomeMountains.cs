using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Gen;

namespace MvkServer.World.Biome
{
    /// <summary>
    /// Биом Горы
    /// </summary>
    public class BiomeMountains : BiomeBase
    {
        /// <summary>
        /// Блок тела
        /// </summary>
        protected ushort blockIdBody2 = 3;

        public BiomeMountains(ChunkProviderGenerate chunkProvider) : base(chunkProvider)
        {
            Decorator.limestonePerChunk = 10;
            Decorator.oakPerChunk = 1;
            Decorator.randomTree = 10;
            Decorator.brolPerChunk = 1;
        }

        protected virtual int BodyHeight(float area, int yh) => yh;

        /// <summary>
        /// Получить уровень высоты
        /// </summary>
        /// <param name="height">Высота -1..0..1</param>
        /// <param name="river">Определение центра реки 1..0..1</param>
        protected override int GetLevelHeight(int x, int z, float height, float river) => 97 + (int)(height * 96f);

        /// <summary>
        /// Возращаем сгенерированный столбец
        /// </summary>
        /// <param name="x">X 0..15</param>
        /// <param name="z">Z 0..15</param>
        /// <param name="height">Высота -1..0..1</param>
        /// <param name="river">Определение центра реки 1..0..1</param>
        public override void Column(int x, int z, float height, float river)
        {
            float h = height - .125f;
            // чтоб коэффициент был максимум 1 = (0.25 - 0.125) * 7f, а потом добавим 0.125
            h *= 7f; 
            if (h < .25f) h = h * h * 4f;
            height = h + .125f;

            // Максимальная высота горы 96 + 96 = 192
            int yh = 96 + (int)(height * 96f);
            chunk.heightMap[x << 4 | z] = yh;
            int index = x << 12 | z << 8;
            int y;
            float area = Provider.AreaNoise[x << 4 | z];
            // Определяем высоту тела
            int yb = BodyHeight(area, yh);
            // заполняем камнем
            for (y = 3; y < yb; y++) chunk.data[index | y] = 3;
            // заполняем тело
            for (y = yb; y <= yh; y++) chunk.data[index | y] = blockIdBody2;

            // Проплешены из земли
            // 108 - 124
            float k = (yh - 106f) / 4f;
            if (area > k || area < -k)
            {
                chunk.data[index | yh] = blockIdUp;
                y = 1;
                h = 4f;
                while (true)
                {
                    k = ((yh + y) - 106f) / h;
                    if (area > k || area < -k)
                    {
                        chunk.data[index | (yh - y)] = blockIdBody;
                        if (h > 1.5f) h -= .5f;
                        y++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }
}
