using MvkServer.Entity;
using MvkServer.Glm;

namespace MvkServer.Network.Packets.Server
{
    /// <summary>
    /// Частички
    /// </summary>
    public struct PacketS2AParticles : IPacket
    {
        private EnumParticle particle;
        private vec3 position;
        private vec3 offset;
        private float motion;
        private int[] items;
        private int count;

        public EnumParticle GetParticle() => particle;
        public vec3 GetPosition() => position;
        public vec3 GetOffset() => offset;
        public float GetMotion() => motion;
        public int[] GetItems() => items;
        public int GetCount() => count;

        public PacketS2AParticles(EnumParticle particle, int count, vec3 position, vec3 offset, float motion, int[] items)
        {
            this.particle = particle;
            this.position = position;
            this.offset = offset;
            this.motion = motion;
            this.count = count;
            this.items = items;
        }

        public void ReadPacket(StreamBase stream)
        {
            particle = (EnumParticle)stream.ReadUShort();
            position = new vec3(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
            offset = new vec3(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
            motion = stream.ReadFloat();
            count = stream.ReadByte();
            int l = stream.ReadByte();
            items = new int[l];
            for (int i = 0; i < l; i++)
            {
                items[i] = stream.ReadInt();
            }
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteUShort((ushort)particle);
            stream.WriteFloat(position.x);
            stream.WriteFloat(position.y);
            stream.WriteFloat(position.z);
            stream.WriteFloat(offset.x);
            stream.WriteFloat(offset.y);
            stream.WriteFloat(offset.z);
            stream.WriteFloat(motion);
            stream.WriteByte((byte)count);
            stream.WriteByte((byte)items.Length);
            for (int i = 0; i < items.Length; i++)
            {
                stream.WriteInt(items[i]);
            }
        }
    }
}
