using MvkServer.Glm;

namespace MvkServer.Entity.AI
{
    /// <summary>
    /// Перемещение сущности по координатам
    /// </summary>
    public class EntityMoveHelper
    {
        private EntityLiving entity;

        private float posX;
        private float posY;
        private float posZ;

        private bool move;

        /// <summary>
        /// Скорость перемещения
        /// </summary>
        public float Speed { get; private set; } = 0;

        /// <summary>
        /// Обновить перемещение
        /// </summary>
        private bool updateMove = false;
        /// <summary>
        /// Обновить прыжок
        /// </summary>
        private bool updateJump = false;
        /// <summary>
        /// Ускорение
        /// </summary>
        private bool updateSprinting = false;
        /// <summary>
        /// Обновить сесть
        /// </summary>
        private bool updateSneak = false;

        public EntityMoveHelper(EntityLiving entity)
        {
            this.entity = entity;
            posX = entity.Position.x;
            posY = entity.Position.y;
            posZ = entity.Position.z;
        }

        /// <summary>
        /// Устанавливает скорость и место для перемещения
        /// </summary>
        public void SetMoveTo(vec3 pos, float speed) => SetMoveTo(pos.x, pos.y, pos.z, speed);
        /// <summary>
        /// Устанавливает скорость и место для перемещения
        /// </summary>
        public void SetMoveTo(float x, float y, float z, float speed)
        {
            posX = x;
            posY = y;
            posZ = z;
            Speed = speed;
            updateMove = true;
        }

        public void SetJumping() => updateJump = true;

        public void SetSprinting() => updateSprinting = true;

        public void SetSneak() => updateSneak = true;

        public void OnUpdateMove()
        {
            if (move)
            {
                move = false;
                entity.Movement.SetAIStop();
            }
            if (updateMove)
            {
                updateMove = false;
                float x = entity.Position.x - posX;
                float y = posY - entity.Position.y;
                float z = entity.Position.z - posZ;
                float rotationYaw = glm.atan2(z, x) - glm.pi90;
                if (rotationYaw != entity.RotationYawHead)
                {
                    entity.SetRotationHead(rotationYaw, 0);
                }
                // Если сущность может летать, она же плавать, есть возможность управлять вверх и вниз
                if (entity.IsFlying && x * x + z * z < 1f)
                {
                    if (y > .5f) entity.Movement.SetAIJamp();
                    else if (y < .5f) entity.Movement.SetAISneak();
                }
                entity.Movement.SetAIForward(Speed);
                move = true;
            }
            if (updateJump)
            {
                updateJump = false;
                entity.Movement.SetAIJamp();
                move = true;
            }
            if (updateSprinting)
            {
                updateSprinting = false;
                entity.Movement.SetAISprinting();
                move = true;
            }
            if (updateSneak)
            {
                updateSneak = false;
                entity.Movement.SetAISneak();
                move = true;
            }
        }
    }
}
