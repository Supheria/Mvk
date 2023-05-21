using MvkServer.Glm;

namespace MvkServer.World.Block
{
    /// <summary>
    /// Структурка для получении данных взаимосвязи сущности с блоками жидкости
    /// </summary>
    public struct Liquid
    {
        private bool water;
        private bool lava;
        private bool oil;
        private bool tina;
        private bool fire;

        private bool isPushedByLiquid;
        private vec3 vecOil;
        private vec3 vecWater;

        public bool IsWater() => water;
        public bool IsLava() => lava;
        public bool IsOil() => oil;
        public bool IsTina() => tina;
        public bool IsFire() => fire;

        public vec3 GetVecOil() => vecOil;
        public vec3 GetVecWater() => vecWater;
        public void VecSpeed()
        {
            if (vecWater.x != 0 || vecWater.y != 0 || vecWater.z != 0)
            {
                isPushedByLiquid = true;
                // скорость смещение в воде
                vecWater = vecWater.normalize() * .028f;
            }
            if (vecOil.x != 0 || vecOil.y != 0 || vecOil.z != 0)
            {
                isPushedByLiquid = true;
                // скорость смещение в нефте
                vecOil = vecOil.normalize() * .008f;
            }
        }
        public vec3 GetVec() => vecOil + vecWater;
        /// <summary>
        /// Имеются ли толчки от воды
        /// </summary>
        public bool IsPushedByLiquid() => isPushedByLiquid;

        public void Water(vec3 vec)
        {
            vecWater = vec;
            water = true;
        }
        public void Lava() => lava = true;
        public void Oil(vec3 vec)
        {
            vecOil = vec;
            oil = true;
        }
        public void Fire() => fire = true;
        public void Tina() => tina = true;
    }
}
