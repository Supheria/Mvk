﻿using MvkAssets;
using MvkClient.Entity;
using MvkClient.Renderer.Font;
using MvkClient.World;
using MvkServer.Entity;
using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Block;
using SharpGL;

namespace MvkClient.Renderer.Entity
{
    /// <summary>
    /// Абстракный класс рендера сущностей
    /// </summary>
    public abstract class RenderEntityBase
    {
        protected RenderManager renderManager;
        /// <summary>
        /// Текстура тени
        /// </summary>
        //private static final ResourceLocation shadowTextures = new ResourceLocation("textures/misc/shadow.png");
        /// <summary>
        /// Размер тени
        /// </summary>
        protected float shadowSize;
        /// <summary>
        /// Определяет темноту тени объекта. Чем выше значение, тем темнее тень.
        /// </summary>
        protected float shadowOpaque = 1.0f;

        protected RenderEntityBase(RenderManager renderManager) => this.renderManager = renderManager;

        public virtual void DoRender(EntityBase entity, vec3 offset, float timeIndex)
        {
            if (IsEntityLabel(entity))
            {
                RenderLivingLabel(entity, entity.GetName(), offset, timeIndex);
            }
            //if (IsShadowLabel(entity))
            //{
            //    RenderShadow(entity, offset, .5f, timeIndex);
            //}
        }

        public void DoRenderShadowAndFire(EntityBase entity, vec3 offset, float timeIndex)
        {
            if (IsShadowLabel(entity))
            {
                RenderShadow(entity, offset, .5f, timeIndex);
            }

            if (entity.InFire())
            {
                EntityPlayerSP playerSP = renderManager.ClientMain.Player;
                if (entity.Id != playerSP.Id
                    || (entity.Id == playerSP.Id && playerSP.ViewCamera != EnumViewCamera.Eye))
                {
                    RenderEntityOnFire(entity, offset, timeIndex);
                }
            }
        }

        /// <summary>
        /// Можно ли прописывать название над сущностью
        /// </summary>
        protected bool IsEntityLabel(EntityBase entity) => entity.HasCustomName();
        /// <summary>
        /// Есть ли тень у сущности
        /// </summary>
        protected bool IsShadowLabel(EntityBase entity) => true;

        /// <summary>
        /// Название сущности над головой
        /// </summary>
        protected void RenderLivingLabel(EntityBase entity, string text, vec3 offset, float timeIndex)
        {
            float dis = glm.distance(renderManager.CameraPosition, entity.Position);

            if (dis <= 64) // дистанция между сущностями
            {
                vec3 pos = entity.GetPositionFrame(timeIndex);
                vec3 offsetPos = pos - offset;
                float size = 3.2f;
                float scale = 0.0167f * size;
                FontSize font = FontSize.Font8;
                int ws = FontRenderer.WidthString(text, font) / 2;

                GLRender.PushMatrix();
                {
                    GLRender.DepthDisable();
                    GLRender.Translate(offsetPos.x, offsetPos.y + entity.Height + .5f, offsetPos.z);
                    GLRender.Rotate(glm.degrees(-renderManager.CameraRotationYaw), 0, 1, 0);
                    GLRender.Rotate(glm.degrees(renderManager.CameraRotationPitch), 1, 0, 0);
                    GLRender.Scale(renderManager.ClientMain.Player.ViewCamera == EnumViewCamera.Front ? -scale : scale, -scale, scale);
                    GLRender.Texture2DDisable();
                    GLRender.Rectangle(-ws - 1, -1, ws + 1, 8, new vec4(0, 0, 0, .25f));
                    GLRender.Texture2DEnable();
                    GLWindow.Texture.BindTexture(Assets.ConvertFontToTexture(font));
                    FontRenderer.RenderString(-ws, 0, new vec4(1), text, font);
                    GLRender.DepthEnable();
                }
                GLRender.PopMatrix();
            }
        }

        /// <summary>
        /// Отрисовывает тени объекта в позиции, тени альфа и partialTickTime
        /// </summary>
        protected void RenderShadow(EntityBase entity, vec3 offset, float shadowAlpha, float timeIndex)
        {
            float dis = glm.distance(renderManager.CameraPosition, entity.Position);

            if (dis < 32) // дистанция между сущностями
            {
                vec3 pos = entity.GetPositionFrame(timeIndex);
                vec3 offsetPos = pos - offset;
                WorldClient world = renderManager.World;
                float shadowSize = this.shadowSize * 2f;
                float shadowSizeY = shadowSize * 3f;
                if (shadowSizeY < 1.1f) shadowSizeY = 1.1f;
                if (dis > 16)
                {
                    float adis = (32 - dis) / 16f;
                    shadowAlpha *= adis;
                }

                vec3i pos0 = new vec3i(Mth.Floor(pos.x - shadowSize), Mth.Floor(pos.y - shadowSizeY), Mth.Floor(pos.z - shadowSize));
                vec3i pos1 = new vec3i(Mth.Floor(pos.x + shadowSize), Mth.Floor(pos.y - .1f), Mth.Floor(pos.z + shadowSize));

                BlockPos[] blocks = BlockPos.GetAllInBox(pos0, pos1);

                GLRender.DepthEnable();
                GLRender.DepthMask(false);
                GLRender.PolygonOffsetEnable();
                GLRender.CullDisable();
                GLRender.Texture2DEnable();
                GLWindow.Texture.BindTexture(AssetsTexture.Shadow);
                GLWindow.gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_CLAMP_TO_BORDER);
                GLWindow.gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_CLAMP_TO_BORDER);

