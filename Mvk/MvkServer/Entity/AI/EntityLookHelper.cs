using MvkServer.Glm;
using MvkServer.Util;

namespace MvkServer.Entity.AI
{
    /// <summary>
    /// Вращение сущности, с возможностью плавности
    /// </summary>
    public class EntityLookHelper
    {
        private EntityLiving entity;

        /// <summary>
        /// Количество изменений, которые вносятся при каждом обновлении для объекта, 
        /// обращенного в направлении Yaw
        /// </summary>
        private float deltaLookYaw;

        /// <summary>
        /// Количество изменений, которые вносятся при каждом обновлении объекта, 
        /// обращенного в направлении Pitch
        /// </summary>
        private float deltaLookPitch;

        /// <summary>
        /// Независимо от того, пытается ли сущность смотреть на что-то
        /// </summary>
        private bool isLooking;
        /// <summary>
        /// Позиция куда смотрим
        /// </summary>
        private vec3 posWhereLooking;

        private bool isLookingOffset;
        private float renderYawOffset;
        private float renderPitchOffset;

        public EntityLookHelper(EntityLiving entity) => this.entity = entity;

        /// <summary>
        /// Устанавливает позицию для просмотра с помощью объекта
        /// </summary>
        public void SetLookPositionWithEntity(EntityBase inEntity, float deltaYaw, float deltaPitch)
        {
            posWhereLooking = inEntity.Position;
            if (inEntity is EntityLiving entityLiving)
            {
                posWhereLooking.y += entityLiving.GetEyeHeight();
            }
            else
            {
                posWhereLooking.y += (inEntity.BoundingBox.Max.y - inEntity.BoundingBox.Min.y) / 2f;
            }
            deltaLookYaw = deltaYaw;
            deltaLookPitch = deltaPitch;
            isLooking = true;
        }

        /// <summary>
        /// Устанавливает позицию для просмотра
        /// </summary>
        public void SetLookPosition(vec3 pos, float deltaYaw, float deltaPitch)
        {
            posWhereLooking = pos;
            deltaLookYaw = deltaYaw;
            deltaLookPitch = deltaPitch;
            isLooking = true;
        }

        /// <summary>
        /// Устанавливает позицию для просмотра вертикали
        /// </summary>
        public void SetLookPitch(float pitch, float deltaPitch)
        {
            isLookingOffset = true;
            isLooking = false;
            renderYawOffset = entity.RotationYawHead;
            deltaLookPitch = deltaPitch;
            renderPitchOffset = pitch;
            //entity.SetRotationHead(entity.RotationYawHead, pitch);
        }

        public void OnUpdateLook()
        {
            float rotationPitch;
            float rotationYaw;

            if (isLooking)
            {
                isLookingOffset = true;
                isLooking = false;
                float x = entity.Position.x - posWhereLooking.x;
                float y = (entity.Position.y + entity.GetEyeHeight()) - posWhereLooking.y;
                float z = entity.Position.z - posWhereLooking.z;
                float k = Mth.Sqrt(x * x + z * z);
                renderYawOffset = glm.atan2(z, x) - glm.pi90;
                renderPitchOffset = -glm.atan2(y, k);
                rotationYaw = UpdateRotation(entity.RotationYawHead, renderYawOffset, deltaLookYaw);
                rotationPitch = UpdateRotation(entity.RotationPitch, renderPitchOffset, deltaLookPitch);
                if (rotationYaw != entity.RotationYawHead || rotationPitch != entity.RotationPitch)
                {
                    entity.SetRotationHead(rotationYaw, rotationPitch);
                }
            }
            else if(isLookingOffset)
            {
                rotationYaw = UpdateRotation(entity.RotationYawHead, renderYawOffset, deltaLookYaw);
                rotationPitch = UpdateRotation(entity.RotationPitch, renderPitchOffset, deltaLookPitch);
                
                if (rotationYaw == entity.RotationYawHead && rotationPitch == entity.RotationPitch)
                {
                    isLookingOffset = false;
                }
                else
                {
                    entity.SetRotationHead(rotationYaw, rotationPitch);
                }
            }
        }

        private float UpdateRotation(float angle1, float angle2, float delta)
        {
            float angle = glm.wrapAngleToPi(angle2 - angle1);
            if (Mth.IsZero(angle)) return angle1;
            if (angle > delta) angle = delta;
            if (angle < -delta) angle = -delta;
            return angle1 + angle;
        }
    }
}
