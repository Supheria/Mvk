namespace MvkServer.World.Gen.Layer
{
    // TODO::2023-04-18 нас C https://github.com/Cubitect/cubiomes
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
            GenLayerSmoothMix layerSmoothMix;

            GenLayerGenerationPattern layerGenerationPattern = new GenLayerGenerationPattern(worldSeed);

            GenLayerZoom layerZoom = new GenLayerZoom(layerGenerationPattern);
            GenLayerZoomRandom layerZoomRand = new GenLayerZoomRandom(21, layerZoom);
            // Этот слой если увеличить биомы в 2 раза
           // layerZoomRand = new GenLayerZoomRandom(71, layerZoomRand);

            // Река
            GenLayerRiver layerRiver = new GenLayerRiver(layerZoomRand);
            // Берег пляжа
            GenLayerShore layerShore = new GenLayerShore(layerRiver);

            // Height
            layerSmoothMix = new GenLayerSmoothMix(new GenLayerBiomeHeight(layerShore));
            layerSmoothMix = new GenLayerSmoothMix(new GenLayerZoomRandom(24, layerSmoothMix));
            layerSmoothMix = new GenLayerSmoothMix(new GenLayerZoomRandom(15, layerSmoothMix));
            layerSmoothMix = new GenLayerSmoothMix(new GenLayerZoomRandom(25, layerSmoothMix));
            layerSmoothMix = new GenLayerSmoothMix(new GenLayerZoomRandom(23, layerSmoothMix));
            layerSmoothMix.InitWorldGenSeed(worldSeed);
            // EndHeight

            // Original
            layerZoomRand = new GenLayerZoomRandom(24, layerShore);
            layerZoomRand = new GenLayerZoomRandom(15, new GenLayerSmooth(15, layerZoomRand));
            layerZoomRand = new GenLayerZoomRandom(25, layerZoomRand);
            layerZoomRand = new GenLayerZoomRandom(23, layerZoomRand);
            // EndOriginal

            // Эффект сглаживания только для биомов
            GenLayerSmooth layerSmooth = new GenLayerSmooth(7, layerZoomRand);
            layerSmooth = new GenLayerSmooth(2, layerSmooth);
            layerSmooth.InitWorldGenSeed(worldSeed);

            return new GenLayer[] { layerSmooth, layerSmoothMix };
        }
    }
}
