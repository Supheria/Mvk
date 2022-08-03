using MvkServer.Glm;
using MvkServer.Sound;
using MvkServer.Util;
using System;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок брол, автор Вероника
    /// </summary>
    public class BlockBrol : BlockAbOre
    {
        /// <summary>
        /// Блок брол, автор Вероника
        /// </summary>
        public BlockBrol() : base(261, new vec3(1f))
        {
            LightValue = 15;
            АmbientOcclusion = false;
            Hardness = 25;
            samplesBreak = new AssetsSample[] { AssetsSample.DigGlass1, AssetsSample.DigGlass2, AssetsSample.DigGlass3 };
        }

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected override void InitBoxs()
        { 
            boxes = new Box[][] { new Box[] {
                new Box()
                {
                    Faces = new Face[]
                    {
                        new Face(Pole.Up, 259),
                        new Face(Pole.Down, 260),
                        new Face(Pole.East, 261),
                        new Face(Pole.North, 261),
                        new Face(Pole.South, 261),
                        new Face(Pole.West, 261)
                    }
                }
            }};
        }

        /// <summary>
        /// Случайный эффект частички и/или звука на блоке только для клиента
        /// </summary>
        public override void RandomDisplayTick(WorldBase world, BlockPos blockPos, BlockState blockState, Random random)
        {
            BlockState blockStateUp = world.GetBlockState(blockPos.OffsetUp());
            if (blockStateUp.GetBlock().IsAir)
            {
                world.SpawnParticle(Entity.EnumParticle.Digging, 3,
                new vec3(blockPos.X + .5f, blockPos.Y + 1.0625f, blockPos.Z + .5f),
                new vec3(1, .125f, 1), 0, (int)EBlock);
            }
        }

        /// <summary>
        /// Спавн предмета при разрушении этого блока
        /// </summary>
        public override void DropBlockAsItemWithChance(WorldBase worldIn, BlockPos blockPos, BlockState state, float chance, int fortune) { }
    }
}
