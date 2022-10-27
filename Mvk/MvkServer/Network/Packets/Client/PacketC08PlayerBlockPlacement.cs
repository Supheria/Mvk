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
        private bool activated;

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
        /// Действие на блок правой клавишей мыши, клик
        /// </summary>
        public bool GetActivated() => activated;

        public PacketC08PlayerBlockPlacement(BlockPos blockPos, Pole side, vec3 facing, bool activated)
        {
            this.blockPos = blockPos;
            this.side = side;
            this.facing = facing;
            this.activated = activated;
        }

        public void ReadPacket(StreamBase stream)
        {
            blockPos = new BlockPos(stream.ReadInt(), stream.ReadInt(), stream.ReadInt());
            side = (Pole)stream.ReadByte();
            facing = new vec3(stream.ReadByte() / 16f, stream.ReadByte() / 16f, stream.ReadByte() / 16f);
            activated = stream.ReadBool();
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteInt(blockPos.X);
            stream.WriteInt(blockPos.Y);
            stream.WriteInt(blockPos.Z);
            stream.WriteByte((byte)side);
            stream.WriteByte((byte)(facing.x * 16f));
            stream.WriteByte((byte)(facing.y * 16f));
            stream.WriteByte((byte)(facing.z * 16f));
            stream.WriteBool(activated);
        }
    }
}
