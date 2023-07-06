using MvkServer.Util;
using System;

namespace MvkClient.Renderer.Block
{
    /// <summary>
    /// Построение полного блока с разных сторон
    /// </summary>
    public class BlockSide
    {
        public ArrayMvk<byte> buffer;
        /// <summary>
        /// Буфер кэш для внутренней части жидких блоков
        /// </summary>
        public ArrayMvk<byte> bufferCache;

        public Vertex[] vertex;

        public byte[] colorsr;
        public byte[] colorsg;
        public byte[] colorsb;
        public byte[] lights;
        public float posCenterX;
        public float posCenterY;
        public float posCenterZ;

        public byte animationFrame;
        public byte animationPause;

        private float pos1x, pos1y, pos1z, pos2x, pos2y, pos2z, pos3x, pos3y, pos3z, pos4x, pos4y, pos4z;
        private float u1, u2, u3, u4, v1, v2, v3, v4;

        /// <summary>
        /// Построение буфера
        /// </summary>
        public void Building()
        {
            pos1x = vertex[0].x + posCenterX;
            pos1y = vertex[0].y + posCenterY;
            pos1z = vertex[0].z + posCenterZ;
            pos2x = vertex[1].x + posCenterX;
            pos2y = vertex[1].y + posCenterY;
            pos2z = vertex[1].z + posCenterZ;
            pos3x = vertex[2].x + posCenterX;
            pos3y = vertex[2].y + posCenterY;
            pos3z = vertex[2].z + posCenterZ;
            pos4x = vertex[3].x + posCenterX;
            pos4y = vertex[3].y + posCenterY;
            pos4z = vertex[3].z + posCenterZ;

            u1 = vertex[0].u;
            u2 = vertex[1].u;
            u3 = vertex[2].u;
            u4 = vertex[3].u;
            v1 = vertex[0].v;
            v2 = vertex[1].v;
            v3 = vertex[2].v;
            v4 = vertex[3].v;

            // снаружи
            AddVertex(pos1x, pos1y, pos1z, u1, v1, colorsr[0], colorsg[0], colorsb[0], lights[0]);
            AddVertex(pos2x, pos2y, pos2z, u2, v2, colorsr[1], colorsg[1], colorsb[1], lights[1]);
            AddVertex(pos3x, pos3y, pos3z, u3, v3, colorsr[2], colorsg[2], colorsb[2], lights[2]);
            AddVertex(pos1x, pos1y, pos1z, u1, v1, colorsr[0], colorsg[0], colorsb[0], lights[0]);
            AddVertex(pos3x, pos3y, pos3z, u3, v3, colorsr[2], colorsg[2], colorsb[2], lights[2]);
            AddVertex(pos4x, pos4y, pos4z, u4, v4, colorsr[3], colorsg[3], colorsb[3], lights[3]);
        }

