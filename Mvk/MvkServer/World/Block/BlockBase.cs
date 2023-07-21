using MvkServer.Entity;
using MvkServer.Entity.List;
using MvkServer.Glm;
using MvkServer.Item;
using MvkServer.Item.List;
using MvkServer.Sound;
using MvkServer.Util;

namespace MvkServer.World.Block
{
    /// <summary>
    /// Базовый объект Блока
    /// </summary>
    public abstract class BlockBase
    {
        // У блока может быть один из параметров true FullBlock || Liquid || IsUnique || IsAir

        /// <summary>
        /// Ограничительная рамка занимает весь блок, для оптимизации, без проверки AABB блока
        /// </summary>
        public bool FullBlock { get; private set; } = true;
        /// <summary>
        /// Блок жидкости: вода, лава, нефть
        /// </summary>
        public bool Liquid { get; private set; } = false;
        /// <summary>
        /// Является ли эта модель не блоком, со всеми сторонами и прозрачной
        /// </summary>
        public bool IsUnique { get; private set; } = false;
        /// <summary>
        /// Явлыется ли блок небом
        /// </summary>
        public bool IsAir { get; private set; } = false;

        /// <summary>
        /// Прорисовка возможно с обеих сторон, для уникальных блоков, типа трава, листва и подобное
        /// </summary>
        public bool BothSides { get; private set; } = false;

        /// <summary>
        /// Сколько света вычитается для прохождения этого блока Air = 0
        /// В VoxelEngine он в public static byte GetBlockLightOpacity(EnumBlock eblock)
        /// получть инфу не создавая блок
        /// </summary>
        public byte LightOpacity { get; protected set; } = 15;
        /// <summary>
        /// Количество излучаемого света (плафон)
        /// </summary>
        public int LightValue { get; protected set; } = 0;
        /// <summary>
        /// Полупрозрачный, альфа блок, вода, стекло...
        /// </summary>
        public bool Translucent { get; protected set; } = false;
        /// <summary>
        /// Флаг, если блок должен использовать самое яркое значение соседнего света как свое собственное
        /// Пример: листва, вода, стекло
        /// </summary>
        public bool UseNeighborBrightness { get; protected set; } = false;
        /// <summary>
        /// Все стороны принудительно, пример: трава, стекло, вода, лава
        /// </summary>
        public bool AllSideForcibly { get; protected set; } = false;
        /// <summary>
        /// Обрабатывается блок эффектом АmbientOcclusion
        /// </summary>
        public bool АmbientOcclusion { get; protected set; } = true;
        /// <summary>
        /// Обрабатывается блок эффектом Плавного перехода цвета между биомами
        /// </summary>
        public bool BiomeColor { get; protected set; } = false;
        /// <summary>
        /// При значении flase у AllSideForcibly + обнотипные блоков не будет между собой сетки, пример: вода, блок стекла
        /// </summary>
        public bool BlocksNotSame { get; protected set; } = true;
        /// <summary>
        /// Может ли быть тень сущности на блоке, только для целых блоков
        /// </summary>
        public bool Shadow { get; protected set; } = true;
        /// <summary>
        /// Устойчивость блоков к взрывам.
        /// 10 камень, 5 дерево, руда, 0.6 стекло, 0.5 земля, песок, 0.2 листва, 0.0 трава, саженцы 
        /// </summary>
        public float Resistance { get; protected set; } = 0;
        /// <summary>
        /// Может ли этот блок выпасть от взрыва, защита чтоб взрываемый блок не выпадал
        /// </summary>
        public bool CanDropFromExplosion { get; protected set; } = true;
        /// <summary>
        /// Включить в статистику. (для будущего)
        /// </summary>
        public bool EnableStats { get; protected set; } = true;
        /// <summary>
        /// Отмечает, относится ли этот блок к типу, требующему случайной пометки в тиках. 
        /// Объект ChunkStorage подсчитывает блоки, чтобы в целях эффективности отобрать фрагмент из 
        /// случайного списка обновлений фрагментов.
        /// </summary>
        public bool NeedsRandomTick { get; protected set; } = false;
        /// <summary>
        /// Скользкость
        /// 0.6 стандартная
        /// 0.8 медленее, типа по песку
        /// 0.98 по льду, скользко
        /// </summary>
        public float Slipperiness { get; protected set; } = .6f;
        /// <summary>
        /// Получить тип блока
        /// </summary>
        public EnumBlock EBlock { get; protected set; }
        /// <summary>
        /// Можно ли выбирать блок
        /// </summary>
        public bool IsAction { get; protected set; } = true;
        /// <summary>
        /// Может ли блок сталкиваться
        /// </summary>
        public bool IsCollidable { get; protected set; } = true;
        
