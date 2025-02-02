﻿using MvkServer.Entity;
using MvkServer.Glm;
using MvkServer.Sound;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок кактуса
    /// </summary>
    public class BlockCactus : BlockBase
    {
        /***
         * Met
         * 0 - 15 age
         */ 
        /// <summary>
        /// Блок кактуса
        /// </summary>
        public BlockCactus()
        {
            NeedsRandomTick = true;
            SetUnique();
            Material = Materials.GetMaterialCache(EnumMaterial.VegetableProtein);
            Particle = 193;
            samplesPut = samplesBreak = new AssetsSample[] { AssetsSample.DigGrass1, AssetsSample.DigGrass2, AssetsSample.DigGrass3, AssetsSample.DigGrass4 };
            samplesStep = new AssetsSample[] { AssetsSample.StepGrass1, AssetsSample.StepGrass2, AssetsSample.StepGrass3, AssetsSample.StepGrass4 };
            
            quads = new QuadSide[][] { new QuadSide[] {
                new QuadSide(0).SetTexture(194).SetSide(Pole.Up),
                new QuadSide(0).SetTexture(192).SetSide(Pole.Down),
                new QuadSide(0).SetTexture(193).SetSide(Pole.East, false, 1, 0, 0, 15, 16, 16),
                new QuadSide(0).SetTexture(193).SetSide(Pole.West, false, 1, 0, 0, 15, 16, 16),
                new QuadSide(0).SetTexture(193).SetSide(Pole.North, false, 0, 0, 1, 16, 16, 15),
                new QuadSide(0).SetTexture(193).SetSide(Pole.South, false, 0, 0, 1, 16, 16, 15)
            } };
        }

        /// <summary>
        /// Разрушается ли блок от жидкости
        /// </summary>
        public override bool IsLiquidDestruction() => true;

        /// <summary>
        /// Сколько ударов требуется, чтобы сломать блок в тактах (20 тактов = 1 секунда)
        /// </summary>
        public override int Hardness(BlockState state) => 20;

        /// <summary>
        /// Смена соседнего блока
        /// </summary>
        public override void NeighborBlockChange(WorldBase worldIn, BlockPos blockPos, BlockState neighborState, BlockBase neighborBlock)
        {
            if (!CanBlockStay(worldIn, blockPos))
            {
                DropBlockAsItem(worldIn, blockPos, neighborState);
                worldIn.SetBlockToAir(blockPos, 30);
            }
        }

        ///// <summary>
        ///// Установить блок
        ///// </summary>
        ///// <param name="side">Сторона на какой ставим блок</param>
        ///// <param name="facing">Значение в пределах 0..1, образно фиксируем пиксел клика на стороне</param>
        //public override bool Put(WorldBase worldIn, BlockPos blockPos, BlockState state, Pole side, vec3 facing)
        //{
        //    if (CheckPut(worldIn, blockPos)) return base.Put(worldIn, blockPos, state, side, facing);
        //    return false;
        //}

        /// <summary>
        /// Проверка установи блока, можно ли его установить тут
        /// </summary>
        public override bool CanBlockStay(WorldBase worldIn, BlockPos blockPos, int met = 0)
        {
            EnumBlock enumBlock = worldIn.GetBlockState(blockPos.OffsetDown()).GetEBlock();
            return (enumBlock == EnumBlock.Sand || enumBlock == EnumBlock.Cactus)
                && worldIn.GetBlockState(blockPos.OffsetEast()).GetEBlock() == EnumBlock.Air
                && worldIn.GetBlockState(blockPos.OffsetNorth()).GetEBlock() == EnumBlock.Air
                && worldIn.GetBlockState(blockPos.OffsetSouth()).GetEBlock() == EnumBlock.Air
                && worldIn.GetBlockState(blockPos.OffsetWest()).GetEBlock() == EnumBlock.Air;
        }

        /// <summary>
        /// Передать список  ограничительных рамок блока
        /// </summary>
        public override AxisAlignedBB[] GetCollisionBoxesToList(BlockPos pos, int met)
        {
            return new AxisAlignedBB[] { new AxisAlignedBB(
                new vec3(pos.X + .0625f, pos.Y, pos.Z + .0625f),
                new vec3(pos.X + .9375f, pos.Y + 1f, pos.Z + .9375f)) };
        }

        /// <summary>
        /// Случайный эффект блока, для сервера
        /// </summary>
        public override void RandomTick(WorldBase world, BlockPos blockPos, BlockState blockState, Rand random)
        {
            BlockPos blockPosUp = blockPos.OffsetUp();

            if (world.IsAirBlock(blockPosUp))
            {
                int i;

                for (i = 1; world.GetBlockState(blockPos.Offset(0, -i, 0)).GetEBlock() == EnumBlock.Cactus; ++i) ;

                if (i < 7)
                {
                    int age = blockState.met;

                    if (age == 15)
                    {
                        BlockState blockStateNew = blockState.NewMet(0);
                        world.SetBlockState(blockPos, blockStateNew, 8);
                        world.SetBlockState(blockPosUp, blockStateNew, 14);
                        NeighborBlockChange(world, blockPosUp, blockStateNew, this);
                    }
                    else
                    {
                        world.SetBlockState(blockPos, blockState.NewMet((byte)(age + 1)), 8);
                    }
                }
            }
        }

        /// <summary>
        /// Вызывается при столкновении объекта с блоком
        /// </summary>
        public override void OnEntityCollidedWithBlock(WorldBase worldIn, BlockPos pos, BlockState state, EntityBase entityIn)
        {
            entityIn.AttackEntityFrom(EnumDamageSource.Cactus, 1.0F);
        }
    }
}
