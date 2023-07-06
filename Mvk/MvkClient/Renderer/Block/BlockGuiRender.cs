using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Block;
using SharpGL;
using System;

namespace MvkClient.Renderer.Block
{
    /// <summary>
    /// Объект рендера блока для GUI
    /// </summary>
    public class BlockGuiRender
    {
        /// <summary>
        /// Объект блока кэш
        /// </summary>
        private BlockBase block;
        /// <summary>
        /// Буфер всех блоков чанка
        /// </summary>
        private ArrayMvk<byte> buffer;

        private QuadSide[] quadSides;
        private QuadSide quadSide;

        /// <summary>
        /// Создание блока генерации для GUI
        /// </summary>
        public BlockGuiRender(BlockBase block) => this.block = block;

        /// <summary>
        /// Получить цвет в зависимости от биома, цвет определяем потипу
        /// </summary>
        private vec3 GetBiomeColor() => quadSide.IsBiomeColorGrass() ? BlockColorBiome.GrassDefault : block.GetColorGuiOrPartFX();

        /// <summary>
        /// Рендер блока VBO, конвертация из  VBO в DisplayList
        /// </summary>
        public void RenderVBOtoDL()
        {
            buffer = new ArrayMvk<byte>(6048);
            quadSides = block.GetQuadsGui();
            int count = quadSides.Length;
            for (int i = 0; i < count; i++)
            {
                quadSide = quadSides[i];
                // Смекшировать яркость, в зависимости от требований самой яркой
                vec3 color = GetBiomeColor();
                color.x -= quadSide.lightPole; if (color.x < 0) color.x = 0;
                color.y -= quadSide.lightPole; if (color.y < 0) color.y = 0;
                color.z -= quadSide.lightPole; if (color.z < 0) color.z = 0;
                byte cr = (byte)(color.x * 255);
                byte cg = (byte)(color.y * 255);
                byte cb = (byte)(color.z * 255);

                BlockSide blockUV = new BlockSide()
                {
                    colorsr = new byte[] { cr, cr, cr, cr },
                    colorsg = new byte[] { cg, cg, cg, cg },
                    colorsb = new byte[] { cb, cb, cb, cb },
                    lights = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF },
                    animationFrame = quadSide.animationFrame,
                    animationPause = quadSide.animationPause,
                };
                blockUV.vertex = quadSide.vertex;
                blockUV.buffer = buffer;
                blockUV.Building();
            }
            byte[] buffer2 = buffer.ToArray();

            GLRender.PushMatrix();
            {
                GLRender.Begin(OpenGL.GL_TRIANGLES);
                for (int i = 0; i < buffer2.Length; i += 28)
                {
                    float r = buffer2[i + 20] / 255f;
                    float g = buffer2[i + 21] / 255f;
                    float b = buffer2[i + 22] / 255f;
                    GLRender.Color(r, g, b);
                    float u = BitConverter.ToSingle(buffer2, i + 12);
                    float v = BitConverter.ToSingle(buffer2, i + 16);
                    GLRender.TexCoord(u, v);
                    float x = BitConverter.ToSingle(buffer2, i);
                    float y = BitConverter.ToSingle(buffer2, i + 4);
                    float z = BitConverter.ToSingle(buffer2, i + 8);
                    GLRender.Vertex(x - .5f, y - .5f, z - .5f);
                }
                GLRender.End();
            }
            GLRender.PopMatrix();
        }
    }
}