        /// <summary>
        /// Построение буфера
        /// </summary>
        public void BuildingWind(byte wind)
        {
            pos1x = vertex[0].x + posCenterX;
            pos1y = vertex[0].y + posCenterY;
            pos1z = vertex[0].z + posCenterZ;
            pos2x = vertex[1].x + posCenterX;
            pos2y = vertex[1].y + posCenterY;
            pos2z = vertex[1].z + posCenterZ;
            pos3x = vertex[2].x + posCenterX;
            pos3y = vertex[2].y + posCenterY;
            pos3z = vertex[2].z + posCenterZ;
            pos4x = vertex[3].x + posCenterX;
            pos4y = vertex[3].y + posCenterY;
            pos4z = vertex[3].z + posCenterZ;

            u1 = vertex[0].u;
            u2 = vertex[1].u;
            u3 = vertex[2].u;
            u4 = vertex[3].u;
            v1 = vertex[0].v;
            v2 = vertex[1].v;
            v3 = vertex[2].v;
            v4 = vertex[3].v;

            if (wind == 0)
            {
                // Нет ветра
                AddVertex(pos1x, pos1y, pos1z, u1, v1, colorsr[0], colorsg[0], colorsb[0], lights[0]);
                AddVertex(pos2x, pos2y, pos2z, u2, v2, colorsr[1], colorsg[1], colorsb[1], lights[1]);
                AddVertex(pos3x, pos3y, pos3z, u3, v3, colorsr[2], colorsg[2], colorsb[2], lights[2]);
                AddVertex(pos1x, pos1y, pos1z, u1, v1, colorsr[0], colorsg[0], colorsb[0], lights[0]);
                AddVertex(pos3x, pos3y, pos3z, u3, v3, colorsr[2], colorsg[2], colorsb[2], lights[2]);
                AddVertex(pos4x, pos4y, pos4z, u4, v4, colorsr[3], colorsg[3], colorsb[3], lights[3]);
            }
            else if (wind == 1)
            {
                // Ветер как для травы, низ не двигается, вверх двигается
                AddVertex(pos1x, pos1y, pos1z, u1, v1, colorsr[0], colorsg[0], colorsb[0], lights[0], 0);
                AddVertex(pos2x, pos2y, pos2z, u2, v2, colorsr[1], colorsg[1], colorsb[1], lights[1], 1);
                AddVertex(pos3x, pos3y, pos3z, u3, v3, colorsr[2], colorsg[2], colorsb[2], lights[2], 1);
                AddVertex(pos1x, pos1y, pos1z, u1, v1, colorsr[0], colorsg[0], colorsb[0], lights[0], 0);
                AddVertex(pos3x, pos3y, pos3z, u3, v3, colorsr[2], colorsg[2], colorsb[2], lights[2], 1);
                AddVertex(pos4x, pos4y, pos4z, u4, v4, colorsr[3], colorsg[3], colorsb[3], lights[3], 0);
            }
            else if (wind == 2)
            {
                // Ветер как для ветки снизу, вверхняя часть не двигается, нижняя двигается
                AddVertex(pos1x, pos1y, pos1z, u1, v1, colorsr[0], colorsg[0], colorsb[0], lights[0], 1);
                AddVertex(pos2x, pos2y, pos2z, u2, v2, colorsr[1], colorsg[1], colorsb[1], lights[1], 0);
                AddVertex(pos3x, pos3y, pos3z, u3, v3, colorsr[2], colorsg[2], colorsb[2], lights[2], 0);
                AddVertex(pos1x, pos1y, pos1z, u1, v1, colorsr[0], colorsg[0], colorsb[0], lights[0], 1);
                AddVertex(pos3x, pos3y, pos3z, u3, v3, colorsr[2], colorsg[2], colorsb[2], lights[2], 0);
                AddVertex(pos4x, pos4y, pos4z, u4, v4, colorsr[3], colorsg[3], colorsb[3], lights[3], 1);
            }
            else
            {
                // Двигается всё
                AddVertex(pos1x, pos1y, pos1z, u1, v1, colorsr[0], colorsg[0], colorsb[0], lights[0], 1);
                AddVertex(pos2x, pos2y, pos2z, u2, v2, colorsr[1], colorsg[1], colorsb[1], lights[1], 1);
                AddVertex(pos3x, pos3y, pos3z, u3, v3, colorsr[2], colorsg[2], colorsb[2], lights[2], 1);
                AddVertex(pos1x, pos1y, pos1z, u1, v1, colorsr[0], colorsg[0], colorsb[0], lights[0], 1);
                AddVertex(pos3x, pos3y, pos3z, u3, v3, colorsr[2], colorsg[2], colorsb[2], lights[2], 1);
                AddVertex(pos4x, pos4y, pos4z, u4, v4, colorsr[3], colorsg[3], colorsb[3], lights[3], 1);
            }
        }

