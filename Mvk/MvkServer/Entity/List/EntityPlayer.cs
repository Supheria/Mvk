using MvkServer.Glm;
using MvkServer.Inventory;
using MvkServer.Item;
using MvkServer.NBT;
using MvkServer.Sound;
using MvkServer.World;
using MvkServer.World.Block;

namespace MvkServer.Entity.List
{
    /// <summary>
    /// Сущность игрока
    /// </summary>
    public abstract class EntityPlayer : EntityLiving
    {
        /// <summary>
        /// Уникальный id
        /// </summary>
        public string UUID { get; protected set; }
        /// <summary>
        /// Обзор чанков
        /// </summary>
        public int OverviewChunk { get; protected set; } = 0;// MvkGlobal.OVERVIEW_CHUNK_START;
        /// <summary>
        /// Обзор чанков прошлого такта
        /// </summary>
        public int OverviewChunkPrev { get; protected set; } = 0;// MvkGlobal.OVERVIEW_CHUNK_START;
        /// <summary>
        /// Массив по длинам используя квадратный корень для всей видимости
        /// </summary>
        public vec2i[] DistSqrt { get; protected set; }
        /// <summary>
        /// В каком чанке было обработка чанков
        /// </summary>
        public vec2i ChunkPosManaged { get; protected set; } = new vec2i();
        /// <summary>
        /// Инвенарь игрока
        /// </summary>
        public InventoryPlayer Inventory { get; protected set; }
        /// <summary>
        /// Творческий режим, бесконечный инвентарь, и всё ломаем за один клик
        /// </summary>
        public bool IsCreativeMode { get; protected set; } = false;
        /// <summary>
        /// Разрешается ли летать
        /// </summary>
        public bool AllowFlying { get; protected set; } = false;
        /// <summary>
        /// Отключить урон
        /// </summary>
        public bool DisableDamage { get; protected set; } = false;

        /// <summary>
        /// Тикущий уровень игрока
        /// </summary>
        public int ExperienceLevel { get; protected set; } = 0;
        /// <summary>
        /// Общее количество опыта игрока. Это также включает в себя количество опыта в их полосе опыта.
        /// </summary>
        public int ExperienceTotal { get; protected set; } = 0;
        /// <summary>
        /// Текущее количество опыта, которое игрок имеет на своей полосе опыта.
        /// </summary>
        public float Experience { get; protected set; } = 0;

        /// <summary>
        /// Это предмет, который используется, когда игрок удерживает кнопку useItemButton (например, лук, еда, меч).
        /// </summary>
        public ItemStack ItemInUse { get; protected set; }
        /// <summary>
        /// Это поле начинается равным GetMaxItemUseDuration и уменьшается на каждый тик.
        /// </summary>
        public int ItemInUseCount { get; protected set; }
        