        /// <summary>
        /// Индекс картинки частички
        /// </summary>
        public int Particle { get; protected set; } = 0;
        /// <summary>
        /// Имеется ли у блока частичка
        /// </summary>
        public bool IsParticle { get; protected set; } = true;
        /// <summary>
        /// Может на этот блок поставить другой, к примеру трава
        /// </summary>
        public bool IsReplaceable { get; protected set; } = false;
        /// <summary>
        /// Имеет ли блок дополнительные данные свыше 4 bit
        /// </summary>
        public bool IsAddMet { get; protected set; } = false;
        
        /// <summary>
        /// Горючесть материала
        /// </summary>
        public bool Combustibility { get; protected set; } = false;
        /// <summary>
        /// Увеличить шансы загорания 0-100 %
        /// 100 мгновенно загарается без рандома
        /// 99 уже через рандом, не так быстро загорается но шанс очень большой
        /// Из-за долго жизни огня, 30 как правило загорится если вверх, рядом 1/1
        /// </summary>
        public byte IgniteOddsSunbathing { get; protected set; } = 0;
        /// <summary>
        /// Шансы на сжигание 0-100 %
        /// 100 мгновенно згарает без рандома
        /// 99 уже через рандом, не так быстро сгорит но шанс очень большой
        /// Из-за долго жизни огня, 60 как правило сгорит
        /// </summary>
        public byte BurnOdds { get; protected set; } = 0;
        /// <summary>
        /// Цвет трещины разрушения чисто чёрный без альфы
        /// </summary>
        public bool IsDamagedBlockBlack { get; protected set; } = false;

        /// <summary>
        /// Присутствует дроп у блока
        /// </summary>
        protected bool canDropPresent = true;
        /// <summary>
        /// Цвет блока по умолчани, биом потом заменит
        /// Для частички
        /// </summary>
        protected vec3 color = new vec3(1);
        /// <summary>
        /// Стороны для прорисовки жидкого блока
        /// </summary>
        protected SideLiquid[] sideLiquids;
        /// <summary>
        /// Стороны целого блока для прорисовки блока quads
        /// </summary>
        protected QuadSide[][] quads = new QuadSide[][] { new QuadSide[] { new QuadSide(0) } };
        /// <summary>
        /// Семплы сломоного блока
        /// </summary>
        protected AssetsSample[] samplesBreak = new AssetsSample[] {
            AssetsSample.DigStone1, AssetsSample.DigStone2,
            AssetsSample.DigStone3, AssetsSample.DigStone4 };
        /// <summary>
        /// Семплы установленного блока
        /// </summary>
        protected AssetsSample[] samplesPut = new AssetsSample[] {
            AssetsSample.DigStone1, AssetsSample.DigStone2,
            AssetsSample.DigStone3, AssetsSample.DigStone4 };
        /// <summary>
        /// Семплы хотьбы по блоку
        /// </summary>
        protected AssetsSample[] samplesStep = new AssetsSample[] { 
            AssetsSample.StepStone1, AssetsSample.StepStone2,
            AssetsSample.StepStone3, AssetsSample.StepStone4 };

