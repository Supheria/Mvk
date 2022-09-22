using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Chunk;
using System.Collections.Generic;
using System.IO;

namespace MvkServer.Network.Packets.Server
{
    /// <summary>
    /// Отправляем клиенту изменённые псевдо чанки
    /// </summary>
    public struct PacketS21ChunkData : IPacket
    {
        private vec2i pos;
        private byte[] buffer;
        private int flagsYAreas;
        private bool biom;

        /// <summary>
        /// Буффер псевдо чанка
        /// </summary>
        public byte[] GetBuffer() => buffer;
        /// <summary>
        /// Позиция
        /// </summary>
        public vec2i GetPos() => pos;
        /// <summary>
        /// Удалить чанк
        /// </summary>
        public bool IsRemoved() => flagsYAreas == 0;
        /// <summary>
        /// Данные столбца биома, как правило при первой загрузке
        /// </summary>
        public bool IsBiom() => biom;
        /// <summary>
        /// Флаг псевдо чанков
        /// </summary>
        public int GetFlagsYAreas() => flagsYAreas;

        public PacketS21ChunkData(ChunkBase chunk, bool biom, int flagsYAreas)
        {
            pos = chunk.Position;
            this.biom = biom;
            buffer = new byte[0];

            this.flagsYAreas = flagsYAreas;
            if (flagsYAreas > 0)
            {
                ushort data, countMet;
                int i, j, count;
                List<ChunkStorage> storages = new List<ChunkStorage>();
                count = 0;
                for (int y = 0; y < ChunkBase.COUNT_HEIGHT; y++)
                {
                    if ((flagsYAreas & 1 << y) != 0)
                    {
                        this.flagsYAreas |= 1 << y;
                        storages.Add(chunk.StorageArrays[y]);
                        // Считаем количество дополнительных метданных
                        count += chunk.StorageArrays[y].addMet.Count;
                    }
                }

                buffer = new byte[storages.Count * CountBufChunck() + CountBufBiom() + count * 4];
                count = 0;
                
                ChunkStorage chunkStorage;
                for (j = 0; j < storages.Count; j++)
                {
                    chunkStorage = storages[j];
                    bool emptyData = chunkStorage.IsEmptyData();
                    for (i = 0; i < 4096; i++)
                    {
                        if (emptyData)
                        {
                            buffer[count++] = 0;
                            buffer[count++] = 0;
                        }
                        else
                        {
                            data = chunkStorage.data[i];
                            buffer[count++] = (byte)(data & 0xFF);
                            buffer[count++] = (byte)(data >> 8);
                        }
                        buffer[count++] = (byte)(chunkStorage.lightBlock[i] << 4 | chunkStorage.lightSky[i] & 0xF);
                    }
                    countMet = (ushort)chunkStorage.addMet.Count;
                    buffer[count++] = (byte)(countMet & 0xFF);
                    buffer[count++] = (byte)(countMet >> 8);
                    foreach (KeyValuePair<ushort, ushort> entry in chunkStorage.addMet)
                    {
                        buffer[count++] = (byte)(entry.Key & 0xFF);
                        buffer[count++] = (byte)(entry.Key >> 8);
                        buffer[count++] = (byte)(entry.Value & 0xFF);
                        buffer[count++] = (byte)(entry.Value >> 8);
                    }
                }
                if (biom)
                {
                    // добавляем данные биома
                    for (i = 0; i < 256; i++)
                    {
                        buffer[count++] = (byte)chunk.biome[i];
                    }
                }
            }
        }

        /// <summary>
        /// Получить количество данных в псевдо чанке
        /// </summary>
        //private int CountBuffer()
        //{
        //    // количество буфер данных
        //    int countBuf = 12288; // 16 * 16 * 16 * 3
        //    int countHeight = biom ? 256 : 0; // 16 * 16 
        //    return countBuf + countHeight;
        //}

        /// <summary>
        /// количество буфер данных псевдо чанка // 16 * 16 * 16 * 3 + 2 на доп метданных
        /// </summary>
        private int CountBufChunck() => 12290;
        /// <summary>
        /// количество буфер данных для биома чанка // 16 * 16
        /// </summary>
        private int CountBufBiom() => biom ? 256 : 0;

        /// <summary>
        /// Количество псевдо чанков по флагу
        /// </summary>
        private int CountChunk()
        {
            int countChunk = 0;
            for (int y = 0; y < ChunkBase.COUNT_HEIGHT; y++)
            {
                if ((flagsYAreas & 1 << y) != 0) countChunk++;
            }
            return countChunk;
        }

        public void ReadPacket(StreamBase stream)
        {
            pos = new vec2i(stream.ReadInt(), stream.ReadInt());
            biom = stream.ReadBool();
            flagsYAreas = stream.ReadUShort();
            if (flagsYAreas > 0)
            {
                int count = stream.ReadInt();
                buffer = stream.Decompress(stream.ReadBytes(count));
                //buffer = stream.ReadBytes(CountChunk() * CountBufChunck() + CountBufBiom());
            }
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteInt(pos.x);
            stream.WriteInt(pos.y);
            stream.WriteBool(biom);
            stream.WriteUShort((ushort)flagsYAreas);
            if (flagsYAreas > 0)
            {
                byte[] buf = stream.Compress(buffer);
                stream.WriteInt(buf.Length);
                stream.WriteBytes(buf);
                //stream.WriteBytes(buffer);
            }
        }
    }
}