        protected EntityPlayer(WorldBase world) : base(world)
        {
            type = EnumEntities.Player;
            StepHeight = 1.2f;
            Inventory = new InventoryPlayer(this);
            previousEquipment = new ItemStack[InventoryPlayer.COUNT_ARMOR + 1];
            samplesHurt = new AssetsSample[] { AssetsSample.DamageHit1, AssetsSample.DamageHit2, AssetsSample.DamageHit3 };

            // TODO::2022-03-29 Временно предметы при старте у игрока
            if (!world.IsRemote)
            {
                //Inventory.SetInventorySlotContents(0, new ItemStack(Blocks.GetBlockCache(EnumBlock.Water), 64));
                //Inventory.SetInventorySlotContents(1, new ItemStack(Blocks.GetBlockCache(EnumBlock.PlanksOak), 64));

                //Inventory.SetInventorySlotContents(2, new ItemStack(Blocks.GetBlockCache(EnumBlock.SaplingOak), 16));
                //Inventory.SetInventorySlotContents(3, new ItemStack(Blocks.GetBlockCache(EnumBlock.SaplingFruit), 16));
                //Inventory.SetInventorySlotContents(4, new ItemStack(Blocks.GetBlockCache(EnumBlock.SaplingOak), 16));
                //Inventory.SetInventorySlotContents(5, new ItemStack(Blocks.GetBlockCache(EnumBlock.SaplingPalm), 16));
                //Inventory.SetInventorySlotContents(6, new ItemStack(Blocks.GetBlockCache(EnumBlock.SaplingSpruce), 16));
                //Inventory.SetInventorySlotContents(3, new ItemStack(Blocks.GetBlockCache(EnumBlock.LogOak), 64));
                //Inventory.SetInventorySlotContents(4, new ItemStack(Blocks.GetBlockCache(EnumBlock.Turf), 64));
                ////Inventory.SetInventorySlotContents(4, new ItemStack(Blocks.GetBlockCache(EnumBlock.Glass), 16));
                //Inventory.SetInventorySlotContents(5, new ItemStack(Blocks.GetBlockCache(EnumBlock.LeavesOak), 16));
                ////Inventory.SetInventorySlotContents(5, new ItemStack(Blocks.GetBlockCache(EnumBlock.Debug), 16));
                //Inventory.SetInventorySlotContents(6, new ItemStack(Blocks.GetBlockCache(EnumBlock.Brol), 64));
                //Inventory.SetInventorySlotContents(7, new ItemStack(Blocks.GetBlockCache(EnumBlock.Fire), 64));

                // Inventory.SetInventorySlotContents(5, new ItemStack(Blocks.GetBlockCache(EnumBlock.Sand), 16));
                //  Inventory.SetInventorySlotContents(7, new ItemStack(Blocks.GetBlockCache(EnumBlock.Cactus), 16));

                //Inventory.SetInventorySlotContents(0, new ItemStack(Blocks.GetBlockCache(48), 16));
                //Inventory.SetInventorySlotContents(1, new ItemStack(Blocks.GetBlockCache(49), 16));
                //Inventory.SetInventorySlotContents(2, new ItemStack(Blocks.GetBlockCache(50), 16));
                //Inventory.SetInventorySlotContents(3, new ItemStack(Blocks.GetBlockCache(51), 16));
                //Inventory.SetInventorySlotContents(4, new ItemStack(Blocks.GetBlockCache(52), 16));
                //Inventory.SetInventorySlotContents(5, new ItemStack(Blocks.GetBlockCache(53), 16));
                //Inventory.SetInventorySlotContents(6, new ItemStack(Blocks.GetBlockCache(4), 16));
                //Inventory.SetInventorySlotContents(7, new ItemStack(Blocks.GetBlockCache(9), 16));

            }

        }

        /// <summary>
        /// Тип сущности
        /// </summary>
        public override EnumEntities GetEntityType() => IsInvisible() ? EnumEntities.PlayerInvisible : EnumEntities.Player;

        /// <summary>
        /// Обновление действия предмета, надо засунуть в такт клиента и сервере
        /// </summary>
        protected void UpdateItemInUse()
        {
            if (ItemInUse != null)
            {
                ItemStack stack = Inventory.GetCurrentItem();
                if (stack == ItemInUse)
                {
                    if (ItemInUseCount <= stack.GetMaxItemUseDuration() - 6 && ItemInUseCount % 4 == 0)
                    {
                        UpdateItemUse(stack, 5);
                    }
                    if (--ItemInUseCount == 0 && !World.IsRemote)
                    {
                        OnItemUseFinish();
                    }
                }
                else
                {
                    ClearItemInUse();
                }
            }
        }

        /// <summary>
        /// Максимальное значение здоровья сущности
        /// </summary>
        protected override float GetHelathMax() => 16;

        //public override void Update()
        //{
        //    base.Update();
        //}
        //protected override void LivingUpdate()
        //{
        //    base.LivingUpdate();
        //}

        /// <summary>
        /// Задать обзор чанков у клиента
        /// </summary>
        public virtual void SetOverviewChunk(int overviewChunk) => OverviewChunk = overviewChunk;

        /// <summary>
        /// Равны ли обзоры чанков между тактами
        /// </summary>
        public bool SameOverviewChunkPrev() => OverviewChunk == OverviewChunkPrev;

        /// <summary>
        /// Обновить перспективу камеры
        /// </summary>
        public virtual void UpProjection() { }

        /// <summary>
        /// Задать чанк обработки
        /// </summary>
        public void SetChunkPosManaged(vec2i pos) => ChunkPosManaged = pos;

        /// <summary>
        /// Возвращает элемент, который держит в руке
        /// </summary>
        public override ItemStack GetHeldItem() => Inventory.GetCurrentItem();

        /// <summary>
        /// Получить стак что в правой руке 0 или броня 1-4 (InventoryPlayer.COUNT_ARMOR)
        /// </summary>
        public override ItemStack GetEquipmentInSlot(int slot) 
            => slot == 0 ? Inventory.GetCurrentItem() : Inventory.GetArmorInventory(slot - 1);