        /// <summary>
        /// Построение буфера с новой текстурой на прошлых позициях для разрушения
        /// </summary>
        public void BuildingDamaged(int numberTexture)
        {
            pos1x = vertex[0].x + posCenterX;
            pos1y = vertex[0].y + posCenterY;
            pos1z = vertex[0].z + posCenterZ;
            pos2x = vertex[1].x + posCenterX;
            pos2y = vertex[1].y + posCenterY;
            pos2z = vertex[1].z + posCenterZ;
            pos3x = vertex[2].x + posCenterX;
            pos3y = vertex[2].y + posCenterY;
            pos3z = vertex[2].z + posCenterZ;
            pos4x = vertex[3].x + posCenterX;
            pos4y = vertex[3].y + posCenterY;
            pos4z = vertex[3].z + posCenterZ;

            u3 = u4 = (numberTexture % 64) * .015625f;
            u1 = u2 = u3 + .015625f * 2f;
            v2 = v3 = numberTexture / 64 * .015625f;
            v1 = v4 = v2 + .015625f * 2f;
            
            // снаружи
            AddVertex(pos1x, pos1y, pos1z, u1, v1, colorsr[0], colorsg[0], colorsb[0], lights[0]);
            AddVertex(pos2x, pos2y, pos2z, u2, v2, colorsr[1], colorsg[1], colorsb[1], lights[1]);
            AddVertex(pos3x, pos3y, pos3z, u3, v3, colorsr[2], colorsg[2], colorsb[2], lights[2]);
            AddVertex(pos1x, pos1y, pos1z, u1, v1, colorsr[0], colorsg[0], colorsb[0], lights[0]);
            AddVertex(pos3x, pos3y, pos3z, u3, v3, colorsr[2], colorsg[2], colorsb[2], lights[2]);
            AddVertex(pos4x, pos4y, pos4z, u4, v4, colorsr[3], colorsg[3], colorsb[3], lights[3]);
        }

        /// <summary>
        /// Добавить вершину
        /// </summary>
        private void AddVertex(float x, float y, float z, float u, float v, byte r, byte g, byte b, byte light, byte height = 0)
        {
            buffer.AddFloat(BitConverter.GetBytes(x));
            buffer.AddFloat(BitConverter.GetBytes(y));
            buffer.AddFloat(BitConverter.GetBytes(z));
            buffer.AddFloat(BitConverter.GetBytes(u));
            buffer.AddFloat(BitConverter.GetBytes(v));
            buffer.buffer[buffer.count++] = r;
            buffer.buffer[buffer.count++] = g;
            buffer.buffer[buffer.count++] = b;
            buffer.buffer[buffer.count++] = light;
            buffer.buffer[buffer.count++] = animationFrame;
            buffer.buffer[buffer.count++] = animationPause;
            buffer.buffer[buffer.count++] = height;
            buffer.count++;
        }

        /// <summary>
        /// Добавить вершину в кэш
        /// </summary>
        private void AddVertexCache(float x, float y, float z, float u, float v, byte r, byte g, byte b, byte light, byte height = 0)
        {
            bufferCache.AddRange(BitConverter.GetBytes(x));
            bufferCache.AddRange(BitConverter.GetBytes(y));
            bufferCache.AddRange(BitConverter.GetBytes(z));
            bufferCache.AddRange(BitConverter.GetBytes(u));
            bufferCache.AddRange(BitConverter.GetBytes(v));
            bufferCache.buffer[bufferCache.count++] = r;
            bufferCache.buffer[bufferCache.count++] = g;
            bufferCache.buffer[bufferCache.count++] = b;
            bufferCache.buffer[bufferCache.count++] = light;
            bufferCache.buffer[bufferCache.count++] = animationFrame;
            bufferCache.buffer[bufferCache.count++] = animationPause;
            bufferCache.buffer[bufferCache.count++] = height;
            bufferCache.count++;
        }

        /// <summary>
        /// Сгенерировать сетку VBO четырёхугольника снаружи
        /// </summary>
        public void BufferSideOutside(float pos1x, float pos1y, float pos1z,
            float pos2x, float pos2y, float pos2z,
            float pos3x, float pos3y, float pos3z,
            float pos4x, float pos4y, float pos4z,
            float u1, float v1, float u2, float v2,
            float u3, float v3, float u4, float v4
            )
        {
            AddVertex(pos1x, pos1y, pos1z, u1, v1, colorsr[0], colorsg[0], colorsb[0], lights[0]);
            AddVertex(pos2x, pos2y, pos2z, u2, v2, colorsr[1], colorsg[1], colorsb[1], lights[1]);
            AddVertex(pos3x, pos3y, pos3z, u3, v3, colorsr[2], colorsg[2], colorsb[2], lights[2]);
            AddVertex(pos1x, pos1y, pos1z, u1, v1, colorsr[0], colorsg[0], colorsb[0], lights[0]);
            AddVertex(pos3x, pos3y, pos3z, u3, v3, colorsr[2], colorsg[2], colorsb[2], lights[2]);
            AddVertex(pos4x, pos4y, pos4z, u4, v4, colorsr[3], colorsg[3], colorsb[3], lights[3]);
        }

