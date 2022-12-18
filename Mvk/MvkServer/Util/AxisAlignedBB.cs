using MvkServer.Glm;

namespace MvkServer.Util
{
    /// <summary>
    /// Выровненные по осям ограничивающие рамки
    /// Axis-aligned bounding boxes
    /// </summary>
    public class AxisAlignedBB
    {
        /// <summary>
        /// Погрешность
        /// </summary>
        private const float FAULT = 0.001f;

        public vec3 Min { get; protected set; }
        public vec3 Max { get; protected set; }

        public AxisAlignedBB(vec3 from, vec3 to)
        {
            Min = new vec3(Mth.Min(from.x, to.x), Mth.Min(from.y, to.y), Mth.Min(from.z, to.z));
            Max = new vec3(Mth.Max(from.x, to.x), Mth.Max(from.y, to.y), Mth.Max(from.z, to.z));
        }

        public AxisAlignedBB(float fromX, float fromY, float fromZ, float toX, float toY, float toZ)
        {
            Min = new vec3(Mth.Min(fromX, toX), Mth.Min(fromY, toY), Mth.Min(fromZ, toZ));
            Max = new vec3(Mth.Max(fromX, toX), Mth.Max(fromY, toY), Mth.Max(fromZ, toZ));
        }

        public vec3i MinInt() => new vec3i(Min);
        public vec3i MaxInt() => new vec3i(Max);

        public AxisAlignedBB Clone() => new AxisAlignedBB(Min, Max);
        /// <summary>
        /// Смещает текущую ограничивающую рамку на указанные координаты
        /// </summary>
        public AxisAlignedBB Offset(vec3 bias) => new AxisAlignedBB(Min + bias, Max + bias);

        /// <summary>
        /// Добавить координату в область как смещение от 0
        /// </summary>
        public AxisAlignedBB AddCoordBias(vec3 pos)
        {
            vec3 min = new vec3(Min);
            vec3 max = new vec3(Max);

            if (pos.x < 0f) min.x += pos.x;
            else if (pos.x > 0f) max.x += pos.x;

            if (pos.y < 0f) min.y += pos.y;
            else if (pos.y > 0f) max.y += pos.y;

            if (pos.z < 0f) min.z += pos.z;
            else if (pos.z > 0f) max.z += pos.z;

            return new AxisAlignedBB(min, max);
        }

        /// <summary>
        /// Добавить координату в область
        /// </summary>
        public AxisAlignedBB AddCoord(vec3 pos)
        {
            vec3 min = new vec3(Min);
            vec3 max = new vec3(Max);

            if (pos.x < min.x) min.x = pos.x;
            else if (pos.x > max.x) max.x = pos.x;

            if (pos.y < min.y) min.y = pos.y;
            else if (pos.y > max.y) max.y = pos.y;

            if (pos.z < min.z) min.z = pos.z;
            else if (pos.z > max.z) max.z = pos.z;

            return new AxisAlignedBB(min, max);
        }

        /// <summary>
        /// Возвращает ограничивающую рамку, расширенную указанным вектором
        /// </summary>
        public AxisAlignedBB Expand(vec3 vec) => new AxisAlignedBB(Min - vec, Max + vec);

        /// <summary>
        /// Возвращает ограничивающую рамку, уменьшенную указанным вектором
        /// </summary>
        public AxisAlignedBB Contract(vec3 vec) => new AxisAlignedBB(Min + vec, Max - vec);

