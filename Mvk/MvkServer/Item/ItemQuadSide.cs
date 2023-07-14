using MvkServer.Glm;
using MvkServer.Util;

namespace MvkServer.World.Item
{
    /// <summary>
    /// Прямоугольная сторона предмета или элемента премета
    /// </summary>
    public struct ItemQuadSide
    {
        /// <summary>
        /// Четыре вершины
        /// </summary>
        public readonly Vertex[] vertex;
        /// <summary>
        /// Боковое затемнение
        /// </summary>
        public float lightPole;

        /// <summary>
        /// Задать текстуру, её можно повернуть кратно 90 гр. 0 - 3 => 0 - 270
        /// </summary>
        public ItemQuadSide(int numberTexture, int rotateYawUV = 0)
        {
            lightPole = 0;
            vertex = new Vertex[4];
            float u1 = (numberTexture % 32) * .03125f;
            float u2 = u1 + .03125f;
            float v1 = numberTexture / 32 * .03125f;
            float v2 = v1 + .03125f;
            switch (rotateYawUV)
            {
                case 0:
                    vertex[0].u = vertex[1].u = u2;
                    vertex[2].u = vertex[3].u = u1;
                    vertex[0].v = vertex[3].v = v2;
                    vertex[1].v = vertex[2].v = v1;
                    break;
                case 1:
                    vertex[1].u = vertex[2].u = u2;
                    vertex[0].u = vertex[3].u = u1;
                    vertex[0].v = vertex[1].v = v2;
                    vertex[2].v = vertex[3].v = v1;
                    break;
                case 2:
                    vertex[0].u = vertex[1].u = u1;
                    vertex[2].u = vertex[3].u = u2;
                    vertex[0].v = vertex[3].v = v1;
                    vertex[1].v = vertex[2].v = v2;
                    break;
                case 3:
                    vertex[1].u = vertex[2].u = u1;
                    vertex[0].u = vertex[3].u = u2;
                    vertex[0].v = vertex[1].v = v1;
                    vertex[2].v = vertex[3].v = v2;
                    break;
            }
        }

        /// <summary>
        /// Задать текстуру, и задать ей размеры не полного сэмпла и можно повернуть кратно 90 гр. 0 - 3 => 0 - 270
        /// </summary>
        public ItemQuadSide(int numberTexture, int biasU1, int biasV1, int biasU2, int biasV2, int rotateYawUV = 0)
        {
            lightPole = 0;
            vertex = new Vertex[4];
            float u = (numberTexture % 32) * .03125f;
            float v = numberTexture / 32 * .03125f;

            switch (rotateYawUV)
            {
                case 0:
                    vertex[0].u = vertex[1].u = u + biasU2 / 1024f;
                    vertex[2].u = vertex[3].u = u + biasU1 / 1024f;
                    vertex[0].v = vertex[3].v = v + biasV2 / 1024f;
                    vertex[1].v = vertex[2].v = v + biasV1 / 1024f;
                    break;
                case 1:
                    vertex[1].u = vertex[2].u = u + biasU2 / 1024f;
                    vertex[0].u = vertex[3].u = u + biasU1 / 1024f;
                    vertex[0].v = vertex[1].v = v + biasV2 / 1024f;
                    vertex[2].v = vertex[3].v = v + biasV1 / 1024f;
                    break;
                case 2:
                    vertex[0].u = vertex[1].u = u + biasU1 / 1024f;
                    vertex[2].u = vertex[3].u = u + biasU2 / 1024f;
                    vertex[0].v = vertex[3].v = v + biasV1 / 1024f;
                    vertex[1].v = vertex[2].v = v + biasV2 / 1024f;
                    break;
                case 3:
                    vertex[1].u = vertex[2].u = u + biasU1 / 1024f;
                    vertex[0].u = vertex[3].u = u + biasU2 / 1024f;
                    vertex[0].v = vertex[1].v = v + biasV1 / 1024f;
                    vertex[2].v = vertex[3].v = v + biasV2 / 1024f;
                    break;
            }
        }

