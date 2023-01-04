using MvkServer.Entity;
using MvkServer.Glm;

namespace MvkClient.Renderer.Model
{
    /// <summary>
    /// Модель пакана
    /// </summary>
    public class ModelPakan : ModelBase
    {
        /// <summary>
        /// Голова
        /// </summary>
        private ModelRender boxHead;
        /// <summary>
        /// Шея
        /// </summary>
        private ModelRender boxNeck;
        /// <summary>
        /// Тело
        /// </summary>
        private ModelRender boxBody;
        // Ноги
        private ModelRender boxLegRight;
        private ModelRender boxLegLeft;
        private ModelRender boxLegCenter;
        // Руки
        private ModelRender boxArmRight;
        private ModelRender boxArmLeft;
        // Пальцы
        private ModelRender boxFingersRight;
        private ModelRender boxFingersLeft;

        public ModelPakan()
        {
            float y = -14;
            TextureSize = new vec2(64f, 64f);
            boxHead = new ModelRender(this, 0, 0) { RotationPointY = y };
            boxHead.SetBox(-4, -8, -4, 8, 9, 8, 0);
            boxNeck = new ModelRender(this, 32, 0) { RotationPointY = y };
            boxNeck.SetBox(-2, 0, -2, 4, 6, 4, 0);
            boxBody = new ModelRender(this, 0, 17) { RotationPointY = y };
            boxBody.SetBox(-6, 6, -4, 12, 18, 8, 0);

            boxLegRight = new ModelRender(this, 48, 0) { RotationPointY = y };
            boxLegRight.SetBox(-1.5f, 0, -1.5f, 3, 14, 3, 0);
            boxLegRight.SetRotationPoint(-4.5f, 24 + y, -2f);
            boxLegLeft = new ModelRender(this, 48, 0) { RotationPointY = y };
            boxLegLeft.Mirror();
            boxLegLeft.SetBox(-1.5f, 0, -1.5f, 3, 14, 3, 0);
            boxLegLeft.SetRotationPoint(4.5f, 24 + y, -2f);
            boxLegCenter = new ModelRender(this, 40, 17) { RotationPointY = y };
            boxLegCenter.SetBox(-2, 0, -2, 4, 14, 4, 0);
            boxLegCenter.SetRotationPoint(0, 24 + y, 2f);

            boxArmRight = new ModelRender(this, 40, 35) { RotationPointY = y };
            boxArmRight.SetBox(-7, -2, -2, 8, 4, 4, 0);
            boxArmRight.SetRotationPoint(-7f, 13 + y, 0);
            boxArmLeft = new ModelRender(this, 40, 35) { RotationPointY = y };
            boxArmLeft.Mirror();
            boxArmLeft.SetBox(-1, -2, -2, 8, 4, 4, 0);
            boxArmLeft.SetRotationPoint(7f, 13 + y, 0);

            boxFingersRight = new ModelRender(this, 0, 43) { RotationPointY = y };
            boxFingersRight.SetBox(-7, 2, 0, 8, 8, 1, 0);
            boxFingersRight.SetRotationPoint(-7f, 13 + y, 0);
            boxFingersLeft = new ModelRender(this, 0, 43) { RotationPointY = y };
            boxFingersLeft.Mirror();
            boxFingersLeft.SetBox(-1, 2, 0, 8, 8, 1, 0);
            boxFingersLeft.SetRotationPoint(7f, 13 + y, 0);
        }

        public override void Render(EntityLiving entity, float limbSwing, float limbSwingAmount, float ageInTicks, float headYaw, float headPitch, float scale)
        {
            SetRotationAngles(entity, limbSwing, limbSwingAmount, ageInTicks, headYaw, headPitch, scale);
            GLRender.PushMatrix();

            boxHead.Render(scale);
            boxNeck.Render(scale);
            boxBody.Render(scale);
            boxLegRight.Render(scale);
            boxLegLeft.Render(scale);
            boxLegCenter.Render(scale);
            boxArmRight.Render(scale);
            boxArmLeft.Render(scale);
            boxFingersRight.Render(scale);
            boxFingersLeft.Render(scale);

            GLRender.PopMatrix();
        }

        protected override void SetRotationAngles(EntityLiving entity, float limbSwing,
            float limbSwingAmount, float ageInTicks, float headYaw, float headPitch, float scale)
        {
            boxNeck.RotateAngleY = boxHead.RotateAngleY = headYaw;
            boxHead.RotateAngleX = -headPitch;

            boxLegCenter.RotateAngleX = glm.cos(limbSwing * 0.6662f) * 1.4f * limbSwingAmount;
            boxLegLeft.RotateAngleX = boxLegRight.RotateAngleX = glm.cos(limbSwing * 0.6662f + glm.pi) * 1.4f * limbSwingAmount;

            boxArmRight.RotateAngleX = boxLegCenter.RotateAngleX * 0.5f;
            boxArmLeft.RotateAngleX = boxLegRight.RotateAngleX * 0.5f;
            boxFingersRight.RotateAngleY = boxFingersRight.RotateAngleZ
                = boxFingersLeft.RotateAngleY = boxFingersLeft.RotateAngleZ
                = boxArmRight.RotateAngleY = boxArmRight.RotateAngleZ
                = boxArmLeft.RotateAngleY = boxArmLeft.RotateAngleZ = 0.0f;

            if (SwingProgress > 0)
            {
                boxArmRight.RotateAngleX = boxArmRight.RotateAngleX - (1f + glm.sin(SwingProgress * glm.pi));
                boxArmLeft.RotateAngleX = boxArmLeft.RotateAngleX - (1f + glm.sin(SwingProgress * glm.pi));
            }

            boxFingersRight.RotateAngleX = boxArmRight.RotateAngleX;
            boxFingersLeft.RotateAngleX = boxArmLeft.RotateAngleX;
        }
    }
}
