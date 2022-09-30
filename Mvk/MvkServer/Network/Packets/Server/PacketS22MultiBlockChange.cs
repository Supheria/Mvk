using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World;
using MvkServer.World.Block;
using MvkServer.World.Chunk;

namespace MvkServer.Network.Packets.Server
{
    /// <summary>
    /// Отправляем клиенту много изменённых блоков
    /// </summary>
    public struct PacketS22MultiBlockChange : IPacket
    {
        private vec2i chPos;
        private BlockUpdateData[] blocks;

        public PacketS22MultiBlockChange(int count, vec3i[] pos, ChunkBase chunk)
        {
            chPos = chunk.Position;
            blocks = new BlockUpdateData[count];
            int x, y, z;
            for (int i = 0; i < count; i++)
            {
                x = pos[i].x;
                y = pos[i].y;
                z = pos[i].z;

                blocks[i] = new BlockUpdateData()
                {
                    xz = (byte)(z << 4 | x),
                    y = (byte)y,
                    blockState = chunk.GetBlockStateNotCheck(x, y, z)
                };
            }
        }

        /// <summary>
        /// Получили блоки
        /// </summary>
        public void ReceivedBlocks(WorldBase world)
        {
            ChunkBase chunk = world.GetChunk(chPos);
            if (chunk != null)
            {
                BlockPos blockPos = new BlockPos();
                int chx = chunk.Position.x << 4;
                int chz = chunk.Position.y << 4;
                byte xz;
                for (int i = 0; i < blocks.Length; i++)
                {
                    xz = blocks[i].xz;
                    blockPos.X = chx + (xz & 0xF);
                    blockPos.Y = blocks[i].y;
                    blockPos.Z = chz + (xz >> 4);
                    chunk.SetBlockState(blockPos, blocks[i].blockState, false, true);
                }
            }
        }

        public void ReadPacket(StreamBase stream)
        {
            chPos = new vec2i(stream.ReadInt(), stream.ReadInt());
            ushort count = stream.ReadUShort();
            blocks = new BlockUpdateData[count];
            for (int i = 0; i < count; i++)
            {
                blocks[i] = new BlockUpdateData()
                {
                    xz = stream.ReadByte(),
                    y = stream.ReadByte()
                };
                blocks[i].blockState.ReadStream(stream);
            }
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteInt(chPos.x);
            stream.WriteInt(chPos.y);
            stream.WriteUShort((ushort)blocks.Length);
            for (int i = 0; i < blocks.Length; i++)
            {
                stream.WriteByte(blocks[i].xz);
                stream.WriteByte(blocks[i].y);
                blocks[i].blockState.WriteStream(stream);
            }
        }

        private struct BlockUpdateData
        {
            public byte y;
            public byte xz;
            public BlockState blockState;
        }
    }
}