        /// <summary>
        /// Задаём сторону и размеры на стороне
        /// </summary>
        /// <param name="pole">индекс стороны</param>
        /// <param name="noSideDimming">Нет бокового затемнения, пример: трава, цветы</param>
        public ItemQuadSide SetSide(Pole pole, bool noSideDimming = false, int x1i = 0, int y1i = 0, int z1i = 0, int x2i = 32, int y2i = 32, int z2i = 32)
        {
            float x1 = x1i / 32f;
            float y1 = y1i / 32f;
            float z1 = z1i / 32f;
            float x2 = x2i / 32f;
            float y2 = y2i / 32f;
            float z2 = z2i / 32f;
            lightPole = noSideDimming ? 0f : 1f - MvkStatic.LightPoles[(int)pole];
            switch (pole)
            {
                case Pole.Up:
                    vertex[0].x = vertex[1].x = x1;
                    vertex[2].x = vertex[3].x = x2;
                    vertex[0].y = vertex[1].y = vertex[2].y = vertex[3].y = y2;
                    vertex[0].z = vertex[3].z = z1;
                    vertex[1].z = vertex[2].z = z2;
                    break;
                case Pole.Down:
                    vertex[0].x = vertex[1].x = x2;
                    vertex[2].x = vertex[3].x = x1;
                    vertex[0].y = vertex[1].y = vertex[2].y = vertex[3].y = y1;
                    vertex[0].z = vertex[3].z = z1;
                    vertex[1].z = vertex[2].z = z2;
                    break;
                case Pole.East:
                    vertex[0].x = vertex[1].x = vertex[2].x = vertex[3].x = x2;
                    vertex[0].y = vertex[3].y = y1;
                    vertex[1].y = vertex[2].y = y2;
                    vertex[0].z = vertex[1].z = z1;
                    vertex[2].z = vertex[3].z = z2;
                    break;
                case Pole.West:
                    vertex[0].x = vertex[1].x = vertex[2].x = vertex[3].x = x1;
                    vertex[0].y = vertex[3].y = y1;
                    vertex[1].y = vertex[2].y = y2;
                    vertex[0].z = vertex[1].z = z2;
                    vertex[2].z = vertex[3].z = z1;
                    break;
                case Pole.North:
                    vertex[0].x = vertex[1].x = x1;
                    vertex[2].x = vertex[3].x = x2;
                    vertex[0].y = vertex[3].y = y1;
                    vertex[1].y = vertex[2].y = y2;
                    vertex[0].z = vertex[1].z = vertex[2].z = vertex[3].z = z1;
                    break;
                case Pole.South:
                    vertex[0].x = vertex[1].x = x2;
                    vertex[2].x = vertex[3].x = x1;
                    vertex[0].y = vertex[3].y = y1;
                    vertex[1].y = vertex[2].y = y2;
                    vertex[0].z = vertex[1].z = vertex[2].z = vertex[3].z = z2;
                    break;
            }
            return this;
        }

        /// <summary>
        /// Задать смещение
        /// </summary>
        public ItemQuadSide SetTranslate(float x, float y, float z)
        {
            for (int i = 0; i < 4; i++)
            {
                vertex[i].x += x;
                vertex[i].y += y;
                vertex[i].z += z;
            }
            return this;
        }

        /// <summary>
        /// Задать смещение
        /// </summary>
        public ItemQuadSide SetTranslate(vec3 bias)
        {
            for (int i = 0; i < 4; i++)
            {
                vertex[i].x += bias.x;
                vertex[i].y += bias.y;
                vertex[i].z += bias.z;
            }
            return this;
        }

        /// <summary>
        /// Задать вращение
        /// </summary>
        public ItemQuadSide SetRotate(float yaw, float pitch = 0, float roll = 0)
        {
            vec3 vec;
            for (int i = 0; i < 4; i++)
            {
                vec = vertex[i].ToPosition() - .5f;
                if (roll != 0) vec = glm.rotate(vec, roll, new vec3(0, 0, 1));
                if (pitch != 0) vec = glm.rotate(vec, pitch, new vec3(1, 0, 0));
                if (yaw != 0) vec = glm.rotate(vec, yaw, new vec3(0, 1, 0));
                vertex[i].x = vec.x + .5f;
                vertex[i].y = vec.y + .5f;
                vertex[i].z = vec.z + .5f;
            }
            return this;
        }
    }
}
