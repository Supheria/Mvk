using MvkServer.Glm;

namespace MvkServer.Network.Packets.Client
{
    /// <summary>
    /// Пакет расположения игрока вместе с камерой
    /// </summary>
    public struct PacketC06PlayerPosLook : IPacket
    {
        private vec3 pos;
        private float yaw;
        private float pitch;
        private bool sneaking;
        private bool sprinting;
        private bool onGround;

        public vec3 GetPos() => pos;
        public float GetYaw() => yaw;
        public float GetPitch() => pitch;
        public bool IsSneaking() => sneaking;
        public bool IsSprinting() => sprinting;
        public bool OnGround() => onGround;

        public PacketC06PlayerPosLook(vec3 pos, float yaw, float pitch, bool sneaking, bool sprinting, bool onGround)
        {
            this.pos = pos;
            this.yaw = yaw;
            this.pitch = pitch;
            this.sneaking = sneaking;
            this.sprinting = sprinting;
            this.onGround = onGround;
        }

        public void ReadPacket(StreamBase stream)
        {
            pos = new vec3(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
            yaw = stream.ReadFloat();
            pitch = stream.ReadFloat();
            sneaking = stream.ReadBool();
            sprinting = stream.ReadBool();
            onGround = stream.ReadBool();
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteFloat(pos.x);
            stream.WriteFloat(pos.y);
            stream.WriteFloat(pos.z);
            stream.WriteFloat(yaw);
            stream.WriteFloat(pitch);
            stream.WriteBool(sneaking);
            stream.WriteBool(sprinting);
            stream.WriteBool(onGround);
        }
    }
}
