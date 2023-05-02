using MvkServer.World.Block;
using MvkServer.World.Gen;

namespace MvkServer.World.Biome
{
    /// <summary>
    /// Биом Горы
    /// </summary>
    public class BiomeMountains : BiomeAbGrass
    {
        /// <summary>
        /// Блок тела
        /// </summary>
        protected ushort blockIdBody2;

        /// <summary>
        /// Максимальный уровень стартового значения слаёв
        /// </summary>
        protected byte maxLevel = 96;
        /// <summary>
        /// Коэфициент дополнительного шума
        /// </summary>
        protected float kofAddNoise = 16f;

        public BiomeMountains(ChunkProviderGenerateBase chunkProvider) : base(chunkProvider)
        {
            blockIdBody2 = blockIdBiomDebug = (ushort)EnumBlock.Stone;
        }

        /// <summary>
        /// Инициализировать декорацию
        /// </summary>
        public override void InitDecorator(bool isRobinson)
        {
            base.InitDecorator(isRobinson);
            Decorator.limestonePerChunk = 10;
            Decorator.oakPerChunk = 1;
            Decorator.randomTree = 10;
            Decorator.brolPerChunk = 1;
            if (isRobinson)
            {
                Decorator.granitePerChunk = 10;
                Decorator.coalPerChunk = 10;
                Decorator.ironPerChunk = 8;
                Decorator.goldPerChunk = 1;
                blockIdBody = blockIdUp = blockIdBody2;
            }
        }

        protected virtual int BodyHeight(float area, int yh) => yh;

        /// <summary>
        /// Получить уровень высоты
        /// </summary>
        /// <param name="height">Высота -1..0..1</param>
        /// <param name="river">Определение центра реки 1..0..1</param>
        protected override int GetLevelHeight(int x, int z, float height, float river) 
            => HEIGHT_WATER_PLUS + (int)(height * HEIGHT_HILL);

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
            int yh = HEIGHT_WATER + (int)(height * HEIGHT_HILL);
            chunk.heightMap[x << 4 | z] = yh;
            int index = x << 12 | z << 8;
            int y;
            float area = Provider.AreaNoise[x << 4 | z];
            // Определяем высоту тела
            int yb = BodyHeight(area, yh);
            // заполняем камнем
            for (y = 3; y < yb; y++) chunk.id[index | y] = 3;
            // заполняем тело
            for (y = yb; y <= yh; y++) chunk.id[index | y] = blockIdBody2;

            // Проплешены из земли
            // 108 - 124
            float k = (yh - HEIGHT_MOUNTAINS_MIX) / 4f;
            if (area > k || area < -k)
            {
                chunk.id[index | yh] = blockIdUp;
                y = 1;
                h = 4f;
                while (true)
                {
                    k = ((yh + y) - HEIGHT_MOUNTAINS_MIX) / h;
                    if (area > k || area < -k)
                    {
                        chunk.id[index | (yh - y)] = blockIdBody;
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

        /// <summary>
        /// Получить уровень множителя высоты
        /// </summary>
        /// <param name="x">X 0..15</param>
        /// <param name="z">Z 0..15</param>
        /// <param name="height">Высота в блоках, средняя рекомендуемая</param>
        /// <param name="heightNoise">Высота -1..0..1</param>
        /// <param name="addNoise">Диапазон -1..0..1</param>
        protected override int GetLevelHeightRobinson(int x, int z, int height, float heightNoise, float addNoise)
        {
            if (height < maxLevel) addNoise *= addNoise;
            float noiseKof;

            if (heightNoise < 0)
            {
                noiseKof = 8f;
            }
            else
            {
                noiseKof = 12f;
                heightNoise = 1 - heightNoise;
                heightNoise *= heightNoise;
                heightNoise = 1 - heightNoise;
            }
            return height + (int)(heightNoise * noiseKof) + (int)(addNoise * kofAddNoise);
        }
    }
}