        /// <summary>
        /// Получить слот брони 0-3 InventoryPlayer.COUNT_ARMOR
        /// </summary>
        public override ItemStack GetCurrentArmor(int slot) => Inventory.GetArmorInventory(slot);

        /// <summary>
        /// Задать стак в слот, что в правой руке 0, или 1-4 слот брони InventoryPlayer.COUNT_ARMOR
        /// </summary>
        public override void SetCurrentItemOrArmor(int slot, ItemStack itemStack)
        {
            if (slot == 0)
            {
                Inventory.SetCurrentItem(itemStack);
            }
            else
            {
                Inventory.SetArmorInventory(slot - 1, itemStack);
            }
        }

        /// <summary>
        /// Добавить очки опыта игроку
        /// </summary>
        public void AddExperience(int experience)
        {
            //this.addScore(experience);
            int i = int.MaxValue - ExperienceTotal;
            if (experience > i) experience = i;

            Experience += (float)experience / (float)XpBarCap();

            for (ExperienceTotal += experience; Experience >= 1f; Experience /= (float)XpBarCap())
            {
                Experience = (Experience - 1.0F) * (float)XpBarCap();
                AddExperienceLevel(1);
            }
        }

        /// <summary>
        /// Использование уровня игрока
        /// </summary>
        public void UseExperienceLevel(int experienceLevel)
        {
            ExperienceLevel -= experienceLevel;

            if (ExperienceLevel < 0)
            {
                ExperienceLevel = 0;
                Experience = 0f;
                ExperienceTotal = 0;
            }
        }

        /// <summary>
        /// Добавить уровень игроку
        /// </summary>
        public void AddExperienceLevel(int experienceLevel)
        {
            ExperienceLevel += experienceLevel;

            if (ExperienceLevel < 0)
            {
                ExperienceLevel = 0;
                Experience = 0f;
                ExperienceTotal = 0;
            }

            //if (experienceLevel > 0 && ExperienceLevel % 5 == 0 && (float)this.field_82249_h < (float)this.ticksExisted - 100.0F)
            //{
            //    // Звуковой эффект каждый 5 уровень с интервалом 100 тактов
            //    float var2 = ExperienceLevel > 30 ? 1.0F : (float)ExperienceLevel / 30.0F;
            //    this.worldObj.playSoundAtEntity(this, "random.levelup", var2 * 0.75F, 1.0F);
            //    this.field_82249_h = this.ticksExisted;
            //}
        }

        /// <summary>
        /// Этот метод возвращает максимальное количество опыта, которое может содержать полоса опыта.
        /// С каждым уровнем предел опыта на шкале опыта игрока увеличивается на 10.
        /// </summary>
        public int XpBarCap() 
            => ExperienceLevel >= 30 ? 112 + (ExperienceLevel - 30) * 9 : (ExperienceLevel >= 15 ? 37 + (ExperienceLevel - 15) * 5 : 7 + ExperienceLevel * 2);

        /// <summary>
        /// Есть ли звуковой эффект шага
        /// </summary>
        public override bool IsSampleStep(BlockBase blockDown) => IsInvisible() ? false : blockDown.IsSampleStep();

        /// <summary>
        /// Семпл хотьбы
        /// </summary>
        public override AssetsSample SampleStep(WorldBase worldIn, BlockBase blockDown) => blockDown.SampleStep(worldIn);

        /// <summary>
        /// Имеется ли у сущности иммунитет на всё
        /// </summary>
        protected override bool IsImmuneToAll() => DisableDamage;

