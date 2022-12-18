using MvkServer.Glm;
using MvkServer.Util;

namespace MvkServer.Network.Packets.Client
{
    /// <summary>
    /// Отправляем на сервер установку блока или клик
    /// </summary>
    public struct PacketC08PlayerBlockPlacement : IPacket
    {
        private BlockPos blockPos;
        private Pole side;
        private vec3 facing;
        /// <summary>
        /// Flag: 0 - PutBlock, 1 - BlockActivated, 2 - UseItemRightClick
        /// </summary>
        private byte flag;

        /// <summary>
        /// Позиция блока где устанавливаем блок
        /// </summary>
        public BlockPos GetBlockPos() => blockPos;
        /// <summary>
        /// Точка куда устанавливаем блок (параметр с RayCast)
        /// значение в пределах 0..1, образно фиксируем пиксел клика на стороне
        /// </summary>
        public vec3 GetFacing() => facing;
        /// <summary>
        /// С какой стороны устанавливаем блок
        /// </summary>
        public Pole GetSide() => side;
        /// <summary>
        /// Flag: 0 - PutBlock, 1 - BlockActivated, 2 - UseItemRightClick
        /// </summary>
        public byte GetFlag() => flag;

        public PacketC08PlayerBlockPlacement(BlockPos blockPos, Pole side, vec3 facing)
        {
            this.blockPos = blockPos;
            this.side = side;
            this.facing = facing;
            flag = 0;
        }
        /// <summary>
        /// Отправляем на сервер правй клик. Flag: 0 - PutBlock, 1 - BlockActivated, 2 - UseItemRightClick
        /// </summary>
        public PacketC08PlayerBlockPlacement(BlockPos blockPos)
        {
            this.blockPos = blockPos;
            side = Pole.All;
            facing = new vec3(0);
            flag = 1;
        }

        /// <summary>
        /// Отправляем на сервер правй клик. Flag: 0 - PutBlock, 1 - BlockActivated, 2 - UseItemRightClick
        /// </summary>
        public PacketC08PlayerBlockPlacement(byte flag)
        {
            blockPos = new BlockPos();
            side = Pole.All;
            facing = new vec3(0);
            this.flag = flag;
        }

        public void ReadPacket(StreamBase stream)
        {
            flag = stream.ReadByte();
            if (flag < 2)
            {
                blockPos = new BlockPos(stream.ReadInt(), stream.ReadInt(), stream.ReadInt());
            }
            if (flag == 0)
            {
                side = (Pole)stream.ReadByte();
                facing = new vec3(stream.ReadByte() / 16f, stream.ReadByte() / 16f, stream.ReadByte() / 16f);
            }
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteByte(flag);
            if (flag < 2)
            {
                stream.WriteInt(blockPos.X);
                stream.WriteInt(blockPos.Y);
                stream.WriteInt(blockPos.Z);
            }
            if (flag == 0)
            {
                stream.WriteByte((byte)side);
                stream.WriteByte((byte)(facing.x * 16f));
                stream.WriteByte((byte)(facing.y * 16f));
                stream.WriteByte((byte)(facing.z * 16f));
            }
        }
    }
}
