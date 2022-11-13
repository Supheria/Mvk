﻿using MvkServer.Glm;
using MvkServer.NBT;
using MvkServer.Util;
using MvkServer.World;

namespace MvkServer.Entity
{
    /// <summary>
    /// Объект сущьности c головой
    /// </summary>
    public abstract class EntityLivingHead : EntityLiving
    {
        /// <summary>
        /// Вращение головы
        /// </summary>
        public float RotationYawHead { get; private set; }
        /// <summary>
        /// Вращение головы на предыдущем тике
        /// </summary>
        public float RotationYawHeadPrev { get; protected set; }

        public EntityLivingHead(WorldBase world) : base(world) { }

        /// <summary>
        /// Вызывается для обновления позиции / логики объекта
        /// </summary>
        public override void Update()
        {
            base.Update();
            // Для вращении головы
            HeadTurn();
        }

        /// <summary>
        /// Вращение головы
        /// </summary>
        public override float GetRotationYawHead() => RotationYawHead;

        /// <summary>
        /// Задать вращение
        /// </summary>
        public void SetRotationHead(float yawHead, float pitch)
        {
            RotationYawHead = yawHead;
            SetRotation(RotationYaw, pitch);
        }

        /// <summary>
        /// Задать место положение игрока, при спавне, телепорте и тп
        /// </summary>
        public override void SetPosLook(vec3 pos, float yaw, float pitch)
        {
            base.SetPosLook(pos, yaw, pitch);
            RotationYawHeadPrev = RotationYawHead = RotationYaw;
        }

        /// <summary>
        /// Получить вектор направления куда смотрит сущность
        /// </summary>
        public vec3 GetLook() => GetRay(RotationYawHead, RotationPitch);

        #region Frame

        /// <summary>
        /// Получить угол Yaw для кадра
        /// </summary>
        /// <param name="timeIndex">Коэфициент между тактами</param>
        public override float GetRotationYawFrame(float timeIndex)
        {
            if (timeIndex >= 1.0f || RotationYawHeadPrev == RotationYawHead) return RotationYawHead;
            return RotationYawHeadPrev + (RotationYawHead - RotationYawHeadPrev) * timeIndex;
        }

        /// <summary>
        /// Получить вектор направления камеры от головы
        /// </summary>
        /// <param name="timeIndex">Коэфициент между тактами</param>
        public override vec3 GetLookFrame(float timeIndex)
            => GetRay(GetRotationYawFrame(timeIndex), GetRotationPitchFrame(timeIndex));

        #endregion

        /// <summary>
        /// Проверить градусы
        /// </summary>
        protected override void CheckRotation()
        {
            base.CheckRotation();
            while (RotationYawHead - RotationYawHeadPrev < -glm.pi) RotationYawHeadPrev -= glm.pi360;
            while (RotationYawHead - RotationYawHeadPrev >= glm.pi) RotationYawHeadPrev += glm.pi360;
        }

        /// <summary>
        /// Получить градус поворота по Yaw
        /// </summary>
        protected override float GetRotationYaw() => RotationYawHead;

        /// <summary>
        /// Поворот тела от движения и поворота головы 
        /// </summary>
        protected override void HeadTurn()
        {
            float yawOffset = RotationYaw;

            if (swingProgress > 0)
            {
                // Анимация движении руки
                yawOffset = RotationYawHead;
            }
            else
            {
                float xDis = Position.x - PositionPrev.x;
                float zDis = Position.z - PositionPrev.z;
                float movDis = xDis * xDis + zDis * zDis;
                if (movDis > 0.0025f)
                {
                    // Движение, высчитываем угол направления
                    yawOffset = glm.atan2(zDis, xDis) + glm.pi90;
                    // Реверс для бега назад
                    float yawRev = glm.wrapAngleToPi(yawOffset - RotationYaw);
                    if (yawRev < -1.8f || yawRev > 1.8f) yawOffset += glm.pi;
                }
            } 
            
            float yaw2 = glm.wrapAngleToPi(yawOffset - RotationYaw);
            RotationYaw += yaw2 * .3f;
            float yaw3 = glm.wrapAngleToPi(RotationYawHead - RotationYaw);

            float angleR = glm.pi45;
            if (yaw3 < -angleR) yaw3 = -angleR;
            if (yaw3 > angleR) yaw3 = angleR;

            RotationYaw = RotationYawHead - yaw3;

            // Смещаем тело если дельта выше 60 градусов
            if (yaw3 * yaw3 > 1.1025f) RotationYaw += yaw3 * .2f;

            RotationYaw = glm.wrapAngleToPi(RotationYaw);

            CheckRotation();
        }

        /// <summary>
        /// Обновить поворот с ограниченым смещением
        /// </summary>
        /// <param name="angle">Входящий угол в радианах</param>
        /// <param name="angleOffset">Смещение угла в радианах</param>
        /// <param name="increment">Увеличение в радианах</param>
        /// <returns>Новое значение</returns>
        //private float UpdateRotation(float angle, float angleOffset, float increment)
        //{
        //    float offset = glm.wrapAngleToPi(angleOffset - angle);

        //    if (offset > increment) offset = increment;
        //    if (offset < -increment) offset = -increment;

        //    return angle + offset;
        //}

        /// <summary>
        /// Дополнительное обновление сущности в клиентской части в зависимости от сущности
        /// </summary>
        protected override void UpdateEntityRotation()
        {
            RotationYawHeadPrev = RotationYawHead;
            SetRotationHead(RotationYawServer, RotationPitchServer);
        }

        public override string ToString()
        {
            vec3 m = motionDebug;
            m.y = 0;
            vec3 my = new vec3(0, motionDebug.y, 0);
            float rotationYawHead = glm.degrees(RotationYawHead);
            return string.Format("{15}-{16} XYZ {7} ch:{12}\r\n{0:0.000} | {13:0.000} м/c\r\nHealth: {14:0.00} Air: {17}\r\nyaw:{8:0.00} H:{9:0.00} pitch:{10:0.00} {18} \r\n{1}{2}{6}{4}{19}{20}{21} boom:{5:0.00}\r\nMotion:{3}\r\n{11}",
                glm.distance(m) * 10f, // 0
                OnGround ? "__" : "", // 1
                IsSprinting() ? "[Sp]" : "", // 2
                Motion, // 3
                IsJumping ? "[J]" : "", // 4
                fallDistanceResult, // 5
                IsSneaking() ? "[Sn]" : "", // 6
                Position, // 7
                glm.degrees(RotationYaw), // 8
                rotationYawHead, // 9
                glm.degrees(RotationPitch), // 10
                IsCollidedHorizontally, // 11
                GetChunkPos(), // 12
                glm.distance(my) * 10f, // 13
                Health, // 14
                Id, // 15
                Type, // 16
                GetAir(), // 17
                EnumFacing.FromAngle(rotationYawHead), // 18
                IsInWater() ? "[W]" : "", // 19
                IsInLava() ? "[L]" : "", // 20
                IsInOil() ? "[O]" : "" // 21
                );
        }

        public override void WriteEntityToNBT(TagCompound nbt)
        {
            base.WriteEntityToNBT(nbt);
            nbt.SetTag("Rotation", new TagList(new float[] { RotationYawHead, RotationPitch }));
        }

        public override void ReadEntityFromNBT(TagCompound nbt)
        {
            base.ReadEntityFromNBT(nbt);
            RotationYawHeadPrev = RotationYawHead = RotationYaw;
        }
    }
}
