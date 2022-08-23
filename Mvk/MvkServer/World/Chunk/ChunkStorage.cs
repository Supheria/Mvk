using MvkServer.Util;
using MvkServer.World.Block;
using System;

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
            int index = y << 8 | z << 4 | x;
            try
            {
                return new BlockState(data[index], lightBlock[index], lightSky[index]);
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
        public void SetData(int index, ushort value)
        {
            if ((value & 0xFFF) == 0)
            {
                // воздух, проверка на чистку
                if (countBlock > 0 && (data[index] & 0xFFF) != 0)
                {
                    countBlock--;
                    if (Blocks.blocksRandomTick[data[index] & 0xFFF]) countTickBlock--;
                    if (countBlock == 0)
                    {
                        data = null;
                        countTickBlock = 0;
                    }
                    else data[index] = value;
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
                bool rnew = Blocks.blocksRandomTick[value & 0xFFF];
                if (!rold && rnew) countTickBlock++;
                else if (rold && !rnew) countTickBlock--;
                data[index] = value;
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

        /// <summary>
        /// Имеются ли блоки которым нужен случайный тик
        /// </summary>
        public bool GetNeedsRandomTick() => countTickBlock > 0;

        public override string ToString() => "yB:" + yBase + " body:" + countBlock + " ";

        public string ToCountString() => countBlock.ToString() + "|" + countTickBlock.ToString();
    }
}
