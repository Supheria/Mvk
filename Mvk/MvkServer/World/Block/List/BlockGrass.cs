using MvkServer.Glm;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок травы
    /// </summary>
    public class BlockGrass : BlockAbPlants
    {
        /// <summary>
        /// Блок травы
        /// </summary>
        public BlockGrass() : base(195, true)
        {
            IsReplaceable = true;
        }

        /// <summary>
        /// Передать список ограничительных рамок блока
        /// </summary>
        public override AxisAlignedBB[] GetCollisionBoxesToList(BlockPos pos, int met)
        {
            return new AxisAlignedBB[] { new AxisAlignedBB(
                new vec3(pos.X + .125f, pos.Y, pos.Z + .125f),
                new vec3(pos.X + .875f, pos.Y + .875f, pos.Z + .875f)) };
        }

        /// <summary>
        /// Спавн предмета при разрушении этого блока
        /// </summary>
        public override void DropBlockAsItemWithChance(WorldBase worldIn, BlockPos blockPos, BlockState state, float chance, int fortune) { }

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
                    new QuadSide((byte)(biomeColor ? 1 : 0)).SetTexture(Particle).SetSide(Pole.South, true, -2, 0, 8, 18, 16, 8).SetRotate(glm.pi45).SetTranslate(offsetMet[i]).Wind(),
                    new QuadSide((byte)(biomeColor ? 1 : 0)).SetTexture(Particle).SetSide(Pole.East, true, 8, 0, -2, 8, 16, 18).SetRotate(glm.pi45).SetTranslate(offsetMet[i]).Wind()
                };
            }
        }
    }
}
