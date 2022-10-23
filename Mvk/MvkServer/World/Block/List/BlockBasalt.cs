using MvkServer.Glm;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок Базальт
    /// </summary>
    public class BlockBasalt : BlockBase
    {
        /// <summary>
        /// Блок Базальт
        /// </summary>
        public BlockBasalt()
        {
            Particle = 10;
            Material = EnumMaterial.Solid;
            InitBoxs();
        }

        /// <summary>
        /// Сколько ударов требуется, чтобы сломать блок в тактах (20 тактов = 1 секунда)
        /// </summary>
        public override int Hardness(BlockState state) => 15;

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        private void InitBoxs()
        {
            vec3 color = new vec3(.7f);
            boxes = new Box[][] { new Box[] {
                new Box()
                {
                    Faces = new Face[]
                    {
                        new Face(Pole.Up, 10, color),
                        new Face(Pole.Down, 10, color),
                        new Face(Pole.East, 11, color),
                        new Face(Pole.North, 11, color),
                        new Face(Pole.South, 11, color),
                        new Face(Pole.West, 11, color)
                    }
                }
            }};
        }
    }
}
