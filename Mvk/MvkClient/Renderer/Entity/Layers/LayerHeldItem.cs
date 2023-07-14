using MvkClient.Renderer.Model;
using MvkServer.Entity;
using MvkServer.Item;
using MvkServer.Item.List;

namespace MvkClient.Renderer.Entity.Layers
{
    public class LayerHeldItem : ILayerRenderer
    {
        /// <summary>
        /// Модель к которой привязывается предмет, правая рука как равило
        /// </summary>
        private readonly ModelRender modelRender;
        /// <summary>
        /// Рендер предмета
        /// </summary>
        private readonly RenderItems renderItem;
        /// <summary>
        /// Вид с глаз
        /// </summary>
        private readonly bool fromEyes = false;

        public LayerHeldItem(ModelRender modelRender, RenderItems renderItem, bool fromEyes)
        {
            this.modelRender = modelRender;
            this.renderItem = renderItem;
            this.fromEyes = fromEyes;
        }

        public void DoRenderLayer(EntityLiving entity, float limbSwing, float limbSwingAmount, float timeIndex, float ageInTicks, float headYaw, float headPitch, float scale)
        {
            ItemStack itemStack = entity.GetHeldItem();

            if (itemStack != null && itemStack.Item != null)
            {
                GLRender.PushMatrix();
                ItemBase item = itemStack.Item;
                if (!fromEyes && entity.IsSneaking()) GLRender.Translate(0, .2f, 0);

                // Дублирование поворотов и смещения модели
                modelRender.PostRender(.0625f);
                GLRender.Translate(-.0625f, .4375f, .0625f);

                if (item.EItem == EnumItem.Block && item is ItemBlock)
                {
                    // Блок
                    if (fromEyes)
                    {
                        GLRender.Translate(.1f, .25f, -.3125f);
                        GLRender.Rotate(30, 0, 0, 1);
                        GLRender.Rotate(90, 1, 0, 0);
                        GLRender.Rotate(75, 0, 1, 0);
                    }
                    else
                    {
                        GLRender.Translate(0, .1875f, -.3125f);
                        GLRender.Rotate(20, 1, 0, 0);
                        GLRender.Rotate(30, 1, 0, 0);
                        GLRender.Rotate(45, 0, 1, 0);
                    }
                    float size = .375f;
                    GLRender.Scale(-size, -size, size);
                }
                else
                {
                    // Предмет 2д
                    float size;
                    if (fromEyes)
                    {
                        // Режим с глаз
                        if (item.ItemUseAction == EnumItemAction.Axe)
                        {
                            GLRender.Translate(.08f, .26f, -.3125f);
                            GLRender.Rotate(110, 1, 0, 0);
                            GLRender.Rotate(-20, 0, 1, 0);
                        }
                        else if (item.ItemUseAction == EnumItemAction.Shovel)
                        {
                            GLRender.Scale(2);
                            GLRender.Translate(.026f, .1f, -.15f);
                            GLRender.Rotate(170, 1, 0, 0);
                            GLRender.Rotate(30, 0, 1, 0);
                        }
                        else
                        {
                            GLRender.Translate(.12f, .25f, -.3125f);
                            GLRender.Rotate(30, 0, 0, 1);
                            GLRender.Rotate(135, 1, 0, 0);
                            GLRender.Rotate(45, 0, 1, 0);
                        }
                        size = .5f;
                    }
                    else
                    {
                        // Со стороны
                        GLRender.Translate(0, .1f, -.3125f);
                        GLRender.Rotate(90, 1, 0, 0);
                        size = .5f;
                    }
                    GLRender.Scale(-size, -size, size);
                }

                // Прорисовка предмета
                renderItem.Render(item);
                GLRender.PopMatrix();
            }
        }
    }
}