        /// <summary>
        /// Материал блока
        /// </summary>
        public MaterialBase Material { get; protected set; }
        /// <summary>
        /// Объект крафта
        /// </summary>
        public CraftItem Craft { get; private set; } = new CraftItem();

        /// <summary>
        /// Цвет для подмешевания блока, для гуи или частичек
        /// </summary>
        public vec3 GetColorGuiOrPartFX() => color;

        /// <summary>
        /// Получить сторону для прорисовки жидкого блока
        /// </summary>
        public SideLiquid GetSideLiquid(int index) => sideLiquids[index];

        #region Quad

        /// <summary>
        /// Стороны целого блока для рендера
        /// </summary>
        public virtual QuadSide[] GetQuads(int met, int xc, int zc, int xb, int zb) => quads[0];

        /// <summary>
        /// Коробки для рендера 2д GUI
        /// </summary>
        public virtual QuadSide[] GetQuadsGui() => quads[0];// quads != null ? quads[0] : new QuadSide[0];
        /// <summary>
        /// Имеется ли у блока смена цвета травы от биома из первого квадра
        /// </summary>
        public bool IsBiomeColorGrass() => quads[0][0].IsBiomeColorGrass();

        /// <summary>
        /// Инициализация коробок всех одной текстурой с параметром Нет бокового затемнения, пример: трава, цветы
        /// </summary>
        protected void InitQuads(int numberTexture, bool noSideDimming = false)
        {
            quads = new QuadSide[][] { new QuadSide[] {
                new QuadSide(0).SetTexture(numberTexture).SetSide(Pole.Up, noSideDimming),
                new QuadSide(0).SetTexture(numberTexture).SetSide(Pole.Down, noSideDimming),
                new QuadSide(0).SetTexture(numberTexture).SetSide(Pole.East, noSideDimming),
                new QuadSide(0).SetTexture(numberTexture).SetSide(Pole.West, noSideDimming),
                new QuadSide(0).SetTexture(numberTexture).SetSide(Pole.North, noSideDimming),
                new QuadSide(0).SetTexture(numberTexture).SetSide(Pole.South, noSideDimming)
            } };
        }

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected void InitQuads(int t1, int t2, int t3, int t4, int t5, int t6, bool noSideDimming = false)
        {
            quads = new QuadSide[][] { new QuadSide[] {
                new QuadSide(0).SetTexture(t1).SetSide(Pole.Up, noSideDimming),
                new QuadSide(0).SetTexture(t2).SetSide(Pole.Down, noSideDimming),
                new QuadSide(0).SetTexture(t3).SetSide(Pole.East, noSideDimming),
                new QuadSide(0).SetTexture(t4).SetSide(Pole.West, noSideDimming),
                new QuadSide(0).SetTexture(t5).SetSide(Pole.North, noSideDimming),
                new QuadSide(0).SetTexture(t6).SetSide(Pole.South, noSideDimming)
            } };
        }

        /// <summary>
        /// Получить прямоугольник стороны
        /// </summary>
        protected QuadSide Quad(int x1, int x2, int z1, int z2, int u1, int u2, int v1, int v2, Pole pole, int texture)
            => new QuadSide(0).SetTexture(texture, u1, v1, u2, v2).SetSide(pole, false, x1, 0, z1, x2, 16, z2);

        /// <summary>
        /// Получить прямоугольник стороны с подержкой цвета
        /// </summary>
        protected QuadSide QuadColor(int x1, int x2, int z1, int z2, int u1, int u2, int v1, int v2, Pole pole, int texture)
            => new QuadSide(4).SetTexture(texture, u1, v1, u2, v2).SetSide(pole, false, x1, 0, z1, x2, 16, z2);

        #endregion

