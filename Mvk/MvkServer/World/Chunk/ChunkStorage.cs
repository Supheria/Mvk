using MvkServer.NBT;
using MvkServer.Util;
using MvkServer.World.Block;
using System;
using System.Collections.Generic;

namespace MvkServer.World.Chunk
{
    /// <summary>
    /// Псевдо чанк с данными вокселей
    /// 16 * 16 * 16
    /// y << 8 | z << 4 | x
    /// </summary>
    public class ChunkStorage
    {
        /// <summary>
        /// Уровень псевдочанка, нижнего блока, т.е. кратно 16. Глобальная координата Y, не чанка
        /// </summary>
        private readonly int yBase;
        /// <summary>
        /// Данные блока
        /// 12 bit Id блока и 4 bit параметр блока
        /// </summary>
        public ushort[] data;
        /// <summary>
        /// Освещение блочное, 4 bit используется
        /// </summary>
        public byte[] lightBlock;
        /// <summary>
        /// Освещение небесное, 4 bit используется
        /// </summary>
        public byte[] lightSky;
        /// <summary>
        /// Дополнительные данные блока
        /// </summary>
        public Dictionary<ushort, ushort> addMet = new Dictionary<ushort, ushort>();

        /// <summary>
        /// Количество блоков не воздуха
        /// </summary>
        public int countBlock;
        /// <summary>
        /// Количество блоков которым нужен тик
        /// </summary>
        private int countTickBlock;

        public ChunkStorage(int y)
        {
            yBase = y;
            data = null;
            countBlock = 0;
            countTickBlock = 0;
            lightBlock = new byte[4096];
            lightSky = new byte[4096];
        }

        /// <summary>
        /// Пустой, все блоки воздуха
        /// </summary>
        public bool IsEmptyData() => countBlock == 0;

        /// <summary>
        /// Очистить
        /// </summary>
        public void Clear()
        {
            data = null;
            lightBlock = new byte[4096];
            lightSky = new byte[4096];
            countBlock = 0;
            countTickBlock = 0;
        }

        #region Get

        /// <summary>
        /// Уровень псевдочанка
        /// </summary>
        public int GetYLocation() => yBase;

        /// <summary>
        /// Получить данные всего блока, XYZ 0..15 
        /// </summary>
        //public ushort GetData(int x, int y, int z) => data[y << 8 | z << 4 | x];

        /// <summary>
        /// Получить тип блок по координатам XYZ 0..15 
        /// </summary>
       // public int GetEBlock(int x, int y, int z) => data[y << 8 | z << 4 | x] & 0xFFF;

        /// <summary>
        /// Получить дополнительный параметр блока в 4 бита, XYZ 0..15 
        /// </summary>
        //public int GetMetadata(int x, int y, int z) => data[y << 8 | z << 4 | x] >> 12;

