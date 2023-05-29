using MvkServer.Entity.List;
using MvkServer.Glm;
using MvkServer.Item;
using MvkServer.Sound;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Универсальный объект листвы
    /// </summary>
    public class BlockUniLeaves : BlockBase
    {
        /***
         * Met
         * 0 - листва
         * 1 - листва с плодом
         */

        /// <summary>
        /// Может ли у этой листвы быть плод
        /// </summary>
        private readonly bool fetus;

        public BlockUniLeaves(int numberTexture, bool fetus = false)
        {
            this.fetus = fetus;
            Material = EnumMaterial.Leaves;
            IsCollidable = false;
            NeedsRandomTick = fetus;
            SetUnique();
            LightOpacity = 2;
            BiomeColor = true;
            Color = new vec3(.56f, .73f, .35f);
            Particle = numberTexture;
            Combustibility = true;
            IgniteOddsSunbathing = 30;
            BurnOdds = 60;
            Resistance = .2f;
            samplesPut = samplesBreak = new AssetsSample[] { AssetsSample.DigGrass1, AssetsSample.DigGrass2, AssetsSample.DigGrass3, AssetsSample.DigGrass4 };
            InitBoxs();
        }

        /// <summary>
        /// Возвращает количество предметов, которые выпадают при разрушении блока.
        /// </summary>
        public override int QuantityDropped(Rand random) => 0;

        /// <summary>
        /// Коробки для рендера 
        /// </summary>
        public override Box[] GetBoxes(int met, int xc, int zc, int xb, int zb)
        {
            if (fetus)
            {
                return boxes[met];
            }

            return boxes[0];
            //return boxes[(xc + zc + xb + zb) & 4];
        }

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected void InitBoxs()
        {
            if (fetus)
            {
                boxes = new Box[][] {
                    new Box[] {
                        new Box(Particle, true, Color, true)
                    },
                    new Box[] {
                        new Box(Particle, true, Color, true),
                        new Box()
                        {
                            From = new vec3(0, 0, .5f),
                            To = new vec3(1, 1, .5f),
                            RotateYaw = glm.pi45,
                            Faces = new Face[]
                            {
                                new Face(Pole.North, Particle + 2),
                                new Face(Pole.South, Particle + 2),
                            }
                        },
                        new Box()
                        {
                            From = new vec3(.5f, 0, 0),
                            To = new vec3(.5f, 1, 1),
                            RotateYaw = glm.pi45,
                            Faces = new Face[]
                            {
                                new Face(Pole.East, Particle + 2),
                                new Face(Pole.West, Particle + 2)
                            }
                        }
                    }
                };
            }
            else
            {
                boxes = new Box[][] {
                    new Box[] {
                        new Box(Particle, true, Color, true)
                    }
                };

                //vec3[] offsetMet = new vec3[]
                //{
                //    new vec3(0, .7f, 0),
                //    new vec3(.25f, .8f, .25f),
                //    new vec3(-.25f, -.8f, .25f),
                //    new vec3(.25f, .9f, -.25f),
                //    new vec3(-.25f, -.9f, -.25f)
                //};

                //boxes = new Box[5][];
                //for (int i = 0; i < 5; i++)
                //{
                //    boxes[i] = new Box[] 
                //    {
                //        new Box()
                //        {
                //            From = new vec3(-.5f, -.5f, .5f),
                //            To = new vec3(1.5f, 1.5f, .5f),
                //            UVTo = new vec2(MvkStatic.Uv[16] * 2, MvkStatic.Uv[16]),
                //            RotateYaw = offsetMet[i].y,
                //            RotatePitch = .5f,
                //            Faces = new Face[]
                //            {
                //                new Face(Pole.North, Particle, true, Color).SetBiomeColor(),
                //                new Face(Pole.South, Particle, true, Color).SetBiomeColor()
                //            }
                //        },
                //        new Box()
                //        {
                //            From = new vec3(-.5f, -.5f, .5f),
                //            To = new vec3(1.5f, 1.5f, .5f),
                //            RotateYaw = -offsetMet[i].y,
                //            RotatePitch = .5f,
                //            Faces = new Face[]
                //            {
                //                new Face(Pole.North, Particle, true, Color).SetBiomeColor(),
                //                new Face(Pole.South, Particle, true, Color).SetBiomeColor()
                //            }
                //        },
                //        new Box()
                //        {
                //            From = new vec3(-.5f, .5f, -.5f),
                //            To = new vec3(1.5f, .5f, 1.5f),
                //            RotateYaw = offsetMet[i].y,
                //            RotatePitch = .3f,
                //            Faces = new Face[]
                //            {
                //                new Face(Pole.Down, Particle, true, Color).SetBiomeColor(),
                //                new Face(Pole.Up, Particle, true, Color).SetBiomeColor()
                //            }
                //        },
                //        //new Box()
                //        //{
                //        //    From = new vec3(.5f, -.4f, -.4f),
                //        //    To = new vec3(.5f, 1.4f, 1.4f),
                //        //    RotateYaw = glm.pi45,
                //        //    RotatePitch = glm.pi45,
                //        //    Faces = new Face[]
                //        //    {
                //        //        new Face(Pole.East, Particle, true, Color).SetBiomeColor(),
                //        //        new Face(Pole.West, Particle, true, Color).SetBiomeColor()
                //        //    }
                //        //}


                //        //    new Box(Particle, true, Color, true)
                //        //    {
                //        //        From = new vec3(-.2f),
                //        //        To = new vec3(1.2f)
                //        //    }
                //    };
                //    //boxes[i][0].Translate = offsetMet[i];
                //    //boxes[i][1].Translate = offsetMet[i];
                //}
            }
        }

        /// <summary>
        /// Является ли блок проходимым, т.е. можно ли ходить через него
        /// </summary>
        public override bool IsPassable(int met) => true;

        public override void RandomTick(WorldBase world, BlockPos blockPos, BlockState blockState, Rand random)
        {
            if (fetus)
            {
                if (world.GetMoonPhase() == EnumMoonPhase.NewMoon && random.Next(16) == 0)
                {
                    if (EBlock == EnumBlock.LeavesFruit && world.GetTimeYear() == EnumTimeYear.Autumn)
                    {
                        // Яблочки растут только осенью в полнолунию
                        bool isAir = false;
                        bool isFetus = false;
                        BlockState blockState2;
                        for (int i = 1; i < 6; i++)
                        {
                            blockState2 = world.GetBlockState(blockPos.Offset(i));
                            if (blockState2.IsAir()) isAir = true;
                            if (blockState2.GetEBlock() == EBlock && blockState2.met == 1) isFetus = true;
                        }
                        if (isAir && !isFetus)
                        {
                            // появляется плод
                            world.SetBlockStateMet(blockPos, 1);
                        }
                    }
                    else if (EBlock == EnumBlock.LeavesPalm)
                    {
                        // Кокосы растут в полнолунию
                        BlockPos blockPosDown = blockPos.OffsetDown();
                        if (world.GetBlockState(blockPosDown).IsAir())
                        {
                            for (int i = 2; i < 6; i++)
                            {
                                if (world.GetBlockState(blockPosDown.Offset(i)).GetEBlock() == EnumBlock.LogPalm)
                                {
                                    // появляется кокос
                                    world.SetBlockState(blockPosDown, new BlockState(EnumBlock.Coconut), 12);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Активация блока, клик правой клавишей мыши по блоку, true - был клик, false - нет такой возможности
        /// </summary>
        public override bool OnBlockActivated(WorldBase worldIn, EntityPlayer entityPlayer, BlockPos pos, BlockState state, Pole side, vec3 facing)
        {
            if (!fetus || state.met == 0) return false;
            
            if (!worldIn.IsRemote)
            {
                worldIn.SetBlockStateMet(pos, 0);
                // Берём
                entityPlayer.Inventory.AddItemStackToInventory(worldIn, entityPlayer,
                    new ItemStack(Items.GetItemCache(EnumItem.Apple)));
                // Чпок
                worldIn.PlaySoundPop(pos.ToVec3() + .5f);
            }
            return true;
        }
    }
}