        /// <summary>
        /// Сгенерировать сетку VBO четырёхугольника снаружи
        /// </summary>
        public void BufferSideOutsideCache(float pos1x, float pos1y, float pos1z,
            float pos2x, float pos2y, float pos2z,
            float pos3x, float pos3y, float pos3z,
            float pos4x, float pos4y, float pos4z,
            float u1, float v1, float u2, float v2,
            float u3, float v3, float u4, float v4,
            byte h1 = 0, byte h2 = 0, byte h3 = 0, byte h4 = 0
            )
        {
            AddVertexCache(pos1x, pos1y, pos1z, u1, v1, colorsr[0], colorsg[0], colorsb[0], lights[0], h1);
            AddVertexCache(pos2x, pos2y, pos2z, u2, v2, colorsr[1], colorsg[1], colorsb[1], lights[1], h2);
            AddVertexCache(pos3x, pos3y, pos3z, u3, v3, colorsr[2], colorsg[2], colorsb[2], lights[2], h3);
            AddVertexCache(pos1x, pos1y, pos1z, u1, v1, colorsr[0], colorsg[0], colorsb[0], lights[0], h1);
            AddVertexCache(pos3x, pos3y, pos3z, u3, v3, colorsr[2], colorsg[2], colorsb[2], lights[2], h3);
            AddVertexCache(pos4x, pos4y, pos4z, u4, v4, colorsr[3], colorsg[3], colorsb[3], lights[3], h4);
        }

        /// <summary>
        /// Сгенерировать сетку VBO четырёхугольника изнутри
        /// </summary>
        public void BufferSideInside(float pos1x, float pos1y, float pos1z,
            float pos2x, float pos2y, float pos2z,
            float pos3x, float pos3y, float pos3z,
            float pos4x, float pos4y, float pos4z,
            float u1, float v1, float u2, float v2,
            float u3, float v3, float u4, float v4,
            byte h1 = 0, byte h2 = 0, byte h3 = 0, byte h4 = 0
            )
        {
            AddVertex(pos3x, pos3y, pos3z, u3, v3, colorsr[2], colorsg[2], colorsb[2], lights[2], h3);
            AddVertex(pos2x, pos2y, pos2z, u2, v2, colorsr[1], colorsg[1], colorsb[1], lights[1], h2);
            AddVertex(pos1x, pos1y, pos1z, u1, v1, colorsr[0], colorsg[0], colorsb[0], lights[0], h1);
            AddVertex(pos4x, pos4y, pos4z, u4, v4, colorsr[3], colorsg[3], colorsb[3], lights[3], h4);
            AddVertex(pos3x, pos3y, pos3z, u3, v3, colorsr[2], colorsg[2], colorsb[2], lights[2], h3);
            AddVertex(pos1x, pos1y, pos1z, u1, v1, colorsr[0], colorsg[0], colorsb[0], lights[0], h1);
        }

        /// <summary>
        /// Сгенерировать сетку VBO четырёхугольника с двух сторон
        /// </summary>
        public void BufferSideTwo(float pos1x, float pos1y, float pos1z,
            float pos2x, float pos2y, float pos2z,
            float pos3x, float pos3y, float pos3z,
            float pos4x, float pos4y, float pos4z,
            float u1, float v1, float u2, float v2,
            float u3, float v3, float u4, float v4,
            bool insideNot = false,
            byte h1 = 0, byte h2 = 0, byte h3 = 0, byte h4 = 0
            )
        {
            if (!insideNot)
            {
                BufferSideInside(pos1x, pos1y, pos1z, pos2x, pos2y, pos2z, pos3x, pos3y, pos3z, pos4x, pos4y, pos4z,
                    u1, v1, u2, v2, u3, v3, u4, v4, h1, h2, h3, h4);
            }
            BufferSideOutsideCache(pos1x, pos1y, pos1z, pos2x, pos2y, pos2z, pos3x, pos3y, pos3z, pos4x, pos4y, pos4z,
                u1, v1, u2, v2, u3, v3, u4, v4, h1, h2, h3, h4);
        }

        /// <summary>
        /// Добавить кэш сетку в основной буфер
        /// </summary>
        public void AddBufferCache()
        {
            if (bufferCache.count > 0)
            {
                buffer.AddRange(bufferCache);
            }
        }
    }
}
