using MvkServer.Glm;
using System.Collections.Generic;

namespace MvkServer.Util
{
    /// <summary>
    /// Объект для отладки чанков
    /// </summary>
    public class DebugChunk
    {
        public List<DebugChunkValue> listChunkServer = new List<DebugChunkValue>();
        public List<vec2i> listChunkPlayers = new List<vec2i>();
        public List<DebugChunkValue> listChunkPlayer = new List<DebugChunkValue>();
        public bool isRender;
    }

    public struct DebugChunkValue
    {
        public vec2i pos;
        public bool entities;
    }
}