        /// <summary>
        /// Если экземпляр и ограничивающие рамки аргумента перекрываются в измерениях Y и Z, 
        /// вычислите смещение между ними в измерении X. вернуть offset, если ограничивающие 
        /// рамки не перекрываются или если offset ближе к 0, чем вычисленное смещение. 
        /// В противном случае вернуть рассчитанное смещение.
        /// </summary>
        /// <param name="other"></param>
        /// <param name="offset">смещение</param>
        /// <returns></returns>
        public float CalculateXOffset(AxisAlignedBB other, float offset)
        {
            if (other.Max.y > Min.y && other.Min.y < Max.y && other.Max.z > Min.z && other.Min.z < Max.z)
            {
                if (offset > 0f && other.Max.x <= Min.x)
                {
                    float bias = Min.x - other.Max.x;
                    if (bias < offset) offset = bias - FAULT;
                }
                else if (offset < 0f && other.Min.x >= Max.x)
                {
                    float bias = Max.x - other.Min.x;
                    if (bias > offset) offset = bias + FAULT;
                }
            }
            return offset;
        }

        /// <summary>
        /// Если экземпляр и ограничивающие рамки аргумента перекрываются в измерениях X и Z, 
        /// вычислите смещение между ними в измерении Y. вернуть offset, если ограничивающие 
        /// рамки не перекрываются или если offset ближе к 0, чем вычисленное смещение. 
        /// В противном случае вернуть рассчитанное смещение.
        /// </summary>
        /// <param name="other"></param>
        /// <param name="offset">смещение</param>
        /// <returns></returns>
        public float CalculateYOffset(AxisAlignedBB other, float offset)
        {
            if (other.Max.x > Min.x && other.Min.x < Max.x && other.Max.z > Min.z && other.Min.z < Max.z)
            {
                if (offset > 0f && other.Max.y <= Min.y)
                {
                    float bias = Min.y - other.Max.y;
                    if (bias < offset) offset = bias - FAULT;
                }
                else if (offset < 0f && other.Min.y >= Max.y)
                {
                    float bias = Max.y - other.Min.y;
                    if (bias > offset) offset = bias + FAULT;
                }
            }
            return offset;
        }

        /// <summary>
        /// Если экземпляр и ограничивающие рамки аргумента перекрываются в измерениях Y и X, 
        /// вычислите смещение между ними в измерении Z. вернуть offset, если ограничивающие 
        /// рамки не перекрываются или если offset ближе к 0, чем вычисленное смещение. 
        /// В противном случае вернуть рассчитанное смещение.
        /// </summary>
        /// <param name="other"></param>
        /// <param name="offset">смещение</param>
        /// <returns></returns>
        public float CalculateZOffset(AxisAlignedBB other, float offset)
        {
            if (other.Max.x > Min.x && other.Min.x < Max.x && other.Max.y > Min.y && other.Min.y < Max.y)
            {
                if (offset > 0f && other.Max.z <= Min.z)
                {
                    float bias = Min.z - other.Max.z;
                    if (bias < offset) offset = bias - FAULT;
                }
                else if (offset < 0f && other.Min.z >= Max.z)
                {
                    float bias = Max.z - other.Min.z;
                    if (bias > offset) offset = bias + FAULT;
                }
            }
            return offset;
        }