                BlockBase block;
                BlockBase blockUp;
                for (int i = 0; i < blocks.Length; i++)
                {
                    block = world.GetBlockState(blocks[i]).GetBlock();
                    blockUp = world.GetBlockState(blocks[i].OffsetUp()).GetBlock();
                    if ((!blockUp.IsCollidable || !blockUp.Shadow) && block.Shadow)
                    {
                        float x1 = blocks[i].X - pos.x;
                        float x2 = blocks[i].X + 1f - pos.x;
                        float y = blocks[i].Y + 1f - pos.y;
                        float z1 = blocks[i].Z - pos.z;
                        float z2 = blocks[i].Z + 1f - pos.z;

                        float u1 = -x1 / 2.0f / shadowSize + 0.5f;
                        float u2 = -x2 / 2.0f / shadowSize + 0.5f;
                        float v1 = -z1 / 2.0f / shadowSize + 0.5f;
                        float v2 = -z2 / 2.0f / shadowSize + 0.5f;

                        float height = 1f - (pos.y - (float)blocks[i].Y - 1f) / (shadowSizeY + .5f);
                        if (height > 1) height = 1;
                        float alpha = shadowAlpha * height;

                        GLRender.PushMatrix();
                        GLRender.Translate(offsetPos.x, offsetPos.y, offsetPos.z);
                        GLRender.Scale(1, 1, 1);
                        GLRender.Color(1, 1, 1, alpha);
                        GLRender.Begin(OpenGL.GL_TRIANGLE_STRIP);
                        GLRender.VertexWithUV(x1, y, z1, u1, v1);
                        GLRender.VertexWithUV(x2, y, z1, u2, v1);
                        GLRender.VertexWithUV(x1, y, z2, u1, v2);
                        GLRender.VertexWithUV(x2, y, z2, u2, v2);
                        GLRender.End();
                        GLRender.PopMatrix();
                    }
                }
                GLRender.CullEnable();
                GLRender.PolygonOffsetDisable();
                GLRender.DepthMask(true);
            }
        }

        /// <summary>
        /// Отрисовывает огонь в позиции
        /// </summary>
        protected void RenderEntityOnFire(EntityBase entity, vec3 offset, float timeIndex)
        {
            GLRender.PushMatrix();

            vec3 pos = entity.GetPositionFrame(timeIndex);
            vec3 offsetPos = pos - offset;
            GLRender.CullDisable();
            GLRender.Translate(offsetPos);
            float size = entity.Width * 2.8f;
            GLRender.Scale(size);

            GLRender.Rotate(-glm.degrees(renderManager.CameraRotationYaw), 0, 1, 0);
            float h = entity.Height / size;
            GLRender.Translate(0, 0, -.3f + (float)((int)h) * .02f);
            GLRender.Color(1);

            float x = .5f;
            float y = (entity.Position.y - entity.BoundingBox.Min.y);
            float z = 0;

            uint time = entity.World.GetTotalWorldTime();
            int frame = (int)(time - (time / 32) * 32);

            float u1 = .84375f;
            float u2 = .859375f;
            float v1 = frame * .015625f;
            float v2 = v1 + .015625f;

            GLRender.Texture2DEnable();
            GLWindow.Texture.BindTexture(AssetsTexture.Atlas);

            while (h > 0)
            {
                GLRender.Begin(OpenGL.GL_TRIANGLE_STRIP);
                GLRender.VertexWithUV(-x, -y, z, u2, v2);
                GLRender.VertexWithUV(x, -y, z, u1, v2);
                GLRender.VertexWithUV(-x, 1.4f - y, z, u2, v1);
                GLRender.VertexWithUV(x, 1.4f - y, z, u1, v1);
                GLRender.End();

                h -= .45f;
                y -= .45f;
                x *= .9f;
                z += .03f;

                v1 += .0625f;
                while(v1 >= .5f) v1 -= .5f;
                v2 = v1 + .015625f;
            }
            GLRender.PopMatrix();
            GLRender.CullEnable();
        }
        
    }
}
