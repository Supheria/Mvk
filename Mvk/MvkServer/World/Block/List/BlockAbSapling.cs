﻿using MvkServer.Entity.List;
using MvkServer.Glm;
using MvkServer.Sound;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Абстрактный объект саженцев и прочих растений
    /// </summary>
    public abstract class BlockAbSapling : BlockBase
    {
        protected bool biomeColor;

        public BlockAbSapling(int numberTexture, bool biomeColor = false)
        {
            Material = EnumMaterial.Sapling;
            Particle = numberTexture;
            SetUnique(true);
            IsCollidable = false;
            Combustibility = true;
            this.biomeColor = biomeColor;
            IgniteOddsSunbathing = 60;
            BurnOdds = 100;
            Resistance = 0f;
            samplesPut = samplesBreak = new AssetsSample[] { AssetsSample.DigGrass1, AssetsSample.DigGrass2, AssetsSample.DigGrass3, AssetsSample.DigGrass4 };
            InitQuads();
        }

        /// <summary>
        /// Разрушается ли блок от жидкости
        /// </summary>
        public override bool IsLiquidDestruction() => true;

        /// <summary>
        /// Смена соседнего блока
        /// </summary>
        public override void NeighborBlockChange(WorldBase worldIn, BlockPos blockPos, BlockState neighborState, BlockBase neighborBlock)
        {
            if (!CanBlockStay(worldIn, blockPos))
            {
                DropBlockAsItem(worldIn, blockPos, neighborState, 0);
                worldIn.SetBlockToAir(blockPos);
            }
        }

        public override bool CanBlockStay(WorldBase worldIn, BlockPos blockPos, int met = 0)
        {
            EnumBlock enumBlock = worldIn.GetBlockState(blockPos.OffsetDown()).GetEBlock();
            return /*enumBlock == EnumBlock.Dirt ||*/ enumBlock == EnumBlock.Turf;
        }

        /// <summary>
        /// Передать список ограничительных рамок блока
        /// </summary>
        public override AxisAlignedBB[] GetCollisionBoxesToList(BlockPos pos, int met)
        {
            return new AxisAlignedBB[] { new AxisAlignedBB(
                new vec3(pos.X + .25f, pos.Y, pos.Z + .25f),
                new vec3(pos.X + .75f, pos.Y + .875f, pos.Z + .75f)) };
        }

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected virtual void InitQuads()
        {
            quads = new QuadSide[][] { new QuadSide[] {
                new QuadSide(0).SetTexture(Particle).SetSide(Pole.South, true, 0, 0, 8, 16, 16, 8).SetRotate(glm.pi45).Wind(),
                new QuadSide(0).SetTexture(Particle).SetSide(Pole.East, true, 8, 0, 0, 8, 16, 16).SetRotate(glm.pi45).Wind()
            } };
        }

        /// <summary>
        /// Обновить блок в такте
        /// </summary>
        public override void UpdateTick(WorldBase world, BlockPos blockPos, BlockState blockState, Rand random)
        {
            if (blockState.lightSky > 9 && random.Next(14) == 0 && world.IsAreaLoaded(blockPos, 6))
            {
                GenefateTree(world, random, blockPos);
            }
        }

        /// <summary>
        /// Генерация дерева
        /// </summary>
        protected virtual void GenefateTree(WorldBase world, Rand rand, BlockPos blockPos) { }

        /// <summary>
        /// Является ли блок проходимым, т.е. можно ли ходить через него
        /// </summary>
        public override bool IsPassable(int met) => true;

        // TODO::2023-05-18 Вырастает дерево по нажатию правой мыши, УБРАТЬ!
        /// <summary>
        /// Активация блока, клик правой клавишей мыши по блоку, true - был клик, false - нет такой возможности
        /// </summary>
        public override bool OnBlockActivated(WorldBase worldIn, EntityPlayer entityPlayer, BlockPos pos, BlockState state, Pole side, vec3 facing)
        {
            if (!worldIn.IsRemote) GenefateTree(worldIn, worldIn.Rnd, pos);
            return true;
        }
    }
}
