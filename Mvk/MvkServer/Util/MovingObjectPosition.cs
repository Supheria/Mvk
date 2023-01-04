using MvkServer.Entity;
using MvkServer.Glm;
using MvkServer.World.Block;

namespace MvkServer.Util
{
    /// <summary>
    /// Объект отвечающий какой объект попадает под луч
    /// </summary>
    public class MovingObjectPosition
    {
        /// <summary>
        /// Объект сущьности
        /// </summary>
        public EntityBase Entity { get; private set; }
        /// <summary>
        /// Объект данных блока
        /// </summary>
        public BlockState Block { get; private set; }
        /// <summary>
        /// Позиция блока
        /// </summary>
        public BlockPos BlockPosition { get; private set; }
        /// <summary>
        /// Объект данных житкого блока
        /// </summary>
        public EnumBlock EBlockLiquid { get; private set; }
        /// <summary>
        /// Позиция житкого блока
        /// </summary>
        public BlockPos BlockLiquidPosition { get; private set; }
        /// <summary>
        /// Имеется ли попадание по жидкому блоку
        /// </summary>
        public bool IsLiquid { get; private set; } = false;
        /// <summary>
        /// Нормаль попадания по блоку
        /// </summary>
        public vec3i Norm { get; private set; }
        /// <summary>
        /// Координата куда попал луч в глобальных координатах по блоку
        /// </summary>
        public vec3 RayHit { get; private set; }
        /// <summary>
        /// Точка куда устанавливаем блок (параметр с RayCast)
        /// значение в пределах 0..1, образно фиксируем пиксел клика на стороне
        /// </summary>
        public vec3 Facing { get; private set; }
        /// <summary>
        /// Сторона блока куда смотрит луч, нельзя по умолчанию All, надо строго из 6 сторон
        /// </summary>
        public Pole Side { get; protected set; } = Pole.Up;

        protected MovingObjectType type = MovingObjectType.None;

        /// <summary>
        /// Нет попадания
        /// </summary>
        public MovingObjectPosition() => Block = new BlockState().Empty();

        /// <summary>
        /// Попадаем в блок
        /// </summary>
        /// <param name="blockState">блок</param>
        /// <param name="pos">позиция блока</param>
        /// <param name="side">Сторона блока куда смотрит луч</param>
        /// <param name="facing">Значение в пределах 0..1, образно фиксируем пиксел клика на стороне</param>
        /// <param name="norm">Нормаль попадания</param>
        /// <param name="rayHit">Координата куда попал луч</param>
        public MovingObjectPosition(BlockState blockState, BlockPos pos, Pole side, vec3 facing, vec3i norm, vec3 rayHit)
        {
            Block = blockState;
            BlockPosition = pos;
            Facing = facing;
            Side = side;
            RayHit = rayHit;
            Norm = norm;
            type = MovingObjectType.Block;
        }

        /// <summary>
        /// Проверка вектора, с какой стороны попали, точка попадания
        /// </summary>
        /// <param name="vec">точка попадания</param>
        /// <param name="side">с какой стороны попали</param>
        public MovingObjectPosition(vec3 vec, Pole side)
        {
            type = MovingObjectType.Block;
            RayHit = vec;
            Side = side;
        }

        /// <summary>
        /// Попадаем в сущность
        /// </summary>
        /// <param name="entity">сущьность</param>
        public MovingObjectPosition(EntityBase entity, vec3 rayHit)
        {
            Block = new BlockState().Empty();
            Entity = entity;
            RayHit = rayHit;
            type = MovingObjectType.Entity;
        }

        public bool IsBlock() => type == MovingObjectType.Block;

        public bool IsEntity() => type == MovingObjectType.Entity;

        public bool IsCollision() => type != MovingObjectType.None;

        /// <summary>
        /// Задать попадание по жидкому блоку
        /// </summary>
        public void SetLiquid(EnumBlock enumBlock, BlockPos blockPos)
        {
            IsLiquid = true;
            EBlockLiquid = enumBlock;
            BlockLiquidPosition = blockPos;
        }

        /// <summary>
        /// Тип объекта
        /// </summary>
        protected enum MovingObjectType
        {
            None = 0,
            Block = 1,
            Entity = 2
        }

        public override string ToString()
        {
            string str = "";
            if (type == MovingObjectType.Entity)
            {
                float h = Entity is EntityLiving ? ((EntityLiving)Entity).GetHealth() : 0; 
                str = Entity.GetName() + " " + h + " " + Entity.Position;
            }
            return string.Format("{0} {3} {1} {2} {4}", type, Side, Facing, str, IsLiquid ? EBlockLiquid.ToString() : "");
        }
    }
}
