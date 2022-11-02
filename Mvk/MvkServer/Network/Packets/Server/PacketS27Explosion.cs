using MvkServer.Glm;

namespace MvkServer.Network.Packets.Server
{
    public struct PacketS27Explosion : IPacket
    {
        /// <summary>
        /// Позиция взрыва
        /// </summary>
        public vec3 pos;
        /// <summary>
        /// Сила, для мощности ломания блоков
        /// </summary>
        //public float strength;
        /// <summary>
        /// Дистанция радиус разлёта
        /// </summary>
        public float distance;
        /// <summary>
        /// Смещение
        /// </summary>
        public vec3 motion;

        public PacketS27Explosion(vec3 pos, float strength, float distance, vec3 motion)
        {
            this.pos = pos;
            this.distance = distance;
            this.motion = motion;
        }

        public void ReadPacket(StreamBase stream)
        {
            pos = new vec3(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
            distance = stream.ReadFloat();
            if (stream.ReadBool())
            {
                motion = new vec3(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
            }
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteFloat(pos.x);
            stream.WriteFloat(pos.y);
            stream.WriteFloat(pos.z);
            stream.WriteFloat(distance);
            if (motion.x == 0 && motion.y == 0 && motion.z == 0)
            {
                stream.WriteBool(false);
            }
            else
            {
                stream.WriteBool(true);
                stream.WriteFloat(motion.x);
                stream.WriteFloat(motion.y);
                stream.WriteFloat(motion.z);
            }
        }
    }
}
