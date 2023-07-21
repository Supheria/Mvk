using MvkClient.Renderer.Model;
using MvkClient.World;
using MvkServer.Entity;
using MvkServer.Glm;
using MvkServer.Util;
using SharpGL;
using System;

namespace MvkClient.Renderer.Entity
{
    public class RenderManager
    {
        /// <summary>
        /// Основной клиент
        /// </summary>
        public Client ClientMain { get; private set; }
        /// <summary>
        /// Клиентский объект мира
        /// </summary>
        public WorldClient World { get; private set; }
        /// <summary>
        /// Скрыть ли хитбокс сущности
        /// </summary>
        public bool IsHiddenHitbox { get; set; } = true;
        /// <summary>
        /// Позиция основного игрока, камеры
        /// </summary>
        public vec3 CameraPosition { get; private set; }
        public float CameraRotationYaw { get; private set; }
        public float CameraRotationPitch { get; private set; }
        /// <summary>
        /// Смещение камеры для всего мира и всех элементов
        /// </summary>
        public vec3 CameraOffset { get; private set; }
        /// <summary>
        /// Рендер всех придметов
        /// </summary>
        public RenderItems Item { get; private set; }
        
        /// <summary>
        /// Массив рендер объектов сущьностей
        /// </summary>
        private readonly RenderEntityBase[] entities;

        public RenderManager(WorldClient world)
        {
            World = world;
            ClientMain = world.ClientMain;
            Item = new RenderItems();
            entities = new RenderEntityBase[EntitiesCount.COUNT + 1];
            entities[(int)EnumEntities.Player] = new RenderPlayer(this, new ModelPlayer(), false);
            entities[(int)EnumEntities.PlayerHand] = new RenderHead(this, new ModelPlayerHand());
            entities[(int)EnumEntities.Chicken] = new RenderChicken(this, new ModelChicken());
            entities[(int)EnumEntities.Item] = new RenderEntityItem(this, Item);
            entities[(int)EnumEntities.Piece] = new RenderEntityItem(this, Item);
            entities[(int)EnumEntities.Chemoglot] = new RenderChemoglot(this, new ModelChemoglot());
            entities[(int)EnumEntities.Pakan] = new RenderPakan(this, new ModelPakan());
            entities[(int)EnumEntities.Book] = new RenderBook(this, new ModelBook());
            entities[(int)EnumEntities.PlayerInvisible] = new RenderPlayer(this, new ModelPlayer(), true);
        }

        /// <summary>
        /// Задать камеру игрока
        /// </summary>
        public void SetCamera(vec3 pos, float yaw, float pitch)
        {
            CameraPosition = pos;
            CameraRotationYaw = yaw;
            CameraRotationPitch = pitch;
            CameraOffset = new vec3(pos.x, pos.y, pos.z);
        }

        /// <summary>
        /// Получить объект рендера сущности по объекту сущности
        /// </summary>
        public RenderEntityBase GetEntityRenderObject(EntityBase entity)
        {
            try
            {
                return entities[(int)entity.GetEntityType()] as RenderEntityBase;
            }
            catch (Exception ex)
            {
                World.Log.Error(ex);
                return null;
            }
        }

        /// <summary>
        /// Сгенерировать сущность на экране
        /// </summary>
        public void RenderEntity(EntityBase entity, float timeIndex)
        {
            if (!entity.IsDead)
            {
                World.CountEntitiesShowAdd();
                RenderEntityBase render = GetEntityRenderObject(entity);
                 
                if (render != null)
                {
                    GLRender.Texture2DEnable();
                    GLRender.TextureLightmapDisable();
                    bool visible = !entity.IsInvisible();
                    if (visible)
                    {
                        render.DoRenderShadowAndFire(entity, CameraOffset, timeIndex);
                    }
                    GLRender.TextureLightmapEnable();
                    GLRender.LightmapTextureCoords(entity.GetBrightnessForRender());
                    render.DoRender(entity, CameraOffset, timeIndex);
                    if (!IsHiddenHitbox && visible)
                    {
                        RenderEntityBoundingBox(entity, CameraOffset, timeIndex);
                    }
                }
            }
        }

        /// <summary>
        /// Отрисовать рамку хитбокса сущности, для отладки
        /// </summary>
        protected void RenderEntityBoundingBox(EntityBase entity, vec3 offset, float timeIndex)
        {
            vec3 pos0 = entity.GetPositionFrame(timeIndex);
            vec3 look = new vec3(0);
            AxisAlignedBB aabb = entity.GetBoundingBox(new vec3(0));
            float eye = aabb.Min.y;
            float width = entity.Width;

            bool isLook = false;
            if (entity is EntityLiving entityLiving)
            {
                look += entityLiving.GetLookFrame(timeIndex);
                eye += entity.GetEyeHeight();
                isLook = true;
            }

            GLRender.PushMatrix();
            {
                GLRender.Translate(pos0 - offset);
                GLRender.Texture2DDisable();
                GLRender.CullDisable();
                GLRender.LineWidth(2f);

                // Рамка хитбокса
                GLRender.Color(new vec4(1));
                GLRender.DrawOutlinedBoundingBox(aabb);

                if (isLook)
                {
                    // Уровень глаз
                    GLRender.Color(new vec4(1, 0, 0, 1));
                    GLRender.Begin(OpenGL.GL_LINE_STRIP);
                    GLRender.Vertex(aabb.Min.x, eye, aabb.Min.z);
                    GLRender.Vertex(aabb.Max.x, eye, aabb.Min.z);
                    GLRender.Vertex(aabb.Max.x, eye, aabb.Max.z);
                    GLRender.Vertex(aabb.Min.x, eye, aabb.Max.z);
                    GLRender.Vertex(aabb.Min.x, eye, aabb.Min.z);
                    GLRender.End();

                    // Луч глаз куда смотрит
                    GLRender.Color(new vec4(0, 0, 1, 1));
                    GLRender.Begin(OpenGL.GL_LINES);
                    vec3 pos = new vec3(aabb.Min.x + width, eye, aabb.Min.z + width);
                    GLRender.Vertex(pos);
                    GLRender.Vertex(pos + look * 2f);
                    GLRender.End();
                }

                GLRender.CullEnable();
            }
            GLRender.PopMatrix();
        }
    }
}
