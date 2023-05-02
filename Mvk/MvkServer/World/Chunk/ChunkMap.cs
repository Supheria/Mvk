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
        private Dictionary<vec2i, ChunkBase> map = new Dictionary<vec2i, ChunkBase>();
        private Dictionary<vec2i, RgionChunk> reg = new Dictionary<vec2i, RgionChunk>();

        /// <summary>
        /// Добавить или изменить чанк
        /// </summary>
        public void Set(ChunkBase chunk)
        {
            chunk.UpdateTime();
            try
            {
                int chx = chunk.Position.x;
                int chz = chunk.Position.y;
                vec2i r = new vec2i(chx >> 5, chz >> 5);
                if (!reg.ContainsKey(r))
                {
                    reg.Add(r, new RgionChunk());
                }
                reg[r].Set(chx, chz, chunk);
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
            int chx = pos.x;
            int chz = pos.y;
            vec2i r = new vec2i(chx >> 5, chz >> 5);
            if (reg.ContainsKey(r))
            {
                return reg[r].Get(chx, chz);
            }
            //if (map.ContainsKey(pos))
            //{
            //    return map[pos];
            //}
            return null;
        }

        /// <summary>
        /// Проверить наличие чанка
        /// </summary>
        public bool Contains(ChunkBase chunk)
        {
            return Contains(chunk.Position);
            //map.ContainsKey(chunk.Position);
        }
        /// <summary>
        /// Проверить наличие чанка
        /// </summary>
        public bool Contains(vec2i pos)
        {
            int chx = pos.x;
            int chz = pos.y;
            vec2i r = new vec2i(chx >> 5, chz >> 5);
            if (reg.ContainsKey(r)) return reg[r].Get(chx, chz) != null;
            return false;
        }
        //=> map.ContainsKey(pos);

        /// <summary>
        /// Очистить
        /// </summary>
        public void Clear()
        {
            reg.Clear();
            map.Clear();
            //=> map.Clear();
        }

        /// <summary>
        /// Удалить
        /// </summary>
        public void Remove(vec2i pos)
        {
            ChunkBase chunk = Get(pos);
            if (chunk != null && chunk.IsChunkPresent)
            {
                chunk.OnChunkUnload();
            }
            map.Remove(pos);
            int chx = pos.x;
            int chz = pos.y;
            vec2i r = new vec2i(chx >> 5, chz >> 5);
            if (reg.ContainsKey(r))
            {
                if (reg[r].Remove(chx, chz))
                {
                    reg.Remove(r);
                }
            }
        }

        /// <summary>
        /// Получить количество
        /// </summary>
        public int Count => map.Count;

        /// <summary>
        /// Получить коллекцию чанков
        /// </summary>
        public Dictionary<vec2i, ChunkBase>.ValueCollection Values() => map.Values;

        /// <summary>
        /// Получить коллекцию позиций чанков
        /// </summary>
        public Dictionary<vec2i, ChunkBase>.KeyCollection Keys() => map.Keys;

        private class RgionChunk
        {
            private readonly ChunkBase[] ar = new ChunkBase[1024];
            private int count = 0;

            public ChunkBase Get(int chx, int chz) => ar[(chz & 31) << 5 | (chx & 31)];

            public void Set(int chx, int chz, ChunkBase chunk)
            {
                int index = (chz & 31) << 5 | (chx & 31);
                if (ar[index] == null) count++;
                ar[index] = chunk;
            }

            public bool Remove(int chx, int chz)
            {
                int index = (chz & 31) << 5 | (chx & 31);
                if (ar[index] != null)
                {
                    ar[index] = null;
                    count--;
                }
                if (count <= 0) return true;
                return false;
            }

            public override string ToString() => count.ToString();
        }
    }
}
