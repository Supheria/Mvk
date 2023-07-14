using MvkServer.Entity.List;
using MvkServer.Glm;
using MvkServer.Item;
using MvkServer.Item.List;
using MvkServer.Sound;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок яблока
    /// </summary>
    public class BlockApple : BlockBase
    {
        /***
         * Met
         * 0 - Зелёное
         * 1 - Жёлтое
         * 2 - Красное
         * 3 - Гнилое
         */

        /// <summary>
        /// Блок яблока
        /// </summary>
        public BlockApple()
        {
            Material = Materials.GetMaterialCache(EnumMaterial.VegetableProtein);
            Particle = 156;
            SetUnique(true);
            IsCollidable = false;
            NeedsRandomTick = true;
            Combustibility = true;
            IgniteOddsSunbathing = 60;
            BurnOdds = 100;
            Resistance = 0;
            samplesPut = samplesBreak = new AssetsSample[] { AssetsSample.DigGrass1, AssetsSample.DigGrass2, AssetsSample.DigGrass3, AssetsSample.DigGrass4 };
            InitQuads();
        }

        /// <summary>
        /// Разрушается ли блок от жидкости
        /// </summary>
        public override bool IsLiquidDestruction() => true;

        /// <summary>
        /// Является ли блок проходимым, т.е. можно ли ходить через него
        /// </summary>
        public override bool IsPassable(int met) => true;

        /// <summary>
        /// Коробки для рендера 
        /// </summary>
        public override QuadSide[] GetQuads(int met, int xc, int zc, int xb, int zb) => quads[met];

        /// <summary>
        /// Получите предмет, который должен выпасть из этого блока при сборе.
        /// </summary>
        protected override ItemBase GetItemDropped(BlockState state, Rand rand, ItemAbTool itemTool) 
            => Items.GetItemCache(EnumItem.Apple);

        /// <summary>
        /// Смена соседнего блока
        /// </summary>
        public override void NeighborBlockChange(WorldBase worldIn, BlockPos blockPos, BlockState neighborState, BlockBase neighborBlock)
        {
            if (worldIn.GetBlockState(blockPos.OffsetUp()).GetEBlock() != EnumBlock.LeavesFruit)
            {
                if (worldIn.Rnd.Next(4) == 0)
                {
                    // 25% шанса, что дропнется яблоко при разрушении листвы
                    DropBlockAsItem(worldIn, blockPos, neighborState);
                }
                worldIn.SetBlockToAir(blockPos, 15);
            }
        }

        /// <summary>
        /// Активация блока, клик правой клавишей мыши по блоку, true - был клик, false - нет такой возможности
        /// </summary>
        public override bool OnBlockActivated(WorldBase worldIn, EntityPlayer entityPlayer, BlockPos pos, BlockState state, Pole side, vec3 facing)
        {
            worldIn.SetBlockToAir(pos);
            if (!worldIn.IsRemote)
            {
                int met = state.met;
                if (met == 1 || met == 2)
                {
                    // Берём
                    entityPlayer.Inventory.AddItemStackToInventory(worldIn, entityPlayer,
                        new ItemStack(Items.GetItemCache(EnumItem.Apple)));
                }
                // Чпок
                worldIn.PlaySoundPop(pos.ToVec3() + .5f);
            }
            return true;
        }

        public override void RandomTick(WorldBase world, BlockPos blockPos, BlockState blockState, Rand random)
        {
            if (random.Next(12) == 0)
            {
                if (world.GetTimeYear() == EnumTimeYear.Spring)
                {
                    // Весна исчезает
                    world.SetBlockToAir(blockPos, 12);
                }
                else if (world.GetTimeYear() == EnumTimeYear.Autumn && blockState.met == 0)
                {
                    // Яблочко краснеет или желтеет если оно зелёное
                    world.SetBlockStateMet(blockPos, (ushort)(random.Next(16) == 0 ? 3 : random.Next(2) + 1));
                }
                else if (world.GetTimeYear() == EnumTimeYear.Winter && blockState.met != 3)
                {
                    // Яблочко гниёт если оно не гнилое
                    world.SetBlockStateMet(blockPos, 3);
                }
            }
        }

        /// <summary>
        /// Передать список ограничительных рамок блока
        /// </summary>
        public override AxisAlignedBB[] GetCollisionBoxesToList(BlockPos pos, int met)
        {
            return new AxisAlignedBB[] { new AxisAlignedBB(
                new vec3(pos.X + .25f, pos.Y + .5f, pos.Z + .25f),
                new vec3(pos.X + .75f, pos.Y + 1, pos.Z + .75f)) };
        }

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected void InitQuads()
        {
            quads = new QuadSide[][]
            {
                new QuadSide[] {
                    new QuadSide(0).SetTexture(Particle, 0, 0, 8, 8).SetSide(Pole.South, true, 4, 8, 8, 12, 16, 8).SetRotate(glm.pi45).Wind(3),
                    new QuadSide(0).SetTexture(Particle, 0, 0, 8, 8).SetSide(Pole.East, true, 8, 8, 4, 8, 16, 12).SetRotate(glm.pi45).Wind(3)
                },
                new QuadSide[] {
                    new QuadSide(0).SetTexture(Particle, 0, 8, 8, 16).SetSide(Pole.South, true, 4, 8, 8, 12, 16, 8).SetRotate(glm.pi45).Wind(3),
                    new QuadSide(0).SetTexture(Particle, 0, 8, 8, 16).SetSide(Pole.East, true, 8, 8, 4, 8, 16, 12).SetRotate(glm.pi45).Wind(3)
                },
                new QuadSide[] {
                    new QuadSide(0).SetTexture(Particle, 8, 0, 16, 8).SetSide(Pole.South, true, 4, 8, 8, 12, 16, 8).SetRotate(glm.pi45).Wind(3),
                    new QuadSide(0).SetTexture(Particle, 8, 0, 16, 8).SetSide(Pole.East, true, 8, 8, 4, 8, 16, 12).SetRotate(glm.pi45).Wind(3)
                },
                new QuadSide[] {
                    new QuadSide(0).SetTexture(Particle, 8, 8, 16, 16).SetSide(Pole.South, true, 4, 8, 8, 12, 16, 8).SetRotate(glm.pi45).Wind(3),
                    new QuadSide(0).SetTexture(Particle, 8, 8, 16, 16).SetSide(Pole.East, true, 8, 8, 4, 8, 16, 12).SetRotate(glm.pi45).Wind(3)
                }
            };
        }
    }
}
