using MvkServer.World.Biome;

namespace MvkServer.World.Gen.Layer
{
    // https://github.com/Cubitect/cubiomes
    /// <summary>
    /// Абстрактный класс слоёв
    /// </summary>
    public abstract class GenLayer
    {
        /// <summary>
        /// Seed из мира, который используется в LCG
        /// </summary>
        private long worldGenSeed;
        /// <summary>
        /// Заключительная часть LCG, которая использует координаты фрагмента X, Z
        /// вместе с двумя другими начальными значениями для генерации псевдослучайных чисел
        /// </summary>
        private long chunkSeed;

        /// <summary>
        /// Базовое семя для LCG, предоставленного через конструктор
        /// </summary>
        protected long baseSeed;
        /// <summary>
        /// Родительский GenLayer, который был предоставлен через конструктор
        /// </summary>
        protected GenLayer parent;

        protected GenLayer() => baseSeed = 1;

        public GenLayer(long baseSeed)
        {
            this.baseSeed = baseSeed;
            this.baseSeed *= this.baseSeed * 6364136223846793005L + 1442695040888963407L;
            this.baseSeed += baseSeed;
            this.baseSeed *= this.baseSeed * 6364136223846793005L + 1442695040888963407L;
            this.baseSeed += baseSeed;
            this.baseSeed *= this.baseSeed * 6364136223846793005L + 1442695040888963407L;
            this.baseSeed += baseSeed;
        }

        /// <summary>
        /// Инициализировать локальный worldGenSeed слоя на основе его собственного baseSeed
        /// и глобального начального числа (передается в качестве аргумента)
        /// </summary>
        public virtual void InitWorldGenSeed(long worldSeed)
        {
            worldGenSeed = worldSeed;
            if (parent != null) parent.InitWorldGenSeed(worldSeed);
            worldGenSeed *= worldGenSeed * 6364136223846793005L + 1442695040888963407L;
            worldGenSeed += baseSeed;
            worldGenSeed *= worldGenSeed * 6364136223846793005L + 1442695040888963407L;
            worldGenSeed += baseSeed;
            worldGenSeed *= worldGenSeed * 6364136223846793005L + 1442695040888963407L;
            worldGenSeed += baseSeed;
        }

        /// <summary>
        /// Инициализировать текущий chunkSeed слоя на основе локального worldGenSeed 
        /// и координат чанка (x,z)
        /// </summary>
        public void InitChunkSeed(long x, long y)
        {
            chunkSeed = worldGenSeed;
            chunkSeed *= chunkSeed * 6364136223846793005L + 1442695040888963407L;
            chunkSeed += x;
            chunkSeed *= chunkSeed * 6364136223846793005L + 1442695040888963407L;
            chunkSeed += y;
            chunkSeed *= chunkSeed * 6364136223846793005L + 1442695040888963407L;
            chunkSeed += x;
            chunkSeed *= chunkSeed * 6364136223846793005L + 1442695040888963407L;
            chunkSeed += y;
        }

        /// <summary>
        /// Возвращает псевдослучайное число LCG из [0, x). Аргументы: целое х
        /// </summary>
        protected int NextInt(int x)
        {
            int random = (int)((chunkSeed >> 24) % x);
            if (random < 0) random += x;

            chunkSeed *= chunkSeed * 6364136223846793005L + 1442695040888963407L;
            chunkSeed += worldGenSeed;
            return random;
        }

        /// <summary>
        /// Возвращает псевдослучайное число LCG из [0, 1]
        /// </summary>
        protected bool NextBool() => NextInt(2) == 1;

        /// <summary>
        /// Возвращает список целочисленных значений, сгенерированных этим слоем. 
        /// Их можно интерпретировать как температуру, 
        /// количество осадков или индексы biomeList[], 
        /// основанные на конкретном подклассе GenLayer.
        /// </summary>
        public abstract int[] GetInts(int areaX, int areaZ, int width, int height);