        /// <summary>
        /// Задать блок воздуха
        /// </summary>
        protected void SetAir()
        {
            Material = Materials.GetMaterialCache(EnumMaterial.Air);
            IsAir = true;
            FullBlock = false;
            IsAction = false;
            IsParticle = false;
            АmbientOcclusion = false;
            Shadow = false;
            IsReplaceable = true;
            LightOpacity = 0;
            canDropPresent = false;
        }

        /// <summary>
        /// Задать уникальную прозрачную модель не целого блока
        /// </summary>
        protected void SetUnique(bool bothSides = false)
        {
            IsUnique = true;
            FullBlock = false;
            AllSideForcibly = true;
            LightOpacity = 0;
            АmbientOcclusion = false;
            Shadow = false;
            BothSides = bothSides;
        }

        protected void SetLiquid()
        {
            Liquid = true;
            FullBlock = false;
            AllSideForcibly = true;
            BlocksNotSame = false;
            UseNeighborBrightness = true;
            IsAction = false;
            IsCollidable = false;
            АmbientOcclusion = false;
            Shadow = false;
            IsReplaceable = true;
            IsParticle = false;
            Resistance = 100f;
            canDropPresent = false;
        }

        /// <summary>
        /// Задать тип блока
        /// </summary>
        public void SetEnumBlock(EnumBlock enumBlock) => EBlock = enumBlock;

        /// <summary>
        /// Передать список  ограничительных рамок блока
        /// </summary>
        public virtual AxisAlignedBB[] GetCollisionBoxesToList(BlockPos pos, int met)
        {
            vec3 min = pos.ToVec3();
            return new AxisAlignedBB[] { new AxisAlignedBB(min, min + 1f) };
        }

        /// <summary>
        /// Получить одну большую рамку блока, если их несколько они объеденяться
        /// </summary>
        public AxisAlignedBB GetCollision(BlockPos pos, int met)
        {
            if (!IsCollidable) return null;
            AxisAlignedBB[] axes = GetCollisionBoxesToList(pos, met);
            if (axes.Length > 0)
            {
                AxisAlignedBB aabb = axes[0];
                for (int i = 1; i < axes.Length; i++)
                {
                    aabb = aabb.AddCoord(axes[i].Min).AddCoord(axes[i].Max);
                }
                return aabb;
            }
            vec3 min = pos.ToVec3();
            return new AxisAlignedBB(min, min + 1f);
        }

        /// <summary>
        /// Проверить колизию блока на пересечение луча
        /// </summary>
        /// <param name="pos">позиция блока</param>
        /// <param name="a">точка от куда идёт лучь</param>
        /// <param name="dir">вектор луча</param>
        /// <param name="maxDist">максимальная дистания</param>
        public bool CollisionRayTrace(BlockPos pos, int met, vec3 a, vec3 dir, float maxDist)
        {
            if (IsAction)
            {
                if (FullBlock) return true;

                // Если блок не полный, обрабатываем хитбокс блока
                RayCross ray = new RayCross(a, dir, maxDist);
                return ray.IsCrossAABBs(GetCollisionBoxesToList(pos, met));
            }
            return false;
        }

        /// <summary>
        /// Значение для разрушения в тактах
        /// </summary>
        //public virtual int GetDamageValue() => 0;

       // public float GetBlockHardness()

        /// <summary>
        /// Получите твердость этого блока относительно способности данного игрока
        /// Тактов чтоб сломать
        /// </summary>
        public int GetPlayerRelativeBlockHardness(EntityPlayer playerIn, BlockState blockState)
        {
            if (playerIn.IsCreativeMode)
            {
                // креатив
                return 0;
            }
            // выживание
            int hardness = Hardness(blockState);
            if (hardness == 0) return 0;
            return playerIn.ToolForcePerBlock(blockState.GetBlock(), hardness);
        }

        /// <summary>
        /// Блок не прозрачный, для расчёта освещения
        /// </summary>
        public bool IsNotTransparent() => LightOpacity > 13;

