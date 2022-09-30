using MvkServer.Glm;
using System.Collections.Generic;

namespace MvkServer.World
{
    /// <summary>
    /// Объект который хранит и отвечает за кэш регионов
    /// </summary>
    public class RegionProvider
    {
        /// <summary>
        /// Список регионов
        /// </summary>
        private Dictionary<vec2i, RegionFile> map = new Dictionary<vec2i, RegionFile>();

        /// <summary>
        /// Сылка на объект мира сервера
        /// </summary>
        private readonly WorldServer world;

        private readonly object locker = new object();

        public RegionProvider(WorldServer world)
        {
            this.world = world;
        }

        /// <summary>
        /// Получить Файл региона по его координатам чанка
        /// </summary>
        public RegionFile Get(vec2i posChunk)
        {
            vec2i pos = new vec2i(posChunk.x >> 5, posChunk.y >> 5);
            if (!map.ContainsKey(pos))
            {
                lock (locker)
                {
                    RegionFile region = new RegionFile(world, pos.x, pos.y);
                    map.Add(pos, region);
                }
            }
            return map[pos];
        }

        /// <summary>
        /// Количество регионов
        /// </summary>
        public int Count() => map.Count;

        /// <summary>
        /// Сохранение регионов
        /// </summary>
        public void WriteToFile(bool isStoping)
        {
            lock (locker)
            {
                foreach (RegionFile region in map.Values)
                {
                    region.WriteToFile();
                }
            }
            if (!isStoping)
            {
                // Проверяем какие регионы можно выгрузить
                List<vec2i> list = world.ChunkPr.GetRegionList();
                List<vec2i> listRemove = new List<vec2i>();
                foreach (vec2i key in map.Keys)
                {
                    if (!list.Contains(key))
                    {
                        listRemove.Add(key);
                    }
                }
                foreach (vec2i key in listRemove)
                {
                    map.Remove(key);
                }
            }
        }
    }
}
