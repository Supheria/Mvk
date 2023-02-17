using MvkServer.Entity;
using MvkServer.Glm;
using MvkServer.Util;

namespace MvkClient.Renderer.Model
{
    /// <summary>
    /// Модель книги
    /// </summary>
    public class ModelBook : ModelBase
    {
        /// <summary>
        /// Переплёт
        /// </summary>
        private ModelRender boxBinding;
        /// <summary>
        /// Закладка
        /// </summary>
        private ModelRender boxBookmark;
        /// <summary>
        /// Левая часть
        /// </summary>
        private ModelRender boxLeft;
        /// <summary>
        /// Правая часть
        /// </summary>
        private ModelRender boxRight;

        public ModelBook()
        {
            float y = 20;
            TextureSize = new vec2(32f, 32f);
            boxBinding = new ModelRender(this, 0, 0) { RotationPointY = y };
            boxBinding.SetBox(-2, -4, 0, 4, 8, 2, 0);
            boxBookmark = new ModelRender(this, 12, 0) { RotationPointY = y };
            boxBookmark.SetBox(0, -2, -5, 0, 4, 5, 0);
            boxLeft = new ModelRender(this, 0, 10);
            boxLeft.Mirror();
            boxLeft.SetBox(-1, -4, -6, 2, 8, 6, 0);
            boxLeft.SetRotationPoint(1f, y, 0);
            boxRight = new ModelRender(this, 0, 10);
            boxRight.SetBox(-1, -4, -6, 2, 8, 6, 0);
            boxRight.SetRotationPoint(-1f, y, 0);
        }

        public override void Render(EntityLiving entity, float limbSwing, float limbSwingAmount, float ageInTicks, float headYaw, float headPitch, float scale)
        {
            SetRotationAngles(entity, limbSwing, limbSwingAmount, ageInTicks, headYaw, headPitch, scale);
            GLRender.PushMatrix();

            boxBinding.Render(scale);
            boxBookmark.Render(scale);
            boxLeft.Render(scale);
            boxRight.Render(scale);

            GLRender.PopMatrix();
        }

        protected override void SetRotationAngles(EntityLiving entity, float limbSwing,
            float limbSwingAmount, float ageInTicks, float headYaw, float headPitch, float scale)
        {
            float swing = Mth.Abs(glm.cos(limbSwing * .6662f + ageInTicks * .096662f));// * 4f * (limbSwingAmount + l1));
            if (swing > 1) swing = 1;
            swing = 1 - swing;
            boxLeft.RotateAngleY = -swing;
            boxRight.RotateAngleY = swing;
        }
    }
}