        /// <summary>
        /// Указывает, является ли материал полупрозрачным, от материала
        /// </summary>
        //public bool IsTranslucent() => Material == EnumMaterial.Air || Material == EnumMaterial.Glass 
        //    || Material == EnumMaterial.Water || Material == EnumMaterial.Debug;

        /// <summary>
        /// Создает данный ItemStack как EntityItem в мире в заданной позиции 
        /// </summary>
        /// <param name="worldIn"></param>
        /// <param name="pos"></param>
        /// <param name="itemStack"></param>
        public static void SpawnAsEntity(WorldBase worldIn, BlockPos blockPos, ItemStack itemStack)
        {
            if (!worldIn.IsRemote)
            {
                vec3 pos = new vec3(
                    blockPos.X + worldIn.Rnd.NextFloat() * .5f + .25f,
                    blockPos.Y + worldIn.Rnd.NextFloat() * .5f + .25f,
                    blockPos.Z + worldIn.Rnd.NextFloat() * .5f + .25f
                );
                EntityItem entityItem = new EntityItem(worldIn, pos, itemStack);
                entityItem.SetDefaultPickupDelay();
                worldIn.SpawnEntityInWorld(entityItem);
            }
        }

        #region Drop

        /// <summary>
        /// Спавн предмета при разрушении этого блока
        /// </summary>
        public void DropBlockAsItem(WorldBase worldIn, BlockPos blockPos, BlockState state, ItemAbTool itemTool = null)
        {
            if (!worldIn.IsRemote && canDropPresent)
            {
                ItemBase item;
                int i, count;
                // Основной
                count = QuantityDroppedWithBonus(itemTool, worldIn.Rnd);
                for (i = 0; i < count; i++)
                {
                    item = GetItemDropped(state, worldIn.Rnd, itemTool);
                    if (item != null)
                    {
                        SpawnAsEntity(worldIn, blockPos, new ItemStack(item, 1, DamageDropped(state)));
                    }
                }
                // Дополнительный
                count = QuantityDroppedWithBonusAdditional(itemTool, worldIn.Rnd);
                for (i = 0; i < count; i++)
                {
                    item = GetItemDroppedAdditional(state, worldIn.Rnd, itemTool);
                    if (item != null)
                    {
                        SpawnAsEntity(worldIn, blockPos, new ItemStack(item, 1, DamageDroppedAdditional(state)));
                    }
                }
            }
        }

