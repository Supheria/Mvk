using MvkAssets;
using MvkClient.Util;
using MvkServer.Glm;
using MvkServer.Item;
using MvkServer.Util;
using MvkServer.World.Block;
using MvkServer.World.Item;

namespace MvkClient.Renderer.Entity
{
    /// <summary>
    /// Рендер предмета
    /// </summary>
    public class RenderItem : RenderDL
    {
        /// <summary>
        /// Тип блока
        /// </summary>
        private readonly EnumItem enumItem;

        public RenderItem(EnumItem enumItem) => this.enumItem = enumItem;

        protected override void DoRender()
        {
            ItemBase item = Items.GetItemCache(enumItem);
            if (item != null)
            {
                if (item.IsRenderQuadSide)
                {
                    RenderQuadSide(item);
                }
                else
                {
                    RenderFlat(item);
                }
            }
        }

        /// <summary>
        /// Рендер предмета из прямоугольников
        /// </summary>
        private void RenderQuadSide(ItemBase item)
        {
            ItemQuadSide quad;
            ItemQuadSide[] quads = item.Quads;
            TextureStruct ts = GLWindow.Texture.GetData(AssetsTexture.AtlasItems);
            GLWindow.Texture.BindTexture(ts.GetKey());
            GLRender.Texture2DEnable();
            GLRender.BeginTriangles();
            for (int i = 0; i < quads.Length; i++)
            {
                quad = quads[i];
                RenderVertex(quad.vertex[0], quad.lightPole);
                RenderVertex(quad.vertex[1], quad.lightPole);
                RenderVertex(quad.vertex[2], quad.lightPole);
                RenderVertex(quad.vertex[0], quad.lightPole);
                RenderVertex(quad.vertex[2], quad.lightPole);
                RenderVertex(quad.vertex[3], quad.lightPole);
            }
            GLRender.End();
        }

        private void RenderVertex(Vertex vertex, float lightPole)
        {
            GLRender.Color(1f - lightPole);
            GLRender.VertexWithUV(vertex.x, vertex.y, vertex.z, vertex.u, vertex.v);
        }

        /// <summary>
        /// Рендер плоского предмета
        /// </summary>
        private void RenderFlat(ItemBase item)
        {
            int ntu = item.NumberTexture % 32;
            int ntv = item.NumberTexture / 32;
            float x1 = -.5f;
            float y1 = -.5f;
            float x2 = .5f;
            float y2 = .5f;
            float z1 = -.03125f;
            float z2 = .03125f;
            float u1 = ntu * .03125f;
            float v2 = ntv * .03125f;
            float u2 = u1 + .03125f;
            float v1 = v2 + .03125f;

            // Буфер атласа предметов
            BufferedImage image = GLWindow.Texture.BufferAtlasItem;
            int wp = image.Width / 32;
            int hp = image.Height / 32;
            ntu *= 32;
            ntv *= 32;
            int ntu2 = ntu + wp - 1;
            int ntv2 = ntv + hp - 1;
            int x0, y0;
            float x3, y3;
            vec4 rgba;

            GLRender.Texture2DDisable();
            for (int x = ntu; x <= ntu2; x++)
            {
                x0 = x - ntu;
                for (int y = ntv; y <= ntv2; y++)
                {
                    y0 = ntv2 - y;
                    rgba = image.GetPixel(x, y);
                    if (rgba.w == 0)
                    {
                        // Проверка соседей
                        if (y < ntv2)
                        {
                            // Top
                            rgba = image.GetPixel(x, y + 1);
                            if (rgba.w == 1)
                            {
                                x3 = (x0 / (float)wp) - .5f;
                                y3 = ((y0 - 1) / (float)hp) - .5f;
                                Color(rgba, .9f);
                                GLRender.SideTop(x3, y3, z1, x3 + .03125f, y3 + .03125f, z2);
                            }
                        }
                        if (y > ntv)
                        {
                            // Bottom
                            rgba = image.GetPixel(x, y - 1);
                            if (rgba.w == 1)
                            {
                                x3 = (x0 / (float)wp) - .5f;
                                y3 = ((y0 + 1) / (float)hp) - .5f;
                                Color(rgba, .6f);
                                GLRender.SideBottom(x3, y3, z1, x3 + .03125f, y3 + .03125f, z2);
                            }
                        }
                        if (x > ntu)
                        {
                            // Left
                            rgba = image.GetPixel(x - 1, y);
                            if (rgba.w == 1)
                            {
                                x3 = ((x0 - 1) / (float)wp) - .5f;
                                y3 = (y0 / (float)hp) - .5f;
                                Color(rgba, .8f);
                                GLRender.SideLeft(x3, y3, z1, x3 + .03125f, y3 + .03125f, z2);
                            }
                        }
                        if (x < ntu2)
                        {
                            // Right
                            rgba = image.GetPixel(x + 1, y);
                            if (rgba.w == 1)
                            {
                                x3 = ((x0 + 1) / (float)wp) - .5f;
                                y3 = (y0 / (float)hp) - .5f;
                                Color(rgba, .8f);
                                GLRender.SideRight(x3, y3, z1, x3 + .03125f, y3 + .03125f, z2);
                            }
                        }
                    }
                    else
                    {
                        if (y == ntv)
                        {
                            // Top
                            x3 = (x0 / (float)wp) - .5f;
                            y3 = (y0 / (float)hp) - .5f;
                            Color(rgba, .9f);
                            GLRender.SideTop(x3, y3, z1, x3 + .03125f, y3 + .03125f, z2);
                        }
                        if (y == ntv2)
                        {
                            // Bottom
                            x3 = (x0 / (float)wp) - .5f;
                            y3 = (y0 / (float)hp) - .5f;
                            Color(rgba, .6f);
                            GLRender.SideBottom(x3, y3, z1, x3 + .03125f, y3 + .03125f, z2);
                        }
                        if (x == ntu2)
                        {
                            // Left
                            x3 = (x0 / (float)wp) - .5f;
                            y3 = (y0 / (float)hp) - .5f;
                            Color(rgba, .8f);
                            GLRender.SideLeft(x3, y3, z1, x3 + .03125f, y3 + .03125f, z2);
                        }
                        if (x == ntu)
                        {
                            // Right
                            Color(rgba, 1.2f);
                            x3 = (x0 / (float)wp) - .5f;
                            y3 = (y0 / (float)hp) - .5f;
                            Color(rgba, .8f);
                            GLRender.SideRight(x3, y3, z1, x3 + .03125f, y3 + .03125f, z2);
                        }
                    }
                }
            }

            TextureStruct ts = GLWindow.Texture.GetData(AssetsTexture.AtlasItems);
            GLWindow.Texture.BindTexture(ts.GetKey());
            GLRender.Texture2DEnable();
            GLRender.Color(1);
            GLRender.SideFront(x1, y1, z1, x2, y2, z2, u1, v1, u2, v2);
            GLRender.SideBack(x1, y1, z1, x2, y2, z2, u1, v1, u2, v2);
        }

        /// <summary>
        /// Цвет с изменением яркости
        /// </summary>
        private void Color(vec4 rgba, float brightness)
        {
            float r = rgba.x * brightness;
            float g = rgba.y * brightness;
            float b = rgba.z * brightness;
            if (r > 1) r = 1;
            if (g > 1) g = 1;
            if (b > 1) b = 1;
            if (r < 0) r = 0;
            if (g < 0) g = 0;
            if (b < 0) b = 0;
            GLRender.Color(r, g, b);
        }
    }
}
