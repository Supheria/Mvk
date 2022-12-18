﻿using MvkServer.Entity;
using MvkServer.Glm;

namespace MvkServer.Network.Packets.Server
{
    /// <summary>
    /// Движение сущности
    /// </summary>
    public struct PacketS14EntityMotion : IPacket
    {
        private ushort id;
        private vec3 pos;
        private float yaw;
        private float pitch;
        private bool onGround;

        public ushort GetId() => id;
        public vec3 GetPos() => pos;
        public float GetYaw() => yaw;
        public float GetPitch() => pitch;
        public bool OnGround() => onGround;

        public PacketS14EntityMotion(EntityBase entity)
        {
            id = entity.Id;
            pos = entity.Position;
            if (entity is EntityLiving entityLiving)
            {
                yaw = entityLiving.RotationYawHead;
                pitch = entityLiving.RotationPitch;
            }
            else
            {
                yaw = pitch = 0;
            }
            onGround = entity.OnGround;
        }

        public void ReadPacket(StreamBase stream)
        {
            id = stream.ReadUShort();
            pos = new vec3(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
            yaw = stream.ReadFloat();
            pitch = stream.ReadFloat();
            onGround = stream.ReadBool();
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteUShort(id);
            stream.WriteFloat(pos.x);
            stream.WriteFloat(pos.y);
            stream.WriteFloat(pos.z);
            stream.WriteFloat(yaw);
            stream.WriteFloat(pitch);
            stream.WriteBool(onGround);
        }
    }
}
