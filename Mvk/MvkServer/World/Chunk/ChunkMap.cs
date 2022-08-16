using MvkServer.Glm;
using MvkServer.Util;
using System;
using System.Collections.Generic;

namespace MvkServer.World.Chunk
{
    /// <summary>
    /// Карта чанков
    /// </summary>
    public class ChunkMap
    {
        protected Dictionary<vec2i, ChunkBase> map = new Dictionary<vec2i, ChunkBase>();

        /// <summary>
        /// Добавить или изменить чанк
        /// </summary>
        public void Set(ChunkBase chunk)
        {
            chunk.UpdateTime();
            try
            {
                if (map.ContainsKey(chunk.Position))
                {
                    map[chunk.Position] = chunk;
                }
                else
                {
                    map.Add(chunk.Position, chunk);
                }
            }
            catch (Exception ex)
            {
                Logger.Crach(ex);
                throw;
            }
        }

        /// <summary>
        /// Получить чанк с массива
        /// </summary>
        public ChunkBase Get(vec2i pos)
        {
            if (map.ContainsKey(pos))
            {
                return map[pos];
            }
            return null;
        }

        /// <summary>
        /// Проверить наличие чанка
        /// </summary>
        public bool Contains(ChunkBase chunk) => map.ContainsKey(chunk.Position);
        /// <summary>
        /// Проверить наличие чанка
        /// </summary>
        public bool Contains(vec2i pos) => map.ContainsKey(pos);

        /// <summary>
        /// Очистить
        /// </summary>
        public void Clear() => map.Clear();

        /// <summary>
        /// Удалить
        /// </summary>
        public void Remove(vec2i pos)
        {
            ChunkBase chunk = Get(pos);
            if (chunk != null && chunk.IsChunkLoaded)
            {
                chunk.OnChunkUnload();
            }
            map.Remove(pos);
        }

        /// <summary>
        /// Добавить в список мусор удаляющих чанков для сервера!
        /// </summary>
        //public void DroopedChunkStatusMin(MapListVec2i droppedChunks, List<EntityPlayerServer> players)
        //{
        //    Hashtable ht = map.Clone() as Hashtable;
        //    foreach (ChunkBase chunk in ht.Values)
        //    {
        //        if (chunk.DoneStatus < 4 && chunk.IsOldTime())
        //        {
        //            droppedChunks.Add(chunk.Position);
        //        }
        //        else
        //        {
        //            bool b = false;
        //            for (int i = 0; i < players.Count; i++)
        //            {
        //                int radius = players[i].OverviewChunk + 1;
        //                vec2i min = players[i].ChunkPosManaged - radius;
        //                vec2i max = players[i].ChunkPosManaged + radius;
        //                if (chunk.Position.x >= min.x && chunk.Position.x <= max.x && chunk.Position.y >= min.y && chunk.Position.y <= max.y)
        //                {
        //                    b = true;
        //                    break;
        //                }
        //            }
        //            if (!b)
        //            {
        //                droppedChunks.Add(chunk.Position);
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// Получить количество
        /// </summary>
        public int Count => map.Count;

        /// <summary>
        /// Получить коллекцию значений
        /// </summary>
        public Dictionary<vec2i, ChunkBase>.ValueCollection Values() => map.Values;
    }
}
