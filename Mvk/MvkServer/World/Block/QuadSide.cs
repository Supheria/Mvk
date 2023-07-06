using MvkServer.Glm;
using MvkServer.Util;

namespace MvkServer.World.Block
{
    /// <summary>
    /// Прямоугольная сторона блока или элемента блока
    /// </summary>
    public struct QuadSide
    {
        /// <summary>
        /// Четыре вершины
        /// </summary>
        public readonly Vertex[] vertex;
        /// <summary>
        /// С какой стороны
        /// </summary>
        public int side;
        /// <summary>
        /// Боковое затемнение
        /// </summary>
        public float lightPole;
        /// <summary>
        /// Для анимации блока, указывается количество кадров в игровом времени (50 мс),
        /// можно кратно 2 в степени (2, 4, 8, 16, 32, 64...)
        /// 0 - нет анимации
        /// </summary>
        public byte animationFrame;
        /// <summary>
        /// Для анимации блока, указывается пауза между кадрами в игровом времени (50 мс),
        /// можно кратно 2 в степени (2, 4, 8, 16, 32, 64...)
        /// 0 или 1 - нет задержки, каждый такт игры смена кадра
        /// </summary>
        public byte animationPause;
        /// <summary>
        /// Флаг имеется ли ветер, для уникальных блоков: листвы, травы и подобного
        /// 0 - нет, 1 - вверхние точки, 2 - нижние
        /// </summary>
        public byte wind;

        /// <summary>
        /// Цвет биома, где 0 - нет цвета, 1 - трава, 2 - листа, 3 - вода
        /// </summary>
        private readonly byte biomeColor;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="biomeColor">Задать цвет биома, где 0 - нет цвета, 1 - трава, 2 - листа, 3 - вода, 4 - свой цвет</param>
        /// <param name="frame">количество кадров в игровом времени</param>
        /// <param name="pause">пауза между кадрами в игровом времени</param>
        public QuadSide(byte biomeColor, byte frame = 0, byte pause = 0)
        {
            vertex = new Vertex[4];
            this.biomeColor = biomeColor;
            animationFrame = frame;
            animationPause = pause;
            lightPole = 0;
            side = 0;
            wind = 0;
        }

        /// <summary>
        /// Задаём сторону и размеры на стороне
        /// </summary>
        /// <param name="pole">индекс стороны</param>
        /// <param name="noSideDimming">Нет бокового затемнения, пример: трава, цветы</param>
        public QuadSide SetSide(Pole pole, bool noSideDimming = false, int x1i = 0, int y1i = 0, int z1i = 0, int x2i = 16, int y2i = 16, int z2i = 16)
        {
            float x1 = x1i / 16f;
            float y1 = y1i / 16f;
            float z1 = z1i / 16f;
            float x2 = x2i / 16f;
            float y2 = y2i / 16f;
            float z2 = z2i / 16f;
            side = (int)pole;
            lightPole = noSideDimming ? 0f : 1f - MvkStatic.LightPoles[side];
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
        /// Задать текстуру, её можно повернуть кратно 90 гр. 0 - 3 => 0 - 270
        /// </summary>
        public QuadSide SetTexture(int numberTexture, int rotateYawUV = 0)
        {
            float u1 = (numberTexture % 64) * .015625f;
            float u2 = u1 + .015625f;
            float v1 = numberTexture / 64 * .015625f;
            float v2 = v1 + .015625f;
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
            return this;
        }

        /// <summary>
        /// Задать текстуру, и задать ей размеры не полного сэмпла и можно повернуть кратно 90 гр. 0 - 3 => 0 - 270
        /// </summary>
        public QuadSide SetTexture(int numberTexture, int biasU1, int biasV1, int biasU2, int biasV2, int rotateYawUV = 0)
        {
            float u = (numberTexture % 64) * .015625f;
            float v = numberTexture / 64 * .015625f;

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
            return this;
        }

        /// <summary>
        /// Задать смещение
        /// </summary>
        public QuadSide SetTranslate(float x, float y, float z)
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
        public QuadSide SetTranslate(vec3 bias)
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
        public QuadSide SetRotate(float yaw, float pitch = 0, float roll = 0)
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

        /// <summary>
        /// Задать стороне ветер, 0 - нет движения 1 - (по умолчанию) вверх двигается низ нет, 2 - низ двигается вверх нет, 3 - двигается всё
        /// </summary>
        public QuadSide Wind(byte w = 1)
        {
            wind = w;
            return this;
        }

        /// <summary>
        /// Имеется ли у блока смена цвета от биома
        /// </summary>
        public bool IsBiomeColor() => biomeColor > 0;
        /// <summary>
        /// Имеется ли у блока смена цвета травы от биома
        /// </summary>
        public bool IsBiomeColorGrass() => biomeColor == 1;
        /// <summary>
        /// Имеется ли у блока свой цвет, цвет как для GUI
        /// </summary>
        public bool IsYourColor() => biomeColor == 4;
    }

    /// <summary>
    /// Тип цаета биома
    /// </summary>
    public enum TypeBiomeColor : byte
    {
        /// <summary>
        /// Нет подкраски от биома
        /// </summary>
        None = 0,
        /// <summary>
        /// Подкраска травы
        /// </summary>
        Grass = 1,
        /// <summary>
        /// Подкраска воды
        /// </summary>
        Water = 3,
        /// <summary>
        /// Свой цвет
        /// </summary>
        Your = 4
    }
}
