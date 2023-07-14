using MvkServer.Glm;
using MvkServer.Util;
using System.Collections.Generic;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Абстрактный объект электрических блоков
    /// </summary>
    public abstract class BlockAbElectricity : BlockBase
    {
        /// <summary>
        /// Сколько игровых тиков между тиками
        /// </summary>
        protected uint tickRate = 2;

        /// <summary>
        /// Игнорирование PowerOn
        /// </summary>
        private bool isIgnorePowerOn;
        /// <summary>
        /// Игнорирование PowerOff
        /// </summary>
        private bool isIgnorePowerOff;
        /// <summary>
        /// Список блоков которые надо включить
        /// </summary>
        private List<BlockPos> listPowerOn = new List<BlockPos>();
        /// <summary>
        /// Список блоков которые надо выключить
        /// </summary>
        private List<BlockPos> listPowerOff = new List<BlockPos>();
        /// <summary>
        /// Список блоков батареи которые попали полное отключение
        /// </summary>
        private ListMvk<BlockPos> listBatteryOff = new ListMvk<BlockPos>();
        /// <summary>
        /// Сильнее
        /// </summary>
        private ArrayMvk<Cell> cellsStronger = new ArrayMvk<Cell>(1089);
        /// <summary>
        /// Слабее
        /// </summary>
        private ArrayMvk<Cell> cellsWeaker = new ArrayMvk<Cell>(1089);
        /// <summary>
        /// Кэш
        /// </summary>
        private ArrayMvk<Cell> cellsCache = new ArrayMvk<Cell>(1089);
        /// <summary>
        /// Список для разрядки
        /// </summary>
        private ArrayMvk<BlockPos> cellsDischarge = new ArrayMvk<BlockPos>(1089);
        /// <summary>
        /// Список для разрядки кэш
        /// </summary>
        private ArrayMvk<BlockPos> cellsDischargeCache = new ArrayMvk<BlockPos>(1089);

        public BlockAbElectricity(int numberTexture)
        {
            Material = Materials.GetMaterialCache(EnumMaterial.Electricity);
            Particle = numberTexture;
        }

        /// <summary>
        /// Сколько ударов требуется, чтобы сломать блок в тактах (20 тактов = 1 секунда)
        /// </summary>
        public override int Hardness(BlockState state) => 2;

        /// <summary>
        /// Действие блока после его установки
        /// </summary>
        public override void OnBlockAdded(WorldBase worldIn, BlockPos blockPos, BlockState state) 
            => UpdateSurroundingElectricity(worldIn, blockPos, state);

        /// <summary>
        /// Действие блока после его удаления
        /// </summary>
        public override void OnBreakBlock(WorldBase worldIn, BlockPos blockPos, BlockState state) 
            => UpdateSurroundingElectricity(worldIn, blockPos, state);

        /// <summary>
        /// Обновить окружающее электричество
        /// </summary>
        protected virtual void UpdateSurroundingElectricity(WorldBase world, BlockPos blockPos, BlockState state)
        {
            // TODO::2022-10-27 Надо подумать, как жить если не попали сюда...
            if (world.IsAreaLoaded(blockPos, 16)) 
            {
                BlockState blockState = world.GetBlockState(blockPos);
                int powerBegin = GetPower(blockState);
                if (powerBegin == -2 || powerBegin == -3)
                {
                    // Разрыв, соседние блоки затемняем
                    cellsWeaker.Clear();
                    cellsStronger.Clear();
                    int power = GetPower(state);
                    cellsWeaker.Add(new Cell(blockPos, power));
                    isIgnorePowerOn = false;
                    isIgnorePowerOff = false;
                    listPowerOff.Clear();
                    listPowerOn.Clear();
                    Weaker(world);
                    Stronger(world);
                    Power(world);
                    return;
                }
                int powerBiside;

                if (powerBegin == -1)
                {
                    // Блок энергии не использует мощность, и значит не передаёт её, по этому игнорируем
                    if (blockState.GetBlock().IsUnitConnectedToElectricity())
                    {
                        // Делаем доп проверку на включение
                        powerBiside = GetLevelBesideMax(world, blockPos);
                        if (powerBiside > 0)
                        {
                            PowerOn(world, blockPos);
                        }
                    }
                    return;
                }

                powerBiside = GetLevelBesideMax(world, blockPos);

                if (powerBiside == -1)
                {
                    // Блоков энергии рядом нет
                    if (powerBegin == 16)
                    {
                        // Делаем дополнительную проверку на активацию
                        BlockPos blockPosBiside;
                        for (int i = 0; i < 6; i++)
                        {
                            blockPosBiside = blockPos.Offset(i);
                            if (world.GetBlockState(blockPosBiside).GetBlock().IsUnitConnectedToElectricity())
                            {
                                PowerOn(world, blockPosBiside);
                            }
                        }
                    }
                    return;
                }

                bool isStronge = false;
                if (powerBiside > powerBegin)
                {
                    // Соседний блок мощнее, коректируем мощность и делаем проверку соседних
                    isStronge = true;
                    powerBiside--;
                    SetBlockStateMetPower(world, blockPos, powerBiside);
                }
                else if (powerBiside <= powerBegin && powerBegin > 0)
                {
                    // Соседний блок слабее, делаем проверку соседних
                    isStronge = true;
                    powerBiside = powerBegin;
                }

                if (isStronge)
                {
                    // Осветление
                    cellsStronger.Clear();
                    cellsStronger.Add(new Cell(blockPos, powerBiside));
                    isIgnorePowerOn = false;
                    listPowerOn.Clear();
                    listPowerOff.Clear();
                    Stronger(world);
                    Power(world);
                }
            }
        }
        
        /// <summary>
        /// Разряжение ближайшего источника
        /// </summary>
        private void Discharge(WorldBase world, BlockPos blockPos)
        {
            bool[,,] temp = new bool[33, 33, 33];
            BlockPos blockPosBeside;
            BlockState blockState;
            BlockBase block;
            cellsDischarge.Clear();
            cellsDischarge.Add(blockPos);
            int ox, oy, oz, x, y, z;
            ox = blockPos.X;
            oy = blockPos.Y;
            oz = blockPos.Z;
            temp[16, 16, 16] = true;
            cellsWeaker.Clear();
            bool ignor = false;

            while (cellsDischarge.count > 0)
            {
                cellsDischargeCache.Clear();
                for (int i = 0; i < cellsDischarge.count; i++)
                {
                    blockPos = cellsDischarge[i];
                    for (int iSide = 0; iSide < 6; iSide++)
                    {
                        blockPosBeside = blockPos.Offset(iSide);
                        x = blockPosBeside.X - ox + 16;
                        y = blockPosBeside.Y - oy + 16;
                        z = blockPosBeside.Z - oz + 16;
                        if (!temp[x, y, z])
                        {
                            temp[x, y, z] = true;
                            blockState = world.GetBlockState(blockPosBeside);
                            block = blockState.GetBlock();
                            if (block.Material.EMaterial == EnumMaterial.Electricity)
                            {
                                cellsDischargeCache.Add(blockPosBeside);
                            }
                            if (block.EBlock == EnumBlock.ElBattery)
                            {
                                if (blockState.met > 0)
                                {
                                    // Разрядка
                                    if (!ignor)
                                    {
                                        world.SetBlockStateMet(blockPosBeside, (ushort)(blockState.met - 1), true);
                                        ignor = true;
                                        if (blockState.met == 1)
                                        {
                                            listBatteryOff.Add(blockPosBeside);
                                        }
                                        else
                                        {
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        listBatteryOff.Add(blockPosBeside);
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
                cellsDischarge.Clear();
                if (cellsDischargeCache.count > 0)
                {
                    cellsDischarge.AddRange(cellsDischargeCache);
                }
            }
        }

        /// <summary>
        /// Находим максимальную мощность рядом, -1 нет блоков с мощностью, 16 рядом батарея
        /// </summary>
        private int GetLevelBesideMax(WorldBase world, BlockPos blockPos)
        {
            int levelMax = -1;
            int level;

            for (int i = 0; i < 6; i++)
            {
                level = GetPower(world.GetBlockState(blockPos.Offset(i)));
                if (level > levelMax) levelMax = level;
                if (levelMax == 16) break;
            }
            return levelMax;
        }

        /// <summary>
        /// Индивидуально пробегает разряд энергии в тике
        /// </summary>
        protected void DischargeTick(WorldBase world, BlockPos blockPos)
        {
            if (world.IsAreaLoaded(blockPos, 16))
            {
                listBatteryOff.Clear();
                // Пробегаемся включаем блоки, разряжение ближайшего источника
                PowerOnTick(world, blockPos);

                // В момент включения могли батарейки отключиться
                if (listBatteryOff.Count > 0)
                {
                    cellsWeaker.Add(new Cell(listBatteryOff[0], 16));
                    cellsStronger.Clear();
                    isIgnorePowerOn = true;
                    isIgnorePowerOff = listBatteryOff.Count == 2;
                    Weaker(world);
                    Stronger(world);
                }
            }
        }

        private void Power(WorldBase world)
        {
            // Надо убрать дублирование, т.е. выключили потом тут же включили
            List<BlockPos> listOn = new List<BlockPos>(listPowerOn.Count);
            List<BlockPos> listOff = new List<BlockPos>(listPowerOff.Count);
            for (int i = 0; i < listPowerOn.Count; i++)
            {
                if (!listPowerOff.Contains(listPowerOn[i])) listOn.Add(listPowerOn[i]);
            }
            for (int i = 0; i < listPowerOff.Count; i++)
            {
                if (!listPowerOn.Contains(listPowerOff[i])) listOff.Add(listPowerOff[i]);
            }

            // Выключаем, массив сформирован в Weaker
            if (listOff.Count > 0)
            {
                for (int i = 0; i < listOff.Count; i++)
                {
                    world.GetBlockState(listOff[i]).GetBlock().UnitDisconnectedFromElectricity(world, listOff[i]);
                }
            }
            // Включаем, массив сформирован в Stronger
            if (listOn.Count > 0)
            {
                listBatteryOff.Clear();
                // Пробегаемся включаем блоки, разряжение ближайшего источника
                for (int i = 0; i < listOn.Count; i++)
                {
                    PowerOn(world, listOn[i]);
                }
                // В момент включения могли батарейки отключиться
                if (listBatteryOff.Count > 0)
                {
                    cellsWeaker.Add(new Cell(listBatteryOff[0], 16));
                    cellsStronger.Clear();
                    isIgnorePowerOn = true;
                    isIgnorePowerOff = listBatteryOff.Count == 2;
                    Weaker(world);
                    Stronger(world);
                }
            }
        }

        /// <summary>
        /// Сильнее
        /// </summary>
        private void Stronger(WorldBase world)
        {
            if (cellsStronger.count > 0)
            {
                bool[,,] temp = new bool[33, 33, 33];
                BlockPos blockPos;
                BlockPos blockPosBeside;
                BlockState blockState;
                int power, powerBiside, ox, oy, oz, x, y, z;

                blockPos = cellsStronger[0].pos;
                ox = blockPos.X;
                oy = blockPos.Y;
                oz = blockPos.Z;
                temp[16, 16, 16] = true;

                while (cellsStronger.count > 0)
                {
                    cellsCache.Clear();
                    for (int i = 0; i < cellsStronger.count; i++)
                    {
                        blockPos = cellsStronger[i].pos;
                        power = cellsStronger[i].power - 1;

                        for (int iSide = 0; iSide < 6; iSide++)
                        {
                            blockPosBeside = blockPos.Offset(iSide);
                            x = blockPosBeside.X - ox + 16;
                            y = blockPosBeside.Y - oy + 16;
                            z = blockPosBeside.Z - oz + 16;
                            if (!temp[x, y, z])
                            {
                                temp[x, y, z] = true;
                                blockState = world.GetBlockState(blockPosBeside);
                                powerBiside = GetPower(blockState);
                                if (powerBiside >= 0 && powerBiside < power)
                                {
                                    SetBlockStateMetPower(world, blockPosBeside, power);
                                    cellsCache.Add(new Cell(blockPosBeside, power));
                                }
                                else if (powerBiside == -3)
                                {
                                    if (power > GetPower(blockState, true))
                                    {
                                        SetBlockStateMetPower(world, blockPosBeside, power);
                                    }
                                }
                                else if (!isIgnorePowerOn && powerBiside == -1)
                                {
                                    if (blockState.GetBlock().IsUnitConnectedToElectricity())
                                    {
                                        listPowerOn.Add(blockPosBeside);
                                    }
                                }
                            }
                        }
                    }
                    cellsStronger.Clear();
                    if (cellsCache.count > 0)
                    {
                        cellsStronger.AddRange(cellsCache);
                    }
                }
            }
        }

        /// <summary>
        /// Слабее
        /// </summary>
        private void Weaker(WorldBase world)
        {
            if (cellsWeaker.count > 0)
            {
                bool[,,] temp = new bool[33, 33, 33];
                BlockPos blockPos;
                BlockPos blockPosBeside;
                BlockState blockState;
                vec2i gp;
                int power, powerBiside, ox, oy, oz, x, y, z;

                blockPos = cellsWeaker[0].pos;
                ox = blockPos.X;
                oy = blockPos.Y;
                oz = blockPos.Z;
                temp[16, 16, 16] = true;

                while (cellsWeaker.count > 0)
                {
                    cellsCache.Clear();
                    for (int i = 0; i < cellsWeaker.count; i++)
                    {
                        blockPos = cellsWeaker[i].pos;
                        power = cellsWeaker[i].power - 1;

                        for (int iSide = 0; iSide < 6; iSide++)
                        {
                            blockPosBeside = blockPos.Offset(iSide);
                            x = blockPosBeside.X - ox + 16;
                            y = blockPosBeside.Y - oy + 16;
                            z = blockPosBeside.Z - oz + 16;
                            if (!temp[x, y, z])
                            {
                                temp[x, y, z] = true;
                                blockState = world.GetBlockState(blockPosBeside);
                                gp = GetPowerWeaker(blockState);
                                powerBiside = gp.x;
                                if (powerBiside >= 0 && powerBiside <= power)
                                {
                                    SetBlockStateMetPower(world, blockPosBeside, 0);
                                    cellsCache.Add(new Cell(blockPosBeside, power));
                                }
                                else if (powerBiside > 0 && gp.y == 0)
                                {
                                    cellsStronger.Add(new Cell(blockPosBeside, powerBiside));
                                }
                                else if (!isIgnorePowerOff && powerBiside == -1)
                                {
                                    if (blockState.GetBlock().IsUnitConnectedToElectricity())
                                    {
                                        listPowerOff.Add(blockPosBeside);
                                    }
                                }
                            }
                        }
                    }
                    cellsWeaker.Clear();
                    if (cellsCache.count > 0)
                    {
                        cellsWeaker.AddRange(cellsCache);
                    }
                }
                
            }
            return;
        }

        /// <summary>
        /// Проверить мощность сигнала и вернуть его, -3 разрыв, -2 воздух, -1 нет сигнала, 16 батарея
        /// </summary>
        private int GetPower(BlockState blockState, bool ignoreGap = false)
        {
            EnumBlock enumBlock = blockState.GetEBlock();
            if (enumBlock == EnumBlock.Air) return -2;
            if (enumBlock == EnumBlock.ElBattery) return blockState.met > 0 ? 16 : -2;
            if (enumBlock == EnumBlock.ElWire) return blockState.met;
            if (enumBlock == EnumBlock.ElSwitch) return ignoreGap ? blockState.met & 0xF
                    : (blockState.met >> 4) == 0 ? -3 : blockState.met & 0xF;
            return -1;
        }

        /// <summary>
        /// Проверить мощность сигнала и вернуть его, -2 воздух, -1 нет сигнала, 16 батарея
        /// y = 1 ElSwitch
        /// </summary>
        private vec2i GetPowerWeaker(BlockState blockState)
        {
            EnumBlock enumBlock = blockState.GetEBlock();
            if (enumBlock == EnumBlock.Air) return new vec2i(-2, 0);
            if (enumBlock == EnumBlock.ElBattery) return new vec2i(blockState.met > 0 ? 16 : -2, 0);
            if (enumBlock == EnumBlock.ElWire) return new vec2i(blockState.met, 0);
            if (enumBlock == EnumBlock.ElSwitch) return new vec2i(blockState.met & 0xF, 1);
            return new vec2i(-1, 0);
        }

        /// <summary>
        /// Изменить мет данные мощности электричсеского блока
        /// </summary>
        private void SetBlockStateMetPower(WorldBase world, BlockPos blockPos, int power)
        {
            BlockState blockState = world.GetBlockState(blockPos);
            EnumBlock enumBlock = blockState.GetEBlock();
            int met = -1;
            if (enumBlock == EnumBlock.ElWire) met = power;
            if (enumBlock == EnumBlock.ElSwitch) met = (blockState.met >> 4) << 4 | power & 0xF;

            if (met != -1)
            {
                world.SetBlockStateMet(blockPos, (ushort)met, true);
            }
        }

        /// <summary>
        /// Нагрузить источник
        /// </summary>
        private void PowerOn(WorldBase world, BlockPos blockPos)
        {
            BlockBase block = world.GetBlockState(blockPos).GetBlock();
            if (block.IsUnitConnectedToElectricity())
            {
                if (block.IsUnitDischargeToElectricity())
                {
                    Discharge(world, blockPos);
                }
                block.UnitConnectedToElectricity(world, blockPos);
            }
        }

        /// <summary>
        /// Нагрузить источник в тике
        /// </summary>
        private void PowerOnTick(WorldBase world, BlockPos blockPos)
        {
            BlockBase block = world.GetBlockState(blockPos).GetBlock();
            if (block.IsUnitConnectedToElectricity())
            {
                int power = GetLevelBesideMax(world, blockPos);
                if (power < 1)
                {
                    // Рядом нет мощности, отключаем
                    block.UnitDisconnectedFromElectricity(world, blockPos);
                }
                else if(block.IsUnitDischargeToElectricity())
                {
                    // Рядом есть источник, ищем источник и понижаем
                    Discharge(world, blockPos);
                }
            }
        }

        private struct Cell
        {
            public BlockPos pos;
            public int power;

            public Cell(BlockPos pos, int power)
            {
                this.pos = pos;
                this.power = power;
            }

            public override string ToString()
            {
                return pos.ToString() + " " + power;
            }
            public override bool Equals(object obj)
            {
                if (obj.GetType() == typeof(Cell))
                {
                    var cell = (Cell)obj;
                    if (pos.Equals(cell.pos) && power == cell.power) return true;
                }
                return false;
            }

            /// <summary>
            /// Returns a hash code for this instance.
            /// </summary>
            /// <returns>
            /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
            /// </returns>
            public override int GetHashCode() => pos.GetHashCode() ^ power.GetHashCode();
        }
    }
}
