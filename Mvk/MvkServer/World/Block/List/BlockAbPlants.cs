using MvkServer.Glm;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Абстрактный объект растений с случайным смещением, для травы и цветов
    /// </summary>
    public abstract class BlockAbPlants : BlockAbSapling
    {
        public BlockAbPlants(int numberTexture) : base(numberTexture) { }
        public BlockAbPlants(int numberTexture, vec3 color) : base(numberTexture, color) { }

        /// <summary>
        /// Коробки
        /// </summary>
        public override Box[] GetBoxes(int met, int xc, int zc, int xb, int zb) => boxes[(xc + zc + xb + zb) & 4];

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected override void InitBoxs()
        {
            vec3[] offsetMet = new vec3[]
            {
                new vec3(0),
                new vec3(.1875f, 0, .1875f),
                new vec3(-.1875f, 0, .1875f),
                new vec3(.1875f, 0, -.1875f),
                new vec3(-.1875f, 0, -.1875f)
            };

            boxes = new Box[5][];
            for (int i = 0; i < 5; i++)
            {
                boxes[i] =
                    new Box[] {
                    new Box()
                    {
                        From = new vec3(0, 0, .5f),
                        To = new vec3(1f, 1f, .5f),
                        RotateYaw = glm.pi45,
                        Faces = new Face[]
                        {
                            new Face(Pole.North, Particle, true, Color),
                            new Face(Pole.South, Particle, true, Color),
                        }
                    },
                    new Box()
                    {
                        From = new vec3(.5f, 0, 0),
                        To = new vec3(.5f, 1f, 1f),
                        RotateYaw = glm.pi45,
                        Faces = new Face[]
                        {
                            new Face(Pole.East, Particle, true, Color),
                            new Face(Pole.West, Particle, true, Color)
                        }
                    }
                };
                boxes[i][0].Translate = offsetMet[i];
                boxes[i][1].Translate = offsetMet[i];
            }
        }
    }
}
