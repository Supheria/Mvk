using MvkServer.Entity.List;

namespace MvkServer.Network.Packets.Server
{
    /// <summary>
    /// Пакет атрибу игрока
    /// </summary>
    public struct PacketS39PlayerAbilities : IPacket
    {
        private bool isCreativeMode;
        private bool noClip;
        private bool allowFlying;
        private bool disableDamage;

        public bool IsCreativeMode() => isCreativeMode;
        public bool IsNoClip() => noClip;
        public bool IsAllowFlying() => allowFlying;
        public bool IsDisableDamage() => disableDamage;

        public PacketS39PlayerAbilities(EntityPlayer player)
        {
            isCreativeMode = player.IsCreativeMode;
            noClip = player.NoClip;
            allowFlying = player.AllowFlying;
            disableDamage = player.DisableDamage;
        }

        public void ReadPacket(StreamBase stream)
        {
            isCreativeMode = stream.ReadBool();
            noClip = stream.ReadBool();
            allowFlying = stream.ReadBool();
            disableDamage = stream.ReadBool();
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteBool(isCreativeMode);
            stream.WriteBool(noClip);
            stream.WriteBool(allowFlying);
            stream.WriteBool(disableDamage);
        }
    }
}
