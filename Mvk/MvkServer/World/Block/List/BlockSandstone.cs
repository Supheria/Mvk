using MvkServer.Glm;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок Песчаник
    /// </summary>
    public class BlockSandstone : BlockBase
    {
        /// <summary>
        /// Блок Песчаник
        /// </summary>
        public BlockSandstone()
        {
            Particle = 7;
            Hardness = 15;
            Material = EnumMaterial.Solid;
            InitBoxs();
        }

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        private void InitBoxs()
        {
            vec3 color = new vec3(.95f, .91f, .73f);
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
