using MvkServer.Network;

namespace MvkServer.World.Block
{
    /// <summary>
    /// Бинарные данные блока
    /// </summary>
    public struct BlockState
    {
        /// <summary>
        /// ID блока 12 bit
        /// </summary>
        public ushort id;
        /// <summary>
        /// Дополнительные параметры блока 4 bita или если IsAddMet то 16 bit;
        /// </summary>
        public ushort met;
        /// <summary>
        /// Освещение блочное, 4 bit используется
        /// </summary>
        public byte lightBlock;
        /// <summary>
        /// Освещение небесное, 4 bit используется
        /// </summary>
        public byte lightSky;

        public BlockState(ushort id, ushort met = 0, byte lightBlock = 0, byte lightSky = 0)
        {
            this.id = id;
            this.met = met;
            this.lightBlock = lightBlock;
            this.lightSky = lightSky;
        }
        public BlockState(EnumBlock eBlock)
        {
            id = (ushort)eBlock;
            met = 0;
            lightBlock = 0;
            lightSky = 0;
        }

        /// <summary>
        /// Пустой ли объект
        /// </summary>
        public bool IsEmpty() => id == 0 && met == 1;
        /// <summary>
        /// Пометить пустой блок
        /// </summary>
        public BlockState Empty()
        {
            // id воздуха, но мет данные 1
            id = 0;
            met = 1;
            return this;
        }

        /// <summary>
        /// Получить тип блок
        /// </summary>
        public EnumBlock GetEBlock() => (EnumBlock)id;

        /// <summary>
        /// Получить кэш блока
        /// </summary>
        public BlockBase GetBlock() => Blocks.GetBlockCache(GetEBlock());

        /// <summary>
        /// Веррнуть новый BlockState с новыйм мет данные
        /// </summary>
        public BlockState NewMet(ushort met) => new BlockState(id, met, lightBlock, lightSky);

        /// <summary>
        /// Записать блок в буффер пакета
        /// </summary>
        public void WriteStream(StreamBase stream)
        {
            stream.WriteUShort(id);
            stream.WriteUShort(met);
            stream.WriteByte((byte)(lightBlock << 4 | lightSky & 0xF));
            //stream.WriteByte(lightBlock);
            //stream.WriteByte(lightSky);
        }

        /// <summary>
        /// Прочесть блок из буффер пакета и занести в чанк
        /// </summary>
        public void ReadStream(StreamBase stream)
        {
            id = stream.ReadUShort();
            met = stream.ReadUShort();
            byte light = stream.ReadByte();
            lightBlock = (byte)(light >> 4);
            lightSky = (byte)(light & 0xF);
            //lightBlock = stream.ReadByte();
            //lightSky = stream.ReadByte();
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(BlockState))
            {
                var vec = (BlockState)obj;
                if (id == vec.id && met == vec.met && lightBlock == vec.lightBlock && lightSky == vec.lightSky) return true;
            }
            return false;
        }

        public override int GetHashCode() => id ^ met ^ lightBlock ^ lightSky;

        public override string ToString()
        {
            return string.Format("#{0} M:{1}", id, met);
        }
    }
}
