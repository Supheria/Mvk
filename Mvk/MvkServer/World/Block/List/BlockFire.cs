using MvkServer.Entity;
using MvkServer.Glm;
using MvkServer.Sound;
using MvkServer.Util;
using System;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок огня
    /// </summary>
    public class BlockFire : BlockBase
    {
        /// <summary>
        /// Блок стоячей лавы
        /// </summary>
        public BlockFire()
        {
            // Затычка, для сортировки, и прорисовки из нутри когда к примеру блок стекла
            //Translucent = true; 
           // IsAction = false;
            IsCollidable = false;
            АmbientOcclusion = false;
            NoSideDimming = true;
            Shadow = false;
            AllSideForcibly = true;
            UseNeighborBrightness = true;
            IsReplaceable = true;
           // Hardness = 0;
            LightOpacity = 0;
            LightValue = 7;
            IsParticle = false;
            Material = EnumMaterial.Fire;
            samplesStep = new AssetsSample[0];
            samplesBreak = new AssetsSample[] { AssetsSample.FireFizz };
            //samplesStep = new AssetsSample[] { AssetsSample.LiquidSwim1, AssetsSample.LiquidSwim2, AssetsSample.LiquidSwim3, AssetsSample.LiquidSwim4 };
            InitBoxs();
        }

        /// <summary>
        /// Не однотипные блоки, пример: трава, цветы, кактус
        /// </summary>
        public override bool BlocksNotSame(int met) => true;

        /// <summary>
        /// Спавн предмета при разрушении этого блока
        /// </summary>
        public override void DropBlockAsItemWithChance(WorldBase worldIn, BlockPos blockPos, BlockState state, float chance, int fortune) { }

        /// <summary>
        /// Тон сэмпла сломанного блока,
        /// </summary>
        public override float SampleBreakPitch(Random random) => 2.6f + (float)(random.NextDouble() - random.NextDouble()) * .8f;

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected void InitBoxs()
        {
            // vec3 color = new vec3(0.24f, 0.45f, 0.88f);

            boxes = new Box[][] { new Box[] {
                new Box()
                {
                    Faces = new Face[]
                    {
                        new Face(Pole.East, 57).SetAnimation(32, 1),
                        new Face(Pole.North, 57).SetAnimation(32, 1),
                        new Face(Pole.South, 57).SetAnimation(32, 1),
                        new Face(Pole.West, 57).SetAnimation(32, 1)
                    }
                },
                //new Box()
                //{
                //    From = new vec3(0, 0, 0),
                //    To = new vec3(0, 1f, 1f),
                //    Faces = new Face[]
                //    {
                //        new Face(Pole.East, 57).SetAnimation(32, 1),
                //        new Face(Pole.West, 57).SetAnimation(32, 1)
                //    }
                //},
                //new Box()
                //{
                //    From = new vec3(1f, 0, 0),
                //    To = new vec3(1f, 1f, 1f),
                //    Faces = new Face[]
                //    {
                //        new Face(Pole.East, 57).SetAnimation(32, 1),
                //        new Face(Pole.West, 57).SetAnimation(32, 1)
                //    }
                //},
                //new Box()
                //{
                //    From = new vec3(0, 0, 0),
                //    To = new vec3(1f, 1f, 0),
                //    Faces = new Face[]
                //    {
                //        new Face(Pole.North, 57).SetAnimation(32, 1),
                //        new Face(Pole.South, 57).SetAnimation(32, 1)
                //    }
                //},
                //new Box()
                //{
                //    From = new vec3(0, 0, 1f),
                //    To = new vec3(1f, 1f, 1f),
                //    //RotatePitch = glm.pi45,
                //    Faces = new Face[]
                //    {
                //        new Face(Pole.North, 57).SetAnimation(32, 1),
                //        new Face(Pole.South, 57).SetAnimation(32, 1)
                //    }
                //},
                new Box()
                {
                    From = new vec3(0, -.0625f, .5f),
                    To = new vec3(1f, 1.25f, .5f),
                    RotatePitch = .4f,
                    Faces = new Face[]
                    {
                        new Face(Pole.North, 57).SetAnimation(32, 1),
                        new Face(Pole.South, 57).SetAnimation(32, 1)
                    }
                },
                new Box()
                {
                    From = new vec3(0, -.0625f, .5f),
                    To = new vec3(1f, 1.25f, .5f),
                    RotatePitch = -.4f,
                    Faces = new Face[]
                    {
                        new Face(Pole.North, 57).SetAnimation(32, 1),
                        new Face(Pole.South, 57).SetAnimation(32, 1)
                    }
                },
                new Box()
                {
                    From = new vec3(0, -.0625f, .5f),
                    To = new vec3(1f, 1.25f, .5f),
                    RotatePitch = .4f,
                    RotateYaw = glm.pi90,
                    Faces = new Face[]
                    {
                        new Face(Pole.North, 57).SetAnimation(32, 1),
                        new Face(Pole.South, 57).SetAnimation(32, 1)
                    }
                },
                new Box()
                {
                    From = new vec3(0, -.0625f, .5f),
                    To = new vec3(1f, 1.25f, .5f),
                    RotatePitch = -.4f,
                    RotateYaw = glm.pi90,
                    Faces = new Face[]
                    {
                        new Face(Pole.North, 57).SetAnimation(32, 1),
                        new Face(Pole.South, 57).SetAnimation(32, 1)
                    }
                }
                
                //new Box()
                //{
                //    From = new vec3(-.2f, 0, .5f),
                //    To = new vec3(1.2f, 1f, .5f),
                //    RotateYaw = glm.pi45,
                //    Faces = new Face[]
                //    {
                //        new Face(Pole.North, 2105).SetAnimation(32, 1),
                //        new Face(Pole.South, 2105).SetAnimation(32, 1),
                //    }
                //},
                //new Box()
                //{
                //    From = new vec3(.5f, 0, -.2f),
                //    To = new vec3(.5f, 1f, 1.2f),
                //    RotateYaw = glm.pi45,
                //    Faces = new Face[]
                //    {
                //        new Face(Pole.East, 2105).SetAnimation(32, 1),
                //        new Face(Pole.West, 2105).SetAnimation(32, 1),
                //    }
                //}
            }};
        }

        /// <summary>
        /// Случайный эффект частички и/или звука на блоке только для клиента
        /// </summary>
        public override void RandomDisplayTick(WorldBase world, BlockPos blockPos, BlockState blockState, Random random)
        {
            if (random.Next(24) == 0)
            {
                world.PlaySound(AssetsSample.Fire, blockPos.ToVec3() + .5f, 1f + (float)random.NextDouble(), (float)random.NextDouble() * .7f + .3f);
            }
            if (world.DoesBlockHaveSolidTopSurface(blockPos.OffsetDown()))
            {
                world.SpawnParticle(EnumParticle.Smoke, 2,
                    new vec3(blockPos.X, blockPos.Y + .5f, blockPos.Z + .5f), new vec3(.125f, 1f, 1f), 0, 40);
                world.SpawnParticle(EnumParticle.Smoke, 2,
                    new vec3(blockPos.X + 1f, blockPos.Y + .5f, blockPos.Z + .5f), new vec3(.125f, 1f, 1f), 0, 40);
                world.SpawnParticle(EnumParticle.Smoke, 2,
                    new vec3(blockPos.X + .5f, blockPos.Y + .5f, blockPos.Z), new vec3(1f, 1f, .125f), 0, 40);
                world.SpawnParticle(EnumParticle.Smoke, 2,
                    new vec3(blockPos.X + .5f, blockPos.Y + .5f, blockPos.Z + 1f), new vec3(1f, 1f, .125f), 0, 40);
                world.SpawnParticle(EnumParticle.Smoke, 2,
                    new vec3(blockPos.X + .5f, blockPos.Y + .75f, blockPos.Z + .5f), new vec3(1f, .5f, 1f), 0, 40);
            }
            else
            {
                world.SpawnParticle(EnumParticle.Smoke, 3,
                    new vec3(blockPos.X + .5f, blockPos.Y + .5f, blockPos.Z + .5f), new vec3(1f), 0, 40);
            }
        }
    }
}
