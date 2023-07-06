using MvkServer.Glm;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Абстрактный объект растений с случайным смещением, для травы и цветов
    /// </summary>
    public abstract class BlockAbPlants : BlockAbSapling
    {
        public BlockAbPlants(int numberTexture, bool biomeColor = false) : base(numberTexture, biomeColor) { }

        /// <summary>
        /// Стороны целого блока для рендера 0 - 5 стороны
        /// </summary>
        public override QuadSide[] GetQuads(int met, int xc, int zc, int xb, int zb) => quads[(xc + zc + xb + zb) & 4];

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected override void InitQuads()
        {
            vec3[] offsetMet = new vec3[]
            {
                new vec3(0),
                new vec3(.1875f, 0, .1875f),
                new vec3(-.1875f, 0, .1875f),
                new vec3(.1875f, 0, -.1875f),
                new vec3(-.1875f, 0, -.1875f)
            };

            quads = new QuadSide[5][];
            for (int i = 0; i < 5; i++)
            {
                quads[i] = new QuadSide[] {
                    new QuadSide((byte)(biomeColor ? 1 : 0)).SetTexture(Particle).SetSide(Pole.South, true, 0, 0, 8, 16, 16, 8).SetRotate(glm.pi45).SetTranslate(offsetMet[i]).Wind(),
                    new QuadSide((byte)(biomeColor ? 1 : 0)).SetTexture(Particle).SetSide(Pole.East, true, 8, 0, 0, 8, 16, 16).SetRotate(glm.pi45).SetTranslate(offsetMet[i]).Wind()
                };
            }
        }
    }
}
