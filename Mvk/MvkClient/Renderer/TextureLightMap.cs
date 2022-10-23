using MvkServer.Util;
using SharpGL;
using System.Drawing;

namespace MvkClient.Renderer
{
    /// <summary>
    /// Объект работы текстуры для освещения
    /// </summary>
    public class TextureLightMap
    {
        private uint locationLightMap = 0;
        /// <summary>
        /// Буфер карты 16 (x) * 16 (y) * 4 (RGBA)
        /// </summary>
        private readonly byte[] buffer = new byte[1024];
        private float skyLightPrev = -1f;

        public void Update(float sunLight, float moonLight)
        {
            if (skyLightPrev != sunLight)
            {
                //Bitmap bitmap = new Bitmap(16, 16);
                skyLightPrev = sunLight;

                for (int i = 0; i < 256; ++i)
                {
                    float ls = sunLight < moonLight && moonLight > 0
                        ? MvkStatic.LightBrightnessTable[i / 16] * moonLight + .05f
                        : MvkStatic.LightBrightnessTable[i / 16] * sunLight * .95f + .05f;

                    //float lb = MvkStatic.LightBrightnessTable[i % 16] * 1.5f;
                    float lb = (i % 16) / 14f;

                    //if (молния)
                    //{
                    //    ls = MvkStatic.LightBrightnessTable[i / 16];
                    //}

                    float ls2 = ls * (sunLight * .65f + .35f);
                    float ls3 = ls * (sunLight * .65f + .35f);
                    float lb2 = lb * ((lb * .6f + .4f) * .6f + .4f);
                    float lb3 = lb * (lb * lb * .6f + .4f);
                    float cr = ls2 + lb;
                    float cg = ls3 + lb2;
                    float cb = ls + lb3;
                    cr = cr * .96f + .03f;
                    cg = cg * .96f + .03f;
                    cb = cb * .96f + .03f;
                    float light;

                    //if (this.bossColorModifier > 0.0F)
                    //{ // босс
                    //light = 1f;// this.bossColorModifierPrev + (this.bossColorModifier - this.bossColorModifierPrev) * p_78472_1_;
                    //cr = cr * (1.0F - light) + cr * 0.7F * light;
                    //cg = cg * (1.0F - light) + cg * 0.6F * light;
                    //cb = cb * (1.0F - light) + cb * 0.6F * light;
                    //}

                    //if (var2.provider.getDimensionId() == 1)
                    //{ // другой мир, всегда темно (ад)
                    //    cr = 0.22F + lb * 0.75F;
                    //    cg = 0.28F + lb2 * 0.75F;
                    //    cb = 0.25F + lb3 * 0.75F;
                    //}

                    float light2;

                    //if (this.mc.thePlayer.isPotionActive(Potion.nightVision))
                    //{ // зелье ночного виденья
                    //light = 1f;// this.func_180438_a(this.mc.thePlayer, p_78472_1_);
                    //light2 = 1.0F / cr;

                    //if (light2 > 1f / cg) light2 = 1f / cg;
                    //if (light2 > 1f / cb) light2 = 1f / cb;

                    //cr = cr * (1f - light) + cr * light2 * light;
                    //cg = cg * (1f - light) + cg * light2 * light;
                    //cb = cb * (1f - light) + cb * light2 * light;
                    //}

                    if (cr > 1f) cr = 1f;
                    if (cg > 1f) cg = 1f;
                    if (cb > 1f) cb = 1f;

                    light = .1f;// this.mc.gameSettings.gammaSetting; // яркость экрана  .1 - .5
                    float cr2 = 1f - cr;
                    float cg2 = 1f - cg;
                    float cb2 = 1f - cb;
                    cr2 = 1f - cr2 * cr2 * cr2 * cr2;
                    cg2 = 1f - cg2 * cg2 * cg2 * cg2;
                    cb2 = 1f - cb2 * cb2 * cb2 * cb2;
                    cr = cr * (1f - light) + cr2 * light;
                    cg = cg * (1f - light) + cg2 * light;
                    cb = cb * (1f - light) + cb2 * light;
                    cr = cr * .96f + .03f;
                    cg = cg * .96f + .03f;
                    cb = cb * .96f + .03f;

                    if (cr > 1f) cr = 1f;
                    if (cg > 1f) cg = 1f;
                    if (cb > 1f) cb = 1f;

                    if (cr < 0f) cr = 0f;
                    if (cg < 0f) cg = 0f;
                    if (cb < 0f) cb = 0f;

                    buffer[i * 4] = (byte)(cb * 255);
                    buffer[i * 4 + 1] = (byte)(cg * 255);
                    buffer[i * 4 + 2] = (byte)(cr * 255);
                    buffer[i * 4 + 3] = 255;

                    //bitmap.SetPixel(i % 16, i / 16, Color.FromArgb(255, (byte)(cr * 255), (byte)(cg * 255), (byte)(cb * 255)));
                }

                //bitmap.Save("LightMap.png", System.Drawing.Imaging.ImageFormat.Png);
                UpdateLightmap();
            }
        }

        private void UpdateLightmap()
        {
            bool isCreate = locationLightMap == 0;
            if (isCreate)
            {
                uint[] texture = new uint[1];
                GLWindow.gl.GenTextures(1, texture);
                locationLightMap = texture[0];
            }
            GLWindow.gl.ActiveTexture(OpenGL.GL_TEXTURE1);
            GLWindow.gl.BindTexture(OpenGL.GL_TEXTURE_2D, locationLightMap);
            GLWindow.gl.TexImage2D(OpenGL.GL_TEXTURE_2D, 0, OpenGL.GL_RGBA, 16, 16,
                0, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE, buffer);
            if (isCreate)
            {
                GLWindow.gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR);
                GLWindow.gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR);
                GLWindow.gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_CLAMP);
                GLWindow.gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_CLAMP);
            }
            GLWindow.gl.ActiveTexture(OpenGL.GL_TEXTURE0);
        }
    }
}