        /// <summary>
        /// Получить блок данных, XYZ 0..15 
        /// </summary>
        public BlockState GetBlockState(int x, int y, int z)
        {
            ushort index = (ushort)(y << 8 | z << 4 | x);
            try
            {
                ushort value = data[index];
                ushort id = (ushort)(value & 0xFFF);
                return new BlockState(id,
                    Blocks.blocksAddMet[id] ? addMet.ContainsKey(index) ? addMet[index] : (ushort)0 : (ushort)(value >> 12),
                    lightBlock[index], lightSky[index]);
            }
            catch(Exception ex)
            {
                Logger.Crach("ChunkStorage.GetBlockState countBlock {0} countTickBlock {1} index {2} data {3} null\r\n{4}",
                    countBlock,
                    countTickBlock,
                    index,
                    data == null ? "==" : "!=",
                    ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Получить байт освещения неба и блока, XYZ 0..15 
        /// </summary>
        //public byte GetLightsFor(int x, int y, int z) => light[y << 8 | z << 4 | x];

        /// <summary>
        /// Получить яркость блока от неба, XYZ 0..15 
        /// </summary>
        //public int GetLightSky(int x, int y, int z) => light[y << 8 | z << 4 | x] & 0xF;
        /// <summary>
        /// Получить яркость блока от блочного освещения, XYZ 0..15 
        /// </summary>
        //public int GetLightBlock(int x, int y, int z) => (light[y << 8 | z << 4 | x] & 0xF0) >> 4;

        #endregion

        #region Set

        /// <summary>
        /// Задать данные блока, XYZ 0..15 
        /// index = y << 8 | z << 4 | x
        /// </summary>
        public void SetData(int index, ushort id, ushort met = 0)
        {
            if (id == 0)
            {
                // воздух, проверка на чистку
                if (countBlock > 0 && (data[index] & 0xFFF) != 0)
                {
                    countBlock--;
                    if (Blocks.blocksRandomTick[data[index] & 0xFFF]) countTickBlock--;
                    addMet.Remove((ushort)index);
                    if (countBlock == 0)
                    {
                        data = null;
                        countTickBlock = 0;
                    }
                    else data[index] = 0;
                }
            }
            else
            {
                if (countBlock == 0)
                {
                    data = new ushort[4096];
                    countTickBlock = 0;
                }
                if ((data[index] & 0xFFF) == 0) countBlock++;
                bool rold = Blocks.blocksRandomTick[data[index] & 0xFFF];
                bool rnew = Blocks.blocksRandomTick[id];
                if (!rold && rnew) countTickBlock++;
                else if (rold && !rnew) countTickBlock--;
                ushort key = (ushort)index;
                if (Blocks.blocksAddMet[id])
                {
                    data[index] = id;
                    if (addMet.ContainsKey(key)) addMet[key] = met;
                    else addMet.Add(key, met);
                }
                else
                {
                    data[index] = (ushort)(id & 0xFFF | met << 12);
                    addMet.Remove(key);
                }
            }
        }

        /// <summary>
        /// Заменить только мет данные блока
        /// </summary>
        /// <param name="index">y << 8 | z << 4 | x</param>
        public void NewMetBlock(int index, ushort met)
        {
            if (countBlock > 0)
            {
                int id = data[index] & 0xFFF;
                ushort key = (ushort)index;
                if (Blocks.blocksAddMet[id])
                {
                    if (addMet.ContainsKey(key)) addMet[key] = met;
                    else addMet.Add(key, met);
                }
                else
                {
                    data[index] = (ushort)(id & 0xFFF | met << 12);
                    addMet.Remove(key);
                }
            }
        }

        /// <summary>
        /// Задать байт освещения неба и блока
        /// </summary>
        //public void SetLightsFor(int x, int y, int z, byte value) => light[y << 8 | z << 4 | x] = value;
        /// <summary>
        /// Задать яркость неба
        /// </summary>
        // public void SetLightSky(int x, int y, int z, byte value) => light[y << 8 | z << 4 | x] = (byte)(light[y << 8 | z << 4 | x] & 0xF0 | value);
        /// <summary>
        /// Задать яркость блока
        /// </summary>
        //  public void SetLightBlock(int x, int y, int z, byte value) => light[y << 8 | z << 4 | x] = (byte)(value << 4 | light[y << 8 | z << 4 | x] & 0xF);

        #endregion

        public void WriteDataToNBT(TagList nbt)
        {
            // нету блоков
            bool emptyB = IsEmptyData();
            // нету блоков освещения блока
            bool emptyLB = true;
            // нету блоков освещения неба
            bool emptyLS = true;
            for (int i = 0; i < 4096; i++)
            {
                if (lightBlock[i] > 0)
                {
                    emptyLB = false;
                    break;
                }
            }
            for (int i = 0; i < 4096; i++)
            {
                if (lightSky[i] < 15)
                {
                    emptyLS = false;
                    break;
                }
            }

            byte[] buffer;
            TagCompound tagCompound = new TagCompound();
            int count = 0;
            if (!emptyB)
            {
                buffer = new byte[8192];
                ushort data;
                for (int i = 0; i < 4096; i++)
                {
                    data = this.data[i];
                    buffer[count++] = (byte)(data & 0xFF);
                    buffer[count++] = (byte)(data >> 8);
                }
                tagCompound.SetByteArray("BlockStates", buffer);
                if (addMet.Count > 0)
                {
                    buffer = new byte[addMet.Count * 4];
                    count = 0;
                    foreach (KeyValuePair<ushort, ushort> entry in addMet)
                    {
                        buffer[count++] = (byte)(entry.Key & 0xFF);
                        buffer[count++] = (byte)(entry.Key >> 8);
                        buffer[count++] = (byte)(entry.Value & 0xFF);
                        buffer[count++] = (byte)(entry.Value >> 8);
                    }
                    tagCompound.SetByteArray("BlockAddMet", buffer);
                }
            }

            if (!emptyLB)
            {
                buffer = new byte[2048];
                // яркость от блоков
                for (int i = 0; i < 2048; i++)
                {
                    count = i * 2;
                    buffer[i] = (byte)((lightBlock[count] & 0xF) | (lightBlock[count + 1] << 4));
                }
                tagCompound.SetByteArray("BlockLight", buffer);
            }
            if (!emptyLS)
            {
                buffer = new byte[2048];
                // яркость от неба
                for (int i = 0; i < 2048; i++)
                {
                    count = i * 2;
                    buffer[i] = (byte)((lightSky[count] & 0xF) | (lightSky[count + 1] << 4));
                }
                tagCompound.SetByteArray("SkyLight", buffer);
            }

            if (!emptyB || !emptyLB || !emptyLS)
            {
                tagCompound.SetByte("Y", (byte)(yBase >> 4));
                nbt.AppendTag(tagCompound);
            }
        }

        public void ReadDataFromNBT(TagCompound nbt)
        {
            int count = 0;
            int i;
            byte[] buffer = nbt.GetByteArray("BlockStates");
            // нету блоков
            if (buffer.Length == 8192)
            {
                for (i = 0; i < 4096; i++)
                {
                    int value = buffer[count++] | buffer[count++] << 8;
                    SetData(i, (ushort)(value & 0xFFF), (ushort)(value >> 12));
                }
            }
            buffer = nbt.GetByteArray("BlockAddMet");
            addMet.Clear();
            count = 0;
            int countMet = buffer.Length / 4;
            for (i = 0; i < countMet; i++)
            {
                addMet.Add(
                    (ushort)(buffer[count++] | buffer[count++] << 8),
                    (ushort)(buffer[count++] | buffer[count++] << 8)
                );
            }

            buffer = nbt.GetByteArray("BlockLight");
            // нету блоков освещения блока
            if (buffer.Length == 2048)
            {
                lightBlock = new byte[4096];
                for (i = 0; i < 2048; i++)
                {
                    count = i * 2;
                    lightBlock[count] = (byte)(buffer[i] & 0xF);
                    lightBlock[count + 1] = (byte)(buffer[i] >> 4);
                }
            }
            else
            {
                for (i = 0; i < 4096; i++) lightBlock[i] = 0;
            }

            buffer = nbt.GetByteArray("SkyLight");
            // нету блоков освещения неба
            if (buffer.Length == 2048)
            {
                lightSky = new byte[4096];
                for (i = 0; i < 2048; i++)
                {
                    count = i * 2;
                    lightSky[count] = (byte)(buffer[i] & 0xF);
                    lightSky[count + 1] = (byte)(buffer[i] >> 4);
                }
            }
            else
            {
                for (i = 0; i < 4096; i++) lightSky[i] = 15;
            }
        }

        /// <summary>
        /// Имеются ли блоки которым нужен случайный тик
        /// </summary>
        public bool GetNeedsRandomTick() => countTickBlock > 0;

        public override string ToString() => "yB:" + yBase + " body:" + countBlock + " ";

        public string ToCountString() => countBlock.ToString() + "|" + countTickBlock.ToString();
    }
}
