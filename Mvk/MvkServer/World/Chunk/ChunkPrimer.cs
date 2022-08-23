using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Biome;

namespace MvkServer.World.Chunk
{
    /// <summary>
    /// Подготовительный чанк, для генерации
    /// x << 12 | z << 8 | y;
    /// </summary>
    public class ChunkPrimer
    {
        /// <summary>
        /// Данные блока
        /// 12 bit Id блока и 4 bit параметр блока
        /// x << 12 | z << 8 | y;
        /// 65536 = 16 * 16 * 256
        /// </summary>
        public ushort[] data = new ushort[65536];
        /// <summary>
        /// Индексы блоков которые надо осветить, плафоны
        /// </summary>
       // public ArrayMvk<ushort> blockLight = new ArrayMvk<ushort>(65536);
        /// <summary>
        /// Массив для списка блоков с освещённости
        /// </summary>
        public ArrayMvk<vec3i> arrayLightBlocks = new ArrayMvk<vec3i>(65536);

        /// <summary>
        /// Биомы
        /// x << 4 | z;
        /// </summary>
        public EnumBiome[] biome = new EnumBiome[256];

        /// <summary>
        /// Карта высот
        /// x << 4 | z;
        /// </summary>
        public int[] heightMap = new int[256];

        /// <summary>
        /// Очистить
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < 65536; i++) data[i] = 0;
            for (int i = 0; i < 256; i++)
            {
                biome[i] = EnumBiome.Plain;
                heightMap[i] = 0;
            }
            arrayLightBlocks.Clear();
        }
    }
}