        /// <summary>
        /// Дропнуть предмет от сущности
        /// </summary>
        /// <param name="itemStack">Стак предмета</param>
        /// <param name="inFrontOf">Флаг перед собой</param>
        /// <param name="longAway">Далеко бросить от себя</param>
        public void DropItem(ItemStack itemStack, bool inFrontOf, bool longAway = false)
        {
            vec3 motion;
            if (inFrontOf)
            {
                if (longAway)
                {
                    float pitchxz = glm.cos(RotationPitch);
                    motion = new vec3(
                        glm.sin(RotationYawHead) * pitchxz * 1.4f,
                        glm.sin(RotationPitch) * 1.4f,
                        -glm.cos(RotationYawHead) * pitchxz * 1.4f
                    );
                    float f1 = rand.NextFloat() * .02f;
                    float f2 = rand.NextFloat() * glm.pi360;
                    motion.x += glm.cos(f2) * f1;
                    motion.z += glm.sin(f2) * f1;
                }
                else
                {
                    float pitchxz = glm.cos(RotationPitch);
                    motion = new vec3(
                        glm.sin(RotationYawHead) * pitchxz * .3f,
                        glm.sin(RotationPitch) * .3f + .1f,
                        -glm.cos(RotationYawHead) * pitchxz * .3f
                    );
                    float f1 = rand.NextFloat() * .02f;
                    float f2 = rand.NextFloat() * glm.pi360;
                    motion.x += glm.cos(f2) * f1;
                    motion.y = (rand.NextFloat() - rand.NextFloat()) * .1f;
                    motion.z += glm.sin(f2) * f1;
                }
            }
            else
            {
                float f1 = rand.NextFloat() * .5f;
                float f2 = rand.NextFloat() * glm.pi360;
                motion = new vec3(-glm.sin(f2) * f1, .2f, glm.cos(f2) * f1);
            }

            vec3 pos = Position;
            if (longAway)
            {
                pos.x += glm.cos(RotationYawHead) * .32f;
                pos.z += glm.sin(RotationYawHead) * .32f;
                pos.y += GetEyeHeight() - .2f;
            }
            else
            {
                pos.y += GetEyeHeight() / 2f;
            }
            EntityItem entityItem = new EntityItem(World, pos, itemStack);
            entityItem.SetPickupDelay(40);
            entityItem.SetMotion(motion);
            World.SpawnEntityInWorld(entityItem);
        }

        #region Food

        /// <summary>
        /// Может ли сущность есть
        /// </summary>
        /// <param name="alwaysEdible">Если это поле истинно, еду можно есть, даже если игроку не нужно есть.</param>
        public bool CanEat(bool alwaysEdible)
        {
            return GetHealth() < GetHelathMax();// (alwaysEdible || this.foodStats.needFood()) && !this.capabilities.disableDamage;
        }

        /// <summary>
        /// Очистить itemInUse
        /// </summary>
        public void ClearItemInUse()
        {
            ItemInUse = null;
            ItemInUseCount = 0;
            if (!World.IsRemote) SetEating(false);
        }

        /// <summary>
        /// Устанавливает itemInUse при нажатии кнопки использования элемента
        /// </summary>
        public void SetItemInUse(ItemStack itemstack, int maxItemUseDuration)
        {
           // if (!World.IsRemote)
            {
                if (itemstack != ItemInUse)
                {
                    ItemInUse = itemstack;
                    ItemInUseCount = maxItemUseDuration;
                    if (!World.IsRemote) SetEating(true);
                }
            }
        }

        /// <summary>
        /// Проверяет, использует ли объект в данный момент предмет (например, лук, еду, меч),
        /// удерживая нажатой кнопку useItemButton.
        /// </summary>
        public bool IsUsingItem() => ItemInUse != null;

        /// <summary>
        /// Получает продолжительность того, как долго используется текущий itemInUse
        /// </summary>
        public int GetItemInUseDuration() => IsUsingItem() ? ItemInUse.GetMaxItemUseDuration() - ItemInUseCount : 0;

        /// <summary>
        /// Воспроизводит звуки и создает частицы для предмета в состоянии использования
        /// </summary>
        /// <param name="count">Количество частичек</param>
        protected void UpdateItemUse(ItemStack itemStackIn, int count)
        {
            if (itemStackIn.GetItemUseAction() == EnumItemAction.Drink)
            {
                PlaySound(World.SampleSoundDrink(), .5f, World.Rnd.NextFloat() * .1f + .9f);
            }
            else if (itemStackIn.GetItemUseAction() == EnumItemAction.Eat)
            {
                vec3 pos = Position;
                pos.y += GetEyeHeight() - .2f;

                float pitchxz = glm.cos(RotationPitch);
                vec3 motion = new vec3(
                    glm.sin(RotationYawHead) * pitchxz,
                    glm.sin(RotationPitch),
                    -glm.cos(RotationYawHead) * pitchxz
                );
                pos += motion.normalize() * Width;
                World.SpawnParticle(EnumParticle.ItemPart, count, pos, new vec3(Width), 0, (int)itemStackIn.Item.EItem);
                PlaySound(World.SampleSoundEat(), .5f + .5f * World.Rnd.Next(2), (World.Rnd.NextFloat() - World.Rnd.NextFloat()) * .2f + 1f);
            }
        }

        #endregion

        /// <summary>
        /// Может дышать под водой
        /// </summary>
        public override bool CanBreatheUnderwater() => DisableDamage ? true : false;

