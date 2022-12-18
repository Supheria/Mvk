using MvkServer.Entity;
using MvkServer.Entity.List;
using MvkServer.Glm;
using MvkServer.Item;
using MvkServer.Util;

namespace MvkClient.Renderer.Entity
{
    /// <summary>
    /// Рендер сущности предмета
    /// </summary>
    public class RenderEntityItem : RenderEntityBase
    {
        private readonly RenderItems item;
        private Rand rand;

        public RenderEntityItem(RenderManager renderManager, RenderItems item) : base(renderManager)
        {
            this.item = item;
            shadowSize = .15f;
            shadowOpaque = .75f;
        }

        public override void DoRender(EntityBase entity, vec3 offset, float timeIndex)
        {
            ItemStack stack = null;
            int count = 1;
            bool render = false;
            if (entity is EntityThrowable entityThrowable)
            {
                render = true;
                //stack = new ItemStack(Items.GetItemCache(entityThrowable.GetEnumItem()));
                stack = entityThrowable.GetEntityItemStack();
            }
            if (entity is EntityItem entityItem)
            {
                render = true;
                stack = entityItem.GetEntityItemStack();
                count = CountItem(stack);
            }
            if (render)
            {

                vec3 pos = entity.GetPositionFrame(timeIndex);
                vec3 offsetPos = pos - offset;
                rand = new Rand(entity.Id);
                int begin = rand.Next(360);
                rand = new Rand(187);

                GLRender.LightmapTextureCoords(entity.GetBrightnessForRender());
                GLRender.TextureLightmapEnable();
                GLRender.Texture2DEnable();

                GLRender.PushMatrix();
                {
                    GLRender.Translate(offsetPos.x, offsetPos.y, offsetPos.z);
                    float ageInTicks = renderManager.World.ClientMain.TickCounter + timeIndex + begin;
                    float yaw = ageInTicks * 2f;
                    float height = glm.cos(glm.radians(yaw)) * .16f + .5f;
                    GLRender.Rotate(yaw, 0, 1, 0);
                    for (int i = 0; i < count; i++)
                    {
                        GLRender.PushMatrix();
                        {
                            float x = (rand.NextFloat() * 2f - 1f) * .15f;
                            float y = (rand.NextFloat() * 2f - 1f) * .15f;
                            float z = (rand.NextFloat() * 2f - 1f) * .15f;

                            GLRender.Translate(x, y + height, z);
                            if (stack.Item.EItem == EnumItem.Block) GLRender.Scale(.5f);
                            item.Render(stack);
                        }
                        GLRender.PopMatrix();
                    }
                }
                GLRender.PopMatrix();
                base.DoRender(entity, offset, timeIndex);
            }
        }

        /// <summary>
        /// Количество предметов в зависимости от количества 
        /// </summary>
        private int CountItem(ItemStack itemStack)
        {
            int amount = itemStack.Amount;
            if (amount > 47) return 5;
            if (amount > 31) return 4;
            if (amount > 15) return 3;
            if (amount > 1) return 2;
            return 1;
        }


    }
}
