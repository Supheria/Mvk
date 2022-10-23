using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Абстрактный объект жидких блоков
    /// </summary>
    public abstract class BlockAbLiquidFlowing : BlockAbLiquid
    {
        /// <summary>
        /// Скорость высыхания, 1 - лава и нефть, 3 - вода
        /// </summary>
        protected int dryingSpeed = 1;

        /// <summary>
        /// Обновить блок в такте
        /// </summary>
        public override void UpdateTick(WorldBase world, BlockPos blockPos, BlockState blockState, Rand random)
        {
            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();

            // Проверка бесконечного источника
            if (CheckEndlessSource(world, blockPos)) return;

            int met = blockState.met;

            // Проверка соседние блоки на уровень жидкости
            int levelLiquidUp = GetLevelLiquid(world, blockPos.OffsetUp());
            int levelLiquid = levelLiquidUp;
            if (levelLiquid < 15)
            {
                // проверим и горизонтальные
                for (int i = 0; i < 4; i++)
                {
                    int levelLiquidCache = GetLevelLiquid(world, blockPos.Offset(EnumFacing.GetHorizontal(i)));
                    if (levelLiquidCache > levelLiquid)
                    {
                        levelLiquid = levelLiquidCache;
                        if (levelLiquid == 15) break;
                    }
                }
            }
                
            if (met >= levelLiquid && (met < 15 || levelLiquidUp <= 0))
            {
                // уменьшаем, так-как рядом жидкость мельче
                met -= (stepWave * dryingSpeed);
                if (met <= 0)
                {
                    // высохла
                    world.SetBlockState(blockPos, new BlockState(EnumBlock.Air), 14);
                }
                else
                {
                    // мельче
                    world.SetBlockState(blockPos, new BlockState((ushort)eBlockFlowing, (ushort)met),
                        met < levelLiquid ? 14 : 12);
                    world.SetBlockTick(blockPos, tickRate);
                }
            }
            else if (met < levelLiquid - stepWave)
            {
                // добавляем уровень жидкости
                met += stepWave;
                if (met <= 15)
                {
                    world.SetBlockStateMet(blockPos, (ushort)met, true);
                    world.SetBlockTick(blockPos, tickRate);
                }
            }

            // надо продолжить растекаться
            BlockPos blockPosDown = blockPos.OffsetDown();
            if (CheckCan(world, blockPosDown))
            {
                // течение вниз
                BlockState blockStateDown = world.GetBlockState(blockPosDown);
                BlockBase blockDown = blockStateDown.GetBlock();

                // Микс с низу
                if (MixingDown(world, blockPos, blockState, blockPosDown, blockStateDown)) return;

                if (blockDown.EBlock != eBlock && blockStateDown.met < 15)
                {
                    InteractionWithBlocks(world, blockPosDown);
                    world.SetBlockState(blockPosDown, new BlockState((ushort)eBlockFlowing, 15), 14);
                    world.SetBlockTick(blockPosDown, tickRate);
                }
            }
            else
            {
                met -= stepWave;
                if (met >= 0)
                {
                    // течение по сторонам
                    ListMvk<Pole> vecs = GetVectors(world, blockPos, blockState.met);

                    for (int i = 0; i < vecs.Count; i++)
                    {
                        if (vecs[i] == Pole.Down) continue;
                        // по бокам
                        BlockPos pos = blockPos.Offset(vecs[i]);
                        levelLiquid = GetLevelLiquid(world, pos);
                        if (levelLiquid != -1 && levelLiquid < met)
                        {
                            InteractionWithBlocks(world, pos);
                            world.SetBlockState(pos, new BlockState((ushort)eBlockFlowing, (ushort)met), 14);
                            world.SetBlockTick(pos, tickRate);
                        }
                    }
                }
            }
            //float f = stopwatch.ElapsedTicks / (float)MvkStatic.TimerFrequency;
            //if (f > 1) world.Log.Log("D {0:0.0000}", f);
        }

        /// <summary>
        /// Проверка бесконечного источника
        /// </summary>
        protected virtual bool CheckEndlessSource(WorldBase world, BlockPos blockPos) => false;

        /// <summary>
        /// Смешивание снизу
        /// </summary>
        protected virtual bool MixingDown(WorldBase world, BlockPos blockPos, BlockState state, BlockPos blockPosDown, BlockState blockStateDown)
        {
            if (material == EnumMaterial.Lava)
            {
                EnumMaterial eMaterial = blockStateDown.GetBlock().Material;
                if (eMaterial == EnumMaterial.Water)
                {
                    world.SetBlockState(blockPosDown, new BlockState(EnumBlock.Basalt), 14);
                    EffectMixingWater(world, blockPosDown);
                    return true;
                }
                if (eMaterial == EnumMaterial.Oil)
                {
                    world.SetBlockState(blockPosDown, new BlockState(EnumBlock.Fire), 14);
                    EffectMixingOil(world, blockPosDown);
                    return true;
                }
            }
            return false;
        }
    }
}
