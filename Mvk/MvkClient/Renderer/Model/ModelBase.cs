﻿using MvkServer.Glm;

namespace MvkClient.Renderer.Model
{
    /// <summary>
    /// Базовый объект 3д моделей
    /// </summary>
    public abstract class ModelBase
    {
        /// <summary>
        /// Размер ширины файла текстуры в пикселях.
        /// </summary>
        public vec2 TextureSize { get; protected set; } = new vec2(64f, 64f);
        protected bool isSneak = false;
        protected bool onGround = true;

        public ModelBase(bool sneak, bool onGround)
        {
            isSneak = sneak;
            this.onGround = onGround;
        }

        public virtual void Render(float limbSwing, float limbSwingAmount, float ageInTicks, float headYaw, float headPitch, float scale) { }

        /// <summary>
        /// Устанавливает различные углы поворота модели. Для двуногих, par1 и par2 используются для анимации движения рук и ног, где par1 представляет время (так что руки и ноги качаются вперед и назад), а par2 представляет, насколько "далеко" руки и ноги могут раскачиваться максимум.
        /// </summary>
        /// <param name="limbSwing">счётчик скорости</param>
        /// <param name="limbSwingAmount">Амплитуда 0 нет движения 1.0 максимальная амплитуда</param>
        /// <param name="ageInTicks"></param>
        /// <param name="headYaw"></param>
        /// <param name="headPitch"></param>
        /// <param name="scale">0.0625F коэффициент масштабирования</param>
        /// <param name="entity">Сущность</param>
        protected virtual void SetRotationAngles(float limbSwing, float limbSwingAmount, float ageInTicks, float headYaw, float headPitch, float scale) { }
    }
}