        /// <summary>
        /// Спавн предмета при разрушении этого блока, используется для взрыва
        /// </summary>
        /// <param name="chance">Вероятность выпадении предмета 1.0 всегда, 0.0 никогда</param>
        public void DropBlockAsItemWithChance(WorldBase worldIn, BlockPos blockPos, BlockState state, float chance)
        {
            if (!worldIn.IsRemote && canDropPresent)
            {
                ItemBase item;
                int i, count;
                // Основной
                count = QuantityDroppedWithBonus(null, worldIn.Rnd);
                for (i = 0; i < count; i++)
                {
                    if (chance == 1 || worldIn.Rnd.NextFloat() <= chance)
                    {
                        item = GetItemDropped(state, worldIn.Rnd, null);
                        if (item != null)
                        {
                            SpawnAsEntity(worldIn, blockPos, new ItemStack(item, 1, DamageDropped(state)));
                        }
                    }
                }
                // Дополнительный
                count = QuantityDroppedWithBonusAdditional(null, worldIn.Rnd);
                for (i = 0; i < count; i++)
                {
                    if (chance == 1 || worldIn.Rnd.NextFloat() <= chance)
                    {
                        item = GetItemDroppedAdditional(state, worldIn.Rnd, null);
                        if (item != null)
                        {
                            SpawnAsEntity(worldIn, blockPos, new ItemStack(item, 1, DamageDroppedAdditional(state)));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Получите предмет, который должен выпасть из этого блока при сборе.
        /// </summary>
        protected virtual ItemBase GetItemDropped(BlockState state, Rand rand, ItemAbTool itemTool) => Items.GetItemCache(state.id);
        /// <summary>
        /// Возвращает количество предметов, которые выпадают при разрушении блока.
        /// </summary>
        protected virtual int QuantityDropped(Rand random) => 1;
        /// <summary>
        /// Получите количество выпавших на основе данного уровня удачи
        /// </summary>
        protected virtual int QuantityDroppedWithBonus(ItemAbTool itemTool, Rand random) => QuantityDropped(random);
        /// <summary>
        /// Получите значение урона, которое должен упасть этот блок
        /// </summary>
        protected virtual int DamageDropped(BlockState state) => 0;
        /// <summary>
        /// Получите ДОПОЛНИТЕЛЬНЫЙ предмет, который должен выпасть из этого блока при сборе.
        /// </summary>
        protected virtual ItemBase GetItemDroppedAdditional(BlockState state, Rand rand, ItemAbTool itemTool) => null;
        /// <summary>
        /// Возвращает количество ДОПОЛНИТЕЬНЫХ предметов, которые выпадают при разрушении блока.
        /// </summary>
        protected virtual int QuantityDroppedAdditional(Rand random) => 0;
        /// <summary>
        /// Получите количество ДОПОЛНИТЕЛЬНЫХ предметов выпавших на основе данного уровня удачи
        /// </summary>
        protected virtual int QuantityDroppedWithBonusAdditional(ItemAbTool itemTool, Rand random) => QuantityDroppedAdditional(random);
        /// <summary>
        /// Получите значение урона, которое должен упасть этот ДОПОЛНИТЕЛЬНЫЙ блок
        /// </summary>
        protected virtual int DamageDroppedAdditional(BlockState state) => 0;

        #endregion

        /// <summary>
        /// Сколько ударов требуется, чтобы сломать блок в тактах (20 тактов = 1 секунда)
        /// </summary>
        public virtual int Hardness(BlockState state) => 0;

        /// <summary>
        /// Действие перед размещеннием блока, для определения метданных
        /// </summary>
        public virtual BlockState OnBlockPlaced(WorldBase worldIn, BlockPos blockPos, BlockState state, Pole side, vec3 facing) 
            => state.NewMet(0);

        /// <summary>
        /// Действие блока после его установки
        /// </summary>
        public virtual void OnBlockAdded(WorldBase worldIn, BlockPos blockPos, BlockState state) { }

        /// <summary>
        /// Действие блока после его удаления
        /// </summary>
        public virtual void OnBreakBlock(WorldBase worldIn, BlockPos blockPos, BlockState state) { }

        /// <summary>
        /// Активация блока, клик правой клавишей мыши по блоку, true - был клик, false - нет такой возможности
        /// </summary>
        public virtual bool OnBlockActivated(WorldBase worldIn, EntityPlayer entityPlayer, BlockPos pos, BlockState state, Pole side, vec3 facing) => false;

        /// <summary> 
        /// Проверка установи блока, можно ли его установить тут
        /// </summary>
        public virtual bool CanBlockStay(WorldBase worldIn, BlockPos blockPos, int met = 0) => true;

        /// <summary>
        /// Семпл сломанного блока
        /// </summary>
        public AssetsSample SampleBreak(WorldBase worldIn) => samplesBreak[worldIn.Rnd.Next(samplesBreak.Length)];
        /// <summary>
        /// Семпл установленного блока
        /// </summary>
        public AssetsSample SamplePut(WorldBase worldIn) => samplesPut[worldIn.Rnd.Next(samplesPut.Length)];
        /// <summary>
        /// Семпл хотьбы по блоку
        /// </summary>
        public AssetsSample SampleStep(WorldBase worldIn) => samplesStep[worldIn.Rnd.Next(samplesStep.Length)];
        /// <summary>
        /// Тон сэмпла сломанного блока,
        /// </summary>
        public virtual float SampleBreakPitch(Rand random) => 1f;
        /// <summary>
        /// Проиграть звук поломки
        /// </summary>
        public void PlaySoundBreak(WorldBase worldIn, BlockPos blockPos) => worldIn.PlaySound(SampleBreak(worldIn), blockPos.ToVec3(), 1f, SampleBreakPitch(worldIn.Rnd));

        /// <summary>
        /// Есть ли звуковой эффект шага
        /// </summary>
        public bool IsSampleStep() => samplesStep.Length > 0;

        /// <summary>
        /// Случайный эффект частички и/или звука на блоке только для клиента
        /// </summary>
        public virtual void RandomDisplayTick(WorldBase world, BlockPos blockPos, BlockState blockState, Rand random) { }

        /// <summary>
        /// Случайный эффект блока, для сервера
        /// </summary>
        public virtual void RandomTick(WorldBase world, BlockPos blockPos, BlockState blockState, Rand random)
            => UpdateTick(world, blockPos, blockState, random);

        /// <summary>
        /// Обновить блок в такте
        /// </summary>
        public virtual void UpdateTick(WorldBase world, BlockPos blockPos, BlockState blockState, Rand random) { }

        /// <summary>
        /// Разрушается ли блок от жидкости
        /// </summary>
        public virtual bool IsLiquidDestruction() => false;

        /// <summary>
        /// Является ли блок проходимым, т.е. можно ли ходить через него
        /// </summary>
        public virtual bool IsPassable(int met) => false;

        /// <summary>
        /// Является ли блок проходимым на нём, т.е. можно ли ходить по нему
        /// </summary>
        public virtual bool IsPassableOnIt(int met) => !IsPassable(met);

        /// <summary>
        /// Смена соседнего блока
        /// </summary>
        public virtual void NeighborBlockChange(WorldBase worldIn, BlockPos blockPos, BlockState neighborState, BlockBase neighborBlock) { }

        /// <summary>
        /// Вызывается при столкновении объекта с блоком
        /// </summary>
        public virtual void OnEntityCollidedWithBlock(WorldBase worldIn, BlockPos pos, BlockState state, EntityBase entityIn) { }
        /// <summary>
        /// Изменить ускорение на блоке
        /// </summary>
        public virtual vec3 ModifyAcceleration(WorldBase worldIn, BlockPos blockPos, vec3 motion) => motion;

        /// <summary>
        /// Блок подключен к электроэнергии
        /// </summary>
        public virtual void UnitConnectedToElectricity(WorldBase world, BlockPos blockPos) { }
        /// <summary>
        /// Блок отключен от электроэнергии
        /// </summary>
        public virtual void UnitDisconnectedFromElectricity(WorldBase world, BlockPos blockPos) { }
        /// <summary>
        /// Имеется ли у блока подключение к электроэнергии
        /// </summary>
        public virtual bool IsUnitConnectedToElectricity() => false;
        /// <summary>
        /// Имеется ли у блока разрядка к электроэнергии
        /// </summary>
        public virtual bool IsUnitDischargeToElectricity() => false;
        /// <summary>
        /// Блок который замедляет сущность в перемещении на ~30%
        /// </summary>
        public virtual bool IsSlow(BlockState state) => false;

        /// <summary>
        /// Задать рецепт
        /// </summary>
        public BlockBase SetCraftRecipe(int time, params Element[] recipe)
        {
            Craft.SetRecipe(time, recipe);
            return this;
        }
        /// <summary>
        /// Задать рецепт
        /// </summary>
        public BlockBase SetCraftRecipe(int amount, int time, params Element[] recipe)
        {
            Craft.SetRecipe(amount, time, recipe);
            return this;
        }

        /// <summary>
        /// Строка
        /// </summary>
        public override string ToString() => EBlock.ToString();
    }
}
