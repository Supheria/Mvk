using MvkServer.Entity;
using MvkServer.Glm;
using MvkServer.Util;

namespace MvkClient.Renderer.Model
{
    /// <summary>
    /// Модель чемоглота
    /// </summary>
    public class ModelChemoglot : ModelBase
    {
        /// <summary>
        /// Верхняя часть
        /// </summary>
        private ModelRender boxUp;
        /// <summary>
        /// Нижняя часть
        /// </summary>
        private ModelRender boxDown;
        /// <summary>
        /// Ручка
        /// </summary>
        private ModelRender boxPen;

        public ModelChemoglot()
        {
            float y = 20;
            TextureSize = new vec2(64f, 64f);
            boxUp = new ModelRender(this, 0, 0) { RotationPointY = y };
            boxUp.SetBox(-8, -8, -8, 16, 8, 16, 0);
            boxDown = new ModelRender(this, 0, 24);
            boxDown.SetBox(-8, 0, -16, 16, 4, 16, 0);
            boxDown.SetRotationPoint(0, y, 8);
            boxPen = new ModelRender(this, 0, 0) { RotationPointY = y };
            boxPen.SetBox(-2, -4, -10, 4, 2, 2, 0);
        }

        public override void Render(EntityLiving entity, float limbSwing, float limbSwingAmount, float ageInTicks, float headYaw, float headPitch, float scale)
        {
            SetRotationAngles(entity, limbSwing, limbSwingAmount, ageInTicks, headYaw, headPitch, scale);
            GLRender.PushMatrix();

            boxUp.Render(scale);
            boxPen.Render(scale);
            boxDown.Render(scale);

            GLRender.PopMatrix();
        }

        protected override void SetRotationAngles(EntityLiving entity, float limbSwing,
            float limbSwingAmount, float ageInTicks, float headYaw, float headPitch, float scale)
        {
            boxDown.RotateAngleX = Mth.Abs(glm.cos(limbSwing * 0.6662f) * 1.4f * limbSwingAmount);
        }
    }
}