        /// <summary>
        /// Рассчитать точку пересечения в AABB и отрезка, в виде вектора от pos1 до pos2
        /// </summary>
        public MovingObjectPosition CalculateIntercept(vec3 pos1, vec3 pos2)
        {
            vec3 posX1 = glm.GetIntermediateWithXValue(pos1, pos2, Min.x);
            vec3 posX2 = glm.GetIntermediateWithXValue(pos1, pos2, Max.x);
            vec3 posY1 = glm.GetIntermediateWithYValue(pos1, pos2, Min.y);
            vec3 posY2 = glm.GetIntermediateWithYValue(pos1, pos2, Max.y);
            vec3 posZ1 = glm.GetIntermediateWithZValue(pos1, pos2, Min.z);
            vec3 posZ2 = glm.GetIntermediateWithZValue(pos1, pos2, Max.z);

            if (!IsVecInYZ(posX1)) posX1 = new vec3(0);
            if (!IsVecInYZ(posX2)) posX2 = new vec3(0);
            if (!IsVecInXZ(posY1)) posY1 = new vec3(0);
            if (!IsVecInXZ(posY2)) posY2 = new vec3(0);
            if (!IsVecInXY(posZ1)) posZ1 = new vec3(0);
            if (!IsVecInXY(posZ2)) posZ2 = new vec3(0);

            vec3 vecResult = posX1;

            if (!posX2.IsZero() && (vecResult.IsZero() || glm.SquareDistanceTo(pos1, posX2) < glm.SquareDistanceTo(pos1, vecResult)))
            {
                vecResult = posX2;
            }

            if (!posY1.IsZero() && (vecResult.IsZero() || glm.SquareDistanceTo(pos1, posY1) < glm.SquareDistanceTo(pos1, vecResult)))
            {
                vecResult = posY1;
            }

            if (!posY2.IsZero() && (vecResult.IsZero() || glm.SquareDistanceTo(pos1, posY2) < glm.SquareDistanceTo(pos1, vecResult)))
            {
                vecResult = posY2;
            }

            if (!posZ1.IsZero() && (vecResult.IsZero() || glm.SquareDistanceTo(pos1, posZ1) < glm.SquareDistanceTo(pos1, vecResult)))
            {
                vecResult = posZ1;
            }

            if (!posZ2.IsZero() && (vecResult.IsZero() || glm.SquareDistanceTo(pos1, posZ2) < glm.SquareDistanceTo(pos1, vecResult)))
            {
                vecResult = posZ2;
            }

            if (vecResult.IsZero()) return null;

            Pole side;
            if (vecResult == posX1) side = Pole.West;
            else if (vecResult == posX2) side = Pole.East;
            else if (vecResult == posY1) side = Pole.Down;
            else if (vecResult == posY2) side = Pole.Up;
            else if (vecResult == posZ1) side = Pole.North;
            else side = Pole.South;

            return new MovingObjectPosition(vecResult, side);
        }

        /// <summary>
        /// Проверяет, находится ли указанный вектор в пределах размеров YZ ограничивающей рамки
        /// </summary>
        private bool IsVecInYZ(vec3 vec) 
            => vec.IsZero() ? false : vec.y >= Min.y && vec.y <= Max.y && vec.z >= Min.z && vec.z <= Max.z;

        /// <summary>
        /// Проверяет, находится ли указанный вектор в пределах размеров XZ ограничивающей рамки
        /// </summary>
        private bool IsVecInXZ(vec3 vec)
            => vec.IsZero() ? false : vec.x >= Min.x && vec.x <= Max.x && vec.z >= Min.z && vec.z <= Max.z;

        /// <summary>
        /// Проверяет, находится ли указанный вектор в пределах размеров XY ограничивающей рамки
        /// </summary>
        private bool IsVecInXY(vec3 vec)
            => vec.IsZero() ? false : vec.x >= Min.x && vec.x <= Max.x && vec.y >= Min.y && vec.y <= Max.y;

        /// <summary>
        /// Возвращает, пересекается ли данная ограничивающая рамка с этой
        /// </summary>
        public bool IntersectsWith(AxisAlignedBB other) => other.Max.x > Min.x && other.Min.x < Max.x 
                ? (other.Max.y > Min.y && other.Min.y < Max.y ? other.Max.z > Min.z && other.Min.z < Max.z : false) 
                : false;

        /// <summary>
        /// Возвращает, если предоставленный vec3 полностью находится внутри ограничивающей рамки.
        /// </summary>
        public bool IsVecInside(vec3 vec) => vec.x > Min.x && vec.x < Max.x
                ? (vec.y > Min.y && vec.y < Max.y ? vec.z > Min.z && vec.z < Max.z : false)
                : false;

        /// <summary>
        /// Возвращает среднюю длину краев ограничивающей рамки
        /// </summary>
        public float GetAverageEdgeLength()
        {
            float x = Max.x - Min.x;
            float y = Max.y - Min.y;
            float z = Max.z - Min.z;
            return (x + y + z) / 3f;
        }

        public override string ToString()
        {
            return "box[" + Min.ToString() + " -> " + Max.ToString() + "]";
        }
    }
}
