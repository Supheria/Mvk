using MvkServer.Entity;
using MvkServer.Glm;
using MvkServer.Sound;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Абстрактный объект жидких блоков
    /// </summary>
    public class BlockAbLiquid : BlockBase
    {
        /// <summary>
        /// Материал жидкости
        /// </summary>
        protected EnumMaterial material = EnumMaterial.Water;
        /// <summary>
        /// Блок стоячей жидкости
        /// </summary>
        protected EnumBlock eBlock = EnumBlock.Water;
        /// <summary>
        /// Блок тикучей жидкости
        /// </summary>
        protected EnumBlock eBlockFlowing = EnumBlock.WaterFlowing;
        /// <summary>
        /// Сколько игровых тиков между тиками
        /// 5 вода, 15 нефть, 30 лава
        /// </summary>
        protected uint tickRate = 5;
        /// <summary>
        /// Шаг волны для дистанции растекания, 15 / step = максимальная длинна
        /// 1 = 14; 2 = 7; вода 3 = 4; нефть 4 = 3; лава
        /// </summary>
        protected int stepWave = 2;
        
        public BlockAbLiquid()
        {
            FullBlock = false;
            Liquid = true;

            BlocksNotSame = false;
            UseNeighborBrightness = true;
            AllSideForcibly = true;
            IsAction = false;
            IsCollidable = false;
            АmbientOcclusion = false;
            Shadow = false;
            IsReplaceable = true;
            IsParticle = false;
            Resistance = 100f;
        }

        /// <summary>
        /// Действие блока после его установки
        /// </summary>
        public override void OnBlockAdded(WorldBase worldIn, BlockPos blockPos, BlockState state)
        {
            if (!Mixing(worldIn, blockPos, state))
            {
                worldIn.SetBlockTick(blockPos, tickRate);
            }
        }

        /// <summary>
        /// Смена соседнего блока
        /// </summary>
        public override void NeighborBlockChange(WorldBase worldIn, BlockPos blockPos, BlockState state, BlockBase neighborBlock)
        {
            if (!Mixing(worldIn, blockPos, state))
            {
                worldIn.SetBlockTick(blockPos, tickRate);
            }
        }

        /// <summary>
        /// Смешивание блоков
        /// </summary>
        protected virtual bool Mixing(WorldBase world, BlockPos blockPos, BlockState state)
        {
            EnumMaterial mBlock = state.GetBlock().Material;

            if (mBlock == EnumMaterial.Oil)
            {
                if (world.GetBlockState(blockPos.OffsetDown()).GetBlock().Material == EnumMaterial.Lava)
                {
                    world.SetBlockState(blockPos, new BlockState(EnumBlock.Fire), 14);
                    EffectMixingOil(world, blockPos);
                    return true;
                }
            }
            else if (mBlock == EnumMaterial.Lava)
            {
                bool water = false;
                bool oil = false;
                EnumMaterial eMaterial;
                for (int i = 0; i < 6; i++)
                {
                    if (i != 1)
                    {
                        eMaterial = world.GetBlockState(blockPos.Offset(i)).GetBlock().Material;
                        if (eMaterial == EnumMaterial.Water)
                        {
                            water = true;
                            break;
                        }
                        if (eMaterial == EnumMaterial.Oil)
                        {
                            oil = true;
                            break;
                        }
                    }
                }

                if (oil)
                {
                    if (state.GetEBlock() == EnumBlock.LavaFlowing)
                    {
                        world.SetBlockState(blockPos, new BlockState(EnumBlock.Fire), 14);
                        EffectMixingOil(world, blockPos);
                        return true;
                    }
                }
                if (water)
                {
                    EnumBlock eBlock = state.GetEBlock();

                    if (eBlock == EnumBlock.Lava)
                    {
                        world.SetBlockState(blockPos, new BlockState(EnumBlock.Obsidian), 14);
                        EffectMixingWater(world, blockPos);
                        return true;
                    }

                    if (eBlock == EnumBlock.LavaFlowing)
                    {
                        world.SetBlockState(blockPos, new BlockState(EnumBlock.Basalt), 14);
                        EffectMixingWater(world, blockPos);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Спавн предмета при разрушении этого блока
        /// </summary>
        public override void DropBlockAsItemWithChance(WorldBase worldIn, BlockPos blockPos, BlockState state, float chance, int fortune) { }


        /// <summary>
        /// Случайный эффект частички и/или звука на блоке только для клиента
        /// </summary>
        public override void RandomDisplayTick(WorldBase world, BlockPos blockPos, BlockState blockState, Rand random)
        {
            if (material == EnumMaterial.Water)
            {
                if (EBlock == EnumBlock.WaterFlowing)
                {
                    if (random.Next(64) == 0)
                    {
                        vec3 pos = blockPos.ToVec3() + .5f;
                        world.PlaySound(AssetsSample.LiquidWater, pos, random.NextFloat() * .25f + .75f, random.NextFloat() + .5f);
                    }
                }
                else if (random.Next(10) == 0)
                {
                    world.SpawnParticle(EnumParticle.Suspend, 1,
                        new vec3(blockPos.X + .5f, blockPos.Y + .5f, blockPos.Z + .5f), new vec3(1f), 0);
                }
            }
            else if (material == EnumMaterial.Lava || material == EnumMaterial.Oil)
            {
                if (material == EnumMaterial.Lava && random.Next(100) == 0)
                {
                    vec3 pos = new vec3(blockPos.X + random.NextFloat(), blockPos.Y + 1, blockPos.Z + random.NextFloat());
                    world.SpawnParticle(EnumParticle.BlockPart, 3, pos, new vec3(.25f, 1, .25f), 1, (int)EnumBlock.Lava);
                    world.PlaySound(AssetsSample.LiquidLavaPop, pos, .2f + random.NextFloat() * .2f, .9f + random.NextFloat() * .15f);
                }
                if (random.Next(200) == 0)
                {
                    vec3 pos = blockPos.ToVec3() + .5f;
                    world.PlaySound(AssetsSample.LiquidLava, pos, .2f + random.NextFloat() * .2f, .9f + random.NextFloat() * .15f);
                }
            }
        }

        /// <summary>
        /// Обновить блок в такте
        /// </summary>
        public override void UpdateTick(WorldBase world, BlockPos blockPos, BlockState blockState, Rand random)
        {
            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();

            // течение по сторонам
            ListMvk<Pole> vecs = GetVectors(world, blockPos, 15);
            for (int i = 0; i < vecs.Count; i++)
            {
                // по бокам
                BlockPos pos = blockPos.Offset(vecs[i]);
                int levelLiquid = GetLevelLiquid(world, pos);
                int s = 15 - stepWave;
                if (levelLiquid != -1 && levelLiquid < s)
                {
                    InteractionWithBlocks(world, pos);
                    world.SetBlockState(pos, new BlockState((ushort)eBlockFlowing, (ushort)(vecs[i] == Pole.Down ? 15 : s)), 14);
                    world.SetBlockTick(pos, tickRate);
                }
            }
            //float f= stopwatch.ElapsedTicks / (float)MvkStatic.TimerFrequency;
            //if (f > 1) world.Log.Log("T {0:0.0000}", f);
        }

        /// <summary>
        /// Получить список направлений куда может течь жидкость
        /// </summary>
        protected ListMvk<Pole> GetVectors(WorldBase world, BlockPos blockPos, int level)
        {
            BlockPos pos;
            ListMvk<Pole> list = new ListMvk<Pole>(5);
            // течение вниз
            bool down = CheckCan(world, blockPos.OffsetDown());
            if (down) list.Add(Pole.Down);

            int stepStart = 1000;
            for (int i = 0; i < 4; i++)
            {
                Pole pole = EnumFacing.GetHorizontal(i);
                pos = blockPos.Offset(pole);
                if (CheckCanNotMaterial(world, pos))
                {
                    int step = 0;
                    if (!CheckCan(world, pos.OffsetDown()))
                    {
                        step = GetVectorsLong(world, pos, 1, EnumFacing.GetOpposite(pole), ((level - 1) / stepWave) - 1);
                    }
                    if (step < stepStart)
                    {
                        list.Clear();
                        if (down) list.Add(Pole.Down);
                    }

                    if (step <= stepStart)
                    {
                        list.Add(EnumFacing.GetHorizontal(i));
                        stepStart = step;
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Находим растояние в конкретной позиции
        /// </summary>
        /// <param name="world">мир</param>
        /// <param name="blockPos">позиция проверки</param>
        /// <param name="step">шаг смещения от начала</param>
        /// <param name="pole">откуда пришёл</param>
        protected int GetVectorsLong(WorldBase world, BlockPos blockPos, int step, Pole poleOpposite, int level)
        {
            int stepResult = 1000;
            BlockPos pos;
            Pole pole;
            for (int i = 0; i < 4; i++)
            {
                pole = EnumFacing.GetHorizontal(i);
                if (pole != poleOpposite)
                {
                    pos = blockPos.Offset(pole);
                    BlockState blockState = world.GetBlockState(pos);
                    if (CheckCanNotMaterial(world, pos))
                    {
                        if (CheckCan(world, pos.OffsetDown())) return step;
                        if (step < level)
                        {
                            int stepCache = GetVectorsLong(world, pos, step + 1, EnumFacing.GetOpposite(pole), level);
                            if (stepCache < stepResult) stepResult = stepCache;
                        }
                    }
                }
            }

            return stepResult;
        }

        /// <summary>
        /// Получиь уровень жидкости с этой позиции где, 15 макс, 0 можно разлить воды нет, -1 нельзя разлить воды нет.
        /// </summary>
        protected int GetLevelLiquid(WorldBase world, BlockPos pos)
        {
            BlockState blockState = world.GetBlockState(pos);
            EnumBlock enumBlock = blockState.GetBlock().EBlock;
            return enumBlock == eBlock ? 15 : enumBlock == eBlockFlowing ? blockState.met : CheckCan(blockState) ? 0 : - 1;
        }

        /// <summary>
        /// Статус для растекания на огонь
        /// </summary>
        protected virtual bool IsFire(EnumMaterial eMaterial) => eMaterial == EnumMaterial.Fire;

        /// <summary>
        /// Можно ли разлить на эту позицию жидкость
        /// </summary>
        protected bool CheckCan(BlockState state)
        {
            BlockBase block = state.GetBlock();
            EnumMaterial eMaterial = block.Material;
            return block.IsAir || block.IsLiquidDestruction() || IsFire(eMaterial)
                || block.Material == material
                // Взаимодействие лавы с водой и нефтью
                || (material == EnumMaterial.Lava && (block.Material == EnumMaterial.Oil || block.Material == EnumMaterial.Water))
                // Взаимодействие воды и нефти с лавой
                || (block.Material == EnumMaterial.Lava && (material == EnumMaterial.Oil || material == EnumMaterial.Water));
        }

        /// <summary>
        /// Можно ли разлить на эту позицию жидкость
        /// </summary>
        protected bool CheckCan(WorldBase world, BlockPos pos) => CheckCan(world.GetBlockState(pos));

        /// <summary>
        /// Можно ли разлить на эту позицию жидкость
        /// </summary>
        protected bool CheckCanNotMaterial(WorldBase world, BlockPos pos)
        {
            BlockBase block = world.GetBlockState(pos).GetBlock();
            EnumMaterial eMaterial = block.Material;
            return block.IsAir || block.IsLiquidDestruction() || IsFire(eMaterial)
                || block.EBlock == eBlockFlowing;
        }

        /// <summary>
        /// Взаимодействие с блоками
        /// </summary>
        protected void InteractionWithBlocks(WorldBase world, BlockPos blockPos)
        {
            // Дроп
            BlockState blockState = world.GetBlockState(blockPos);
            BlockBase block = blockState.GetBlock();
            if (!block.IsAir && block.Material != material)
            {
                block.DropBlockAsItem(world, blockPos, blockState, 0);
                world.SetBlockToAir(blockPos, 30);

                // Затухание огня
                if (block.EBlock == EnumBlock.Fire)
                {
                    EffectFadeFire(world, blockPos);
                }
            }
        }

        /// <summary>
        /// Эффект затухания огня
        /// </summary>
        protected virtual void EffectFadeFire(WorldBase world, BlockPos blockPos) => EffectMixingWater(world, blockPos);

        /// <summary>
        /// Эффект при смешивании лавы с нефтью
        /// </summary>
        protected virtual void EffectMixingOil(WorldBase world, BlockPos blockPos)
        {
            // Бум...
            world.PlaySound(AssetsSample.Fire, blockPos.ToVec3() + .5f, 1f + world.Rnd.NextFloat(), world.Rnd.NextFloat() * .7f + .3f);
        }

        /// <summary>
        /// Эффект при смешивании лавы с водой
        /// </summary>
        protected virtual void EffectMixingWater(WorldBase world, BlockPos blockPos)
        {
            // Пш...
            vec3 vec = new vec3(blockPos.X + .5f, blockPos.Y + .5f, blockPos.Z + .5f);
            world.PlaySound(AssetsSample.FireFizz, vec, .5f,
                2.6f + (world.Rnd.NextFloat() - world.Rnd.NextFloat()) * .8f);
            world.SpawnParticle(EnumParticle.Smoke, 8, vec, new vec3(1), 0, 40);
        }

        /// <summary>
        /// Изменить ускорение на блоке
        /// </summary>
        public override vec3 ModifyAcceleration(WorldBase worldIn, BlockPos blockPos, vec3 motion) 
            => motion + GetVectorFlow(worldIn, blockPos);

        #region Static

        /// <summary>
        /// Получить угол течения
        /// </summary>
        public static float GetAngleFlow(WorldBase world, BlockPos blockPos)
        {
            vec3 vec = GetVectorFlow(world, blockPos);
            return (vec.x == 0f && vec.z == 0f) ? -1000f : glm.atan2(vec.z, vec.x) - glm.pi90;
        }

        /// <summary>
        /// Получить вектор течения, нормализованный
        /// </summary>
        private static vec3 GetVectorFlow(WorldBase world, BlockPos blockPos)
        {
            vec3 vec = new vec3(0);
            int metLevel = GetLevelLiquidStatic(world, blockPos);
            if (metLevel <= 0) return vec;
            if (metLevel == 15) return new vec3(0, -1, 0);

            BlockPos blockPos2;
            int metLevel2;
            int j;
            for (int i = 0; i < 4; i++)
            {
                blockPos2 = blockPos.Offset(EnumFacing.GetHorizontal(i));
                metLevel2 = GetLevelLiquidStatic(world, blockPos2);

                if (metLevel2 > 0 && metLevel2 < 16)
                {
                    j = metLevel - metLevel2;
                    vec.x += (blockPos2.X - blockPos.X) * j;
                    vec.z += (blockPos2.Z - blockPos.Z) * j;
                }
            }
            if (vec.x == 0 && vec.z == 0) return vec;
            //vec.y -= 6;
            return vec.normalize();
        }

        /// <summary>
        /// Получиь уровень жидкости с этой позиции где 16 стоячая, 15 макс, 0 можно разлить воды нет, -1 нельзя разлить воды нет.
        /// </summary>
        private static int GetLevelLiquidStatic(WorldBase world, BlockPos pos)
        {
            BlockState blockState = world.GetBlockState(pos);
            EnumBlock enumBlock = blockState.GetBlock().EBlock;
            if (enumBlock == EnumBlock.Water || enumBlock == EnumBlock.Oil || enumBlock == EnumBlock.Lava)
            {
                return 16;
            }
            if (enumBlock == EnumBlock.WaterFlowing || enumBlock == EnumBlock.OilFlowing || enumBlock == EnumBlock.LavaFlowing)
            {
                return blockState.met;
            }
            BlockBase block = blockState.GetBlock();
            if (block.IsAir) return 0;
            EnumMaterial eMaterial = block.Material;
            return (block.IsLiquidDestruction()
                || eMaterial == EnumMaterial.Fire
                || eMaterial == EnumMaterial.Water
                || eMaterial == EnumMaterial.Lava
                || eMaterial == EnumMaterial.Oil) ? 0 : -1;
        }

        /// <summary>
        /// Получить угол течения
        /// </summary>
        public static float GetAngleFlow(int l11, int l01, int l10, int l12, int l21)
        {
            vec3 vec = GetVectorFlow(l11, l01, l10, l12, l21);
            return (vec.x == 0f && vec.z == 0f) ? -1000f : glm.atan2(vec.z, vec.x) - glm.pi90;
        }

        /// <summary>
        /// Получить вектор течения, нормализованный
        /// </summary>
        private static vec3 GetVectorFlow(int metLevel, int l01, int l10, int l12, int l21)
        {
            vec3 vec = new vec3(0);
            if (metLevel <= 0) return vec;

            if (l01 > 0) vec.x -= metLevel - l01;
            if (l10 > 0) vec.z -= metLevel - l10;
            if (l21 > 0) vec.x += metLevel - l21;
            if (l12 > 0) vec.z += metLevel - l12;
            vec.y -= 6f;
            return vec.normalize();
        }

        #endregion

        /// <summary>
        /// Поджечь соседние горючие блокти, для лавы
        /// </summary>
        protected void SetFireTo(WorldBase world, BlockPos blockPos, Rand random)
        {
            BlockPos pos;
            BlockBase block;
            for (int i = 0; i < 3; i++)
            {
                pos = blockPos.Offset(random.Next(3) - 1, random.Next(2), random.Next(3) - 1);
                block = world.GetBlockState(pos).GetBlock();
                if (block.Combustibility)
                {
                    pos = pos.OffsetUp();
                    if (world.GetBlockState(pos).GetEBlock() == EnumBlock.Air)
                    {
                        world.SetBlockState(pos, new BlockState(EnumBlock.Fire), 14);
                    }
                }
            }
        }

        /// <summary>
        /// Является ли блок проходимым, т.е. можно ли ходить через него
        /// </summary>
        public override bool IsPassable(int met) => true;

    }
}
