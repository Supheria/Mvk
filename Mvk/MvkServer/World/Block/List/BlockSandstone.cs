using MvkServer.Glm;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок Песчаник
    /// </summary>
    public class BlockSandstone : BlockUniSolid
    {
        /// <summary>
        /// Блок Песчаник
        /// </summary>
        public BlockSandstone() : base(7, new vec3(.95f, .91f, .73f), 15) { }

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
                        new Face(Pole.Up, 6, color),
                        new Face(Pole.Down, 8, color),
                        new Face(Pole.East, 7, color),
                        new Face(Pole.North, 7, color),
                        new Face(Pole.South, 7, color),
                        new Face(Pole.West, 7, color)
                    }
                }
            }};
        }
    }
}
