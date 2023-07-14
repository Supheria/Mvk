using MvkServer.Entity;
using MvkServer.Glm;
using MvkServer.Item;

namespace MvkClient.Renderer.Model
{
    public class ModelPlayerHand : ModelBase
    {
        public ModelRender BoxArmRight { get; protected set; }

        public ModelPlayerHand()
        {
            BoxArmRight = new ModelRender(this, 40, 16);
            BoxArmRight.SetBox(-2, 0, -2, 4, 12, 4, 0);
        }

        public override void Render(EntityLiving entity, float limbSwing, float limbSwingAmount, float ageInTicks, float headYaw, float headPitch, float scale)
        {
            SetRotationAngles(entity, limbSwing, limbSwingAmount, ageInTicks, headYaw, headPitch, scale);
            BoxArmRight.Render(scale);
        }

        protected override void SetRotationAngles(EntityLiving entity, float limbSwing,
            float limbSwingAmount, float ageInTicks, float headYaw, float headPitch, float scale)
        {
            BoxArmRight.RotateAngleX = 4;
            BoxArmRight.RotateAngleY = 0;
            BoxArmRight.RotationPointZ = 0;
            BoxArmRight.RotationPointX = 0;
            BoxArmRight.RotationPointY = 0;

            if (entity.IsEating())
            {
                // Принимает пищу
                BoxArmRight.RotationPointY = -2;
                BoxArmRight.RotationPointX = -2 - glm.cos(ageInTicks * 0.9f) * 0.3f + 0.3f;
                BoxArmRight.RotateAngleX = 3.6875f;
                BoxArmRight.RotateAngleZ = .25f;
                BoxArmRight.RotateAngleY = -.75f;
            }
            else if (SwingProgress > 0)
            {
                float sp = 1 - SwingProgress;
                sp *= sp;
                sp *= sp;
                float s1 = glm.sin(sp) * .8f;
                float s2 = glm.sin(SwingProgress);
                // Удар правой руки
                if (entity.GetItemUseAction() == EnumItemAction.Shovel)
                {
                    float sp2 = SwingProgress - .3f;
                    BoxArmRight.RotateAngleX += s1;
                    BoxArmRight.RotateAngleY = s2 * -.8f;
                    BoxArmRight.RotationPointZ = sp2 * -16;
                    BoxArmRight.RotationPointY = 4;
                }
                else
                {
                    BoxArmRight.RotateAngleX += s1;
                    BoxArmRight.RotateAngleZ = s1;
                }
            }
            else
            {
                if (entity.GetItemUseAction() == EnumItemAction.Shovel)
                {
                    BoxArmRight.RotationPointY = 4;
                }

                // Движение рук от дыхания
                BoxArmRight.RotateAngleZ = glm.cos(ageInTicks * 0.09f) * 0.05f + 0.05f;
                BoxArmRight.RotateAngleX += glm.sin(ageInTicks * 0.067f) * 0.05f;
            }
        }
    }
}