        /// <summary>
        /// Перестаём летать
        /// </summary>
        public override void NotFlying()
        {
            if (IsFlying) IsFlying = false;
        }

        /// <summary>
        /// Остоновить юз предмета
        /// </summary>
        public void StopUsingItem()
        {
            if (ItemInUse != null)
            {
                ItemInUse.OnPlayerStoppedUsing(World, this, ItemInUseCount);
            }
            ClearItemInUse();
        }

        protected virtual void OnItemUseFinish()
        {
            if (ItemInUse != null)
            {
                UpdateItemUse(ItemInUse, 16);
                int amount = ItemInUse.Amount;
                ItemStack stack = ItemInUse.OnItemUseFinish(World, this);

                if (stack != ItemInUse || stack != null && stack.Amount != amount)
                {
                    Inventory.SetCurrentItem(stack.Amount == 0 ? null : stack);
                    Inventory.SendSetSlotPlayer();
                }

                ClearItemInUse();
            }
        }

        /// <summary>
        /// Возвращает звук, издаваемый этим мобом при смерти
        /// </summary>
        protected override AssetsSample GetDeathSound() => AssetsSample.None;

        /// <summary>
        /// Имеются ли ограничения по скорости, в воде и на блоках
        /// </summary>
        protected override bool IsSpeed​​Limit() => !IsFlying;
        /// <summary>
        /// Имеются ли звук шага
        /// </summary>
        protected override bool IsSoundStep() => !IsFlying;

        /// <summary>
        /// Возвращает истину, если другие Сущности не должны проходить через эту Сущность
        /// </summary>
        public override bool CanBeCollidedWith() => IsInvisible() ? false : base.CanBeCollidedWith();
        /// <summary>
        /// Возвращает true, если этот объект должен толкать и толкать другие объекты при столкновении
        /// </summary>
        public override bool CanBePushed() => IsInvisible() ? false : base.CanBePushed();

        /// <summary>
        /// Сила инструмента на блок
        /// </summary>
        /// <param name="hardness">твёрдость блока, выше 0</param>
        /// <returns>вёрнём новое значение твёрдости блока</returns>
        public int ToolForcePerBlock(BlockBase block, int hardness)
        {
            float svb = Inventory.GetStrVsBlock(block);
            // Если предмет не выпадает этим инструментом, то двёрдость увеличиваем в 3 раза
            if (!Inventory.CanHarvestBlock(block)) hardness *= 3;
            return (int)(hardness / svb);
        }

        /// <summary>
        /// Время паузы между разрушениями блоков в тактах
        /// </summary>
        public override int PauseTimeBetweenBlockDestruction() 
            => IsCreativeMode ? 6 : Inventory.PauseTimeBetweenBlockDestruction();

        /// <summary>
        /// Вернуть тип действия предмета
        /// </summary>
        public override EnumItemAction GetItemUseAction()
        {
            ItemStack itemStack = Inventory.GetCurrentItem();
            return itemStack != null ? itemStack.GetItemUseAction() : EnumItemAction.None;
        }

        /// <summary>
        /// Либо запишите эту сущность в указанный тег NBT и верните true, либо верните false, ничего не делая.
        /// Если это возвращает false объект не сохраняется на диске.
        /// </summary>
        public override bool WriteEntityToNBToptional(TagCompound nbt) => false;

        public override void WriteEntityToNBT(TagCompound nbt)
        {
            base.WriteEntityToNBT(nbt);
            nbt.SetBool("IsCreativeMode", IsCreativeMode);
            nbt.SetBool("NoClip", NoClip);
            nbt.SetBool("AllowFlying", AllowFlying);
            nbt.SetBool("DisableDamage", DisableDamage);
            nbt.SetBool("Invisible", IsInvisible());
            TagList list = new TagList();
            Inventory.WriteToNBT(list);
            nbt.SetTag("Inventory", list);
        }

        public override void ReadEntityFromNBT(TagCompound nbt)
        {
            base.ReadEntityFromNBT(nbt);
            IsCreativeMode = nbt.GetBool("IsCreativeMode");
            NoClip = nbt.GetBool("NoClip");
            AllowFlying = nbt.GetBool("AllowFlying");
            DisableDamage = nbt.GetBool("DisableDamage");
            SetInvisible(nbt.GetBool("Invisible"));
            Inventory.ReadFromNBT(nbt.GetTagList("Inventory", 10));
        }
    }
}
