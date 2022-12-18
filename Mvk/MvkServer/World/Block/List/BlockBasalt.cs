using MvkServer.Glm;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок Базальт
    /// </summary>
    public class BlockBasalt : BlockUniSolid
    {
        /// <summary>
        /// Блок Базальт
        /// </summary>
        public BlockBasalt() : base(10, new vec3(.7f), 15) { }

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected override void InitBoxs(int numberTexture, bool isColor, vec3 color) => InitBoxs(color);

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        private void InitBoxs(vec3 color)
        {
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
