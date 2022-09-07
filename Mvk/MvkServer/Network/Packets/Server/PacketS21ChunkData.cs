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

           // this.flagsYAreas = 0;
            this.flagsYAreas = flagsYAreas;
            if (flagsYAreas > 0)
            {
                List<ChunkStorage> storages = new List<ChunkStorage>();

                for (int y = 0; y < ChunkBase.COUNT_HEIGHT; y++)
                {
                    if (/*(!biom || !chunk.StorageArrays[y].IsEmptyData()) && */(flagsYAreas & 1 << y) != 0)
                    {
                        this.flagsYAreas |= 1 << y;
                        storages.Add(chunk.StorageArrays[y]);
                    }
                }
                buffer = new byte[storages.Count * CountBufChunck() + CountBufBiom()];
                int count = 0;
                ushort data;
                ChunkStorage chunkStorage;
                for (int j = 0; j < storages.Count; j++)
                {
                    chunkStorage = storages[j];
                    bool emptyData = chunkStorage.IsEmptyData();
                    for (int i = 0; i < 4096; i++)
                    {
                        //if (chunk.Position.x < 0 && chunk.Position.x > -8)
                        //{
                        //    buffer[count++] = 0;
                        //    buffer[count++] = 0;
                        //    buffer[count++] = 0xFF;
                        //}
                        //else
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
                    }
                }
                if (biom)
                {
                    // добавляем данные биома
                    for (int i = 0; i < 256; i++)
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
        /// количество буфер данных псевдо чанка // 16 * 16 * 16 * 3
        /// </summary>
        private int CountBufChunck() => 12288;
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
