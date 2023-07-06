using MvkServer.Glm;

namespace MvkServer.Util
{
    /// <summary>
    /// Вершина имеет xyz и текстуру uv
    /// </summary>
    public struct Vertex
    {
        public float x;
        public float y;
        public float z;

        public float u;
        public float v;

        public Vertex(float x, float y, float z, float u, float v)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.u = u;
            this.v = v;
        }

        public vec3 ToPosition() => new vec3(x, y, z);

        public Vertex Copy() => new Vertex(x, y, z, u, v);

        public override string ToString() => string.Format("{0:0.00}; {1:0.00}; {2:0.00} u{3:0.00} v{4:0.00}]", x, y, z, u, v);
    }
}