        public static GenLayer[] BeginLayerBiome(long worldSeed)
        {
            GenLayer layerBiomeOne;
            GenLayer layerBiomeParam1;
            GenLayer layerBiomeParam2;
            GenLayer layerBiome;
            GenLayer layerHeight;

            layerBiomeOne = new GenLayerGenerationPattern(worldSeed);
            layerBiomeOne = new GenLayerZoom(layerBiomeOne);
            // Этот слой если увеличить биомы в 2 раза
            layerBiomeOne = new GenLayerZoomRandom(71, layerBiomeOne);
            // Река и пляж
            layerBiomeOne = new GenLayerShore(new GenLayerRiver(layerBiomeOne));

            int idRiver = (int)EnumBiome.River;
            int idSea = (int)EnumBiome.Sea;
            // Biome
            layerBiome = new GenLayerZoomRandom(21, layerBiomeOne);
            // Расширяем биом реки, для бесконечного источника воды
            layerBiome = new GenLayerExpand(layerBiome, idRiver);
            layerBiome = new GenLayerExpand(layerBiome, idSea);
            layerBiome = new GenLayerZoomRandom(24, layerBiome);
            // Расширяем биом реки, для бесконечного источника воды
            layerBiome = new GenLayerExpand(layerBiome, idRiver);
            layerBiome = new GenLayerExpand(layerBiome, idSea);
            layerBiome = layerBiomeParam1 = new GenLayerZoomRandom(15, new GenLayerSmooth(15, layerBiome));
            layerBiome = new GenLayerZoomRandom(25, layerBiome);
            layerBiome = new GenLayerSmooth(7, layerBiome);
            // Расширяем биом реки, для бесконечного источника воды
            layerBiome = new GenLayerExpand(layerBiome, idRiver);
            layerBiome = layerBiomeParam2 = new GenLayerSmooth(17, layerBiome);
            // Расширяем биом реки, для бесконечного источника воды
            layerBiome = new GenLayerExpand(layerBiome, idRiver);
            layerBiome = new GenLayerZoomRandom(23, layerBiome);
            layerBiome = new GenLayerSmooth(2, layerBiome);
            // EndBiome

            // Height
            layerHeight = new GenLayerBiomeHeight(layerBiomeOne);
            layerHeight = new GenLayerHeightAddBegin(2, layerHeight);
            layerHeight = new GenLayerSmoothMix(new GenLayerZoomRandom(21, layerHeight));
            // Объект по добавлении высот (неровности вверх)
            layerHeight = new GenLayerHeightAddUp(2, layerHeight);
            layerHeight = new GenLayerSmoothMix(new GenLayerZoomRandom(24, layerHeight));
            // добавлении высот (ущелены в море и не большие неровности)
            layerHeight = new GenLayerHeightAddSea(100, layerHeight);
            layerHeight = new GenLayerSmoothMix(new GenLayerZoomRandom(15, new GenLayerSmooth(15, layerHeight)));
            // Добавляем на гора, болоте и лесу неровности
            layerHeight = new GenLayerHeightAddBiome(200, layerHeight, layerBiomeParam1, true);
            layerHeight = new GenLayerSmoothMix(new GenLayerZoomRandom(25, layerHeight));
            // Добавляем на гора, болоте неровности, чтоб более острее
            layerHeight = new GenLayerHeightAddBiome(300, layerHeight, layerBiomeParam2, false);
            layerHeight = new GenLayerSmooth(7, layerHeight);
            layerHeight = new GenLayerSmooth(17, layerHeight);
            layerHeight = new GenLayerSmoothMix(new GenLayerZoomRandom(23, layerHeight));
            layerHeight = new GenLayerSmooth(2, layerHeight);
            // EndHeight

            layerBiome.InitWorldGenSeed(worldSeed);
            layerHeight.InitWorldGenSeed(worldSeed);

            return new GenLayer[] { layerBiome, layerHeight };
        }
    }
}
