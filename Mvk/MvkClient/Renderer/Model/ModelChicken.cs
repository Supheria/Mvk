using MvkServer.Entity;
using MvkServer.Glm;

namespace MvkClient.Renderer.Model
{
    /// <summary>
    /// Модель курочки
    /// </summary>
    public class ModelChicken : ModelBase
    {
        private ModelRender boxHead;
        private ModelRender boxHeadSleep;
        /// <summary>
        /// Клюв
        /// </summary>
        private ModelRender boxBill;
        /// <summary>
        /// Красная висюлька
        /// </summary>
        private ModelRender boxChin;
        /// <summary>
        /// Гребент
        /// </summary>
        private ModelRender boxComb;
        private ModelRender boxBody;
        private ModelRender boxArmRight;
        private ModelRender boxArmLeft;
        private ModelRender boxLegRight;
        private ModelRender boxLegLeft;
        /// <summary>
        /// Хвост
        /// </summary>
        private ModelRender boxTail;
        

        public ModelChicken()
        {
            TextureSize = new vec2(64f, 32f);
            boxHead = new ModelRender(this, 0, 0);
            boxHead.SetBox(-1.5f, -5, -3, 3, 6, 4, 0);
            boxHead.SetRotationPoint(0, 15, -4);
            boxHeadSleep = new ModelRender(this, 50, 0);
            boxHeadSleep.SetBox(-1.5f, -5, -3, 3, 6, 4, 0);
            boxHeadSleep.SetRotationPoint(0, 15, -4);
            boxBill = new ModelRender(this, 14, 0);
            boxBill.SetBox(-.5f, -4, -5, 1, 1, 2, 0);
            boxBill.SetRotationPoint(0, 15, -4);
            boxChin = new ModelRender(this, 14, 3);
            boxChin.SetBox(-1, -3, -4, 2, 2, 1, 0);
            boxChin.SetRotationPoint(0, 15, -4);

            boxComb = new ModelRender(this, 0, 24);
            boxComb.SetBox(-.5f, -6, -4, 1, 2, 3, 0);
            boxComb.SetRotationPoint(0, 15, -4);

            boxBody = new ModelRender(this, 0, 10) { RotationPointY = 16 };
            boxBody.SetBox(-3, -4, -3, 6, 8, 6, 0);

            boxArmRight = new ModelRender(this, 24, 14);
            boxArmRight.SetBox(0, 0, -3, 1, 4, 6, 0);
            boxArmRight.SetRotationPoint(-4, 13, 0);
            boxArmLeft = new ModelRender(this, 24, 14);
            boxArmLeft.Mirror();
            boxArmLeft.SetBox(-1, 0, -3, 1, 4, 6, 0);
            boxArmLeft.SetRotationPoint(4, 13, 0);
            boxLegRight = new ModelRender(this, 26, 0);
            boxLegRight.SetBox(-1, 0, -3, 3, 5, 3, 0);
            boxLegRight.SetRotationPoint(-2, 19, 1);
            boxLegLeft = new ModelRender(this, 26, 0);
            boxLegLeft.Mirror();
            boxLegLeft.SetBox(-1, 0, -3, 3, 5, 3, 0);
            boxLegLeft.SetRotationPoint(1, 19, 1);

            boxTail = new ModelRender(this, 9, 24);
            boxTail.SetBox(-2, -1, -3, 4, 4, 4, 0);
            boxTail.SetRotationPoint(0, 13, 5);
        }

        public override void Render(EntityLiving entity, float limbSwing, float limbSwingAmount, float ageInTicks, float headYaw, float headPitch, float scale)
        {
            SetRotationAngles(entity, limbSwing, limbSwingAmount, ageInTicks, headYaw, headPitch, scale);
            GLRender.PushMatrix();

            if (entity.IsSneaking())
            {
                GLWindow.gl.Translate(0, .25f, 0);
            }
            else
            {
                GLWindow.gl.Translate(0, -.04f, 0);
            }

            if (entity.IsSleep())
            {
                boxHeadSleep.Render(scale);
            }
            else
            {
                boxHead.Render(scale);
            }

            boxBill.Render(scale);
            boxChin.Render(scale);
            boxBody.Render(scale);
            boxComb.Render(scale);
            boxArmRight.Render(scale);
            boxArmLeft.Render(scale);
            boxLegRight.Render(scale);
            boxLegLeft.Render(scale);
            boxTail.Render(scale);

            GLRender.PopMatrix();
        }

        protected override void SetRotationAngles(EntityLiving entity, float limbSwing,
            float limbSwingAmount, float ageInTicks, float headYaw, float headPitch, float scale)
        {
            if (entity.IsSleep())
            {
                boxHeadSleep.RotateAngleY = headYaw;
                // Движение головы во сне
                boxHeadSleep.RotateAngleX = glm.pi20 + glm.cos(ageInTicks * 0.09f) * 0.05f + 0.05f;
                boxComb.RotateAngleX = boxBill.RotateAngleX = boxChin.RotateAngleX = boxHeadSleep.RotateAngleX;
                boxComb.RotateAngleY = boxBill.RotateAngleY = boxChin.RotateAngleY = boxHeadSleep.RotateAngleY;
            }
            else
            {
                boxHead.RotateAngleY = headYaw;
                boxHead.RotateAngleX = -headPitch;
                boxComb.RotateAngleX = boxBill.RotateAngleX = boxChin.RotateAngleX = boxHead.RotateAngleX;
                boxComb.RotateAngleY = boxBill.RotateAngleY = boxChin.RotateAngleY = boxHead.RotateAngleY;
            }

            boxLegRight.RotateAngleX = glm.cos(limbSwing * 2.6662f) * 1.4f * limbSwingAmount;
            boxLegLeft.RotateAngleX = glm.cos(limbSwing * 2.6662f + glm.pi) * 1.4f * limbSwingAmount;

            if (entity.OnGround)
            {
                boxArmRight.RotateAngleZ = 0;
                boxArmLeft.RotateAngleZ = 0;
            }
            else
            {
                boxArmRight.RotateAngleZ = glm.cos(ageInTicks * 2f + glm.pi) + glm.pi45 + .2f;
                boxArmLeft.RotateAngleZ = glm.cos(ageInTicks * 2f) - glm.pi45 - .2f;
            }
            
            boxBody.RotateAngleX = glm.pi90;

            if (entity.IsSneaking())
            {
                boxHeadSleep.RotationPointY = boxHead.RotationPointY = 16.5f;
                boxComb.RotationPointY = boxBill.RotationPointY = boxChin.RotationPointY = 16.5f;
                boxLegRight.RotationPointY = 15;
                boxLegLeft.RotationPointY = 15;
            }
            else
            {
                boxHead.RotationPointY = 14;
                boxComb.RotationPointY = boxBill.RotationPointY = boxChin.RotationPointY = 14;
                boxLegRight.RotationPointY = 19;
                boxLegLeft.RotationPointY = 19;
            }
        }
    }
}
