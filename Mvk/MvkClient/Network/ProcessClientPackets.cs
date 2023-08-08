using MvkClient.Entity;
using MvkClient.Setitings;
using MvkClient.Util;
using MvkServer.Entity;
using MvkServer.Entity.List;
using MvkServer.Network;
using MvkServer.Network.Packets.Client;
using MvkServer.Network.Packets.Server;
using MvkServer.Sound;
using MvkServer.Util;
using System.Collections;

namespace MvkClient.Network
{
    /// <summary>
    /// Обработка клиентсиких пакетов для сервером
    /// </summary>
    public class ProcessClientPackets : ProcessPackets
    {
        /// <summary>
        /// Основной клиент
        /// </summary>
        public Client ClientMain { get; private set; }
        /// <summary>
        /// Массив очередей пакетов
        /// </summary>
        private DoubleList<IPacket> packets = new DoubleList<IPacket>();

        public ProcessClientPackets(Client client) : base(true) => ClientMain = client;

        /// <summary>
        /// Передача данных для клиента
        /// </summary>
        public void ReceiveBuffer(byte[] buffer) => ReceivePacket(null, buffer);

        protected override void ReceivePacketClient(IPacket packet, int light)
        {
            Debug.Traffic += light;
            // TODO::2022-11-30 проверить без пакета по сети, желательно на плохой связи
            //Task.Factory.StartNew(() =>
            //{
                byte id = GetId(packet);
                if (id == 0xF0)
                {
                    // Мира ещё нет, он в стадии создании, первый старт первого игрока
                    HandleF0Connection((PacketSF0Connection)packet);
                }
                else if(ClientMain.World != null)
                {
                    if (id == 0x00)
                    {
                        Handle00Pong((PacketS00Pong)packet);
                    }
                    else if (id == 0x02)
                    {
                        // Хоть мир уже и есть, но для первого игрока, но он ещё не запустил игровой такт
                        Handle02JoinGame((PacketS02JoinGame)packet);
                    }
                    else
                    {
                        // Мир есть, заносим в пакет с двойным буфером, для обработки в такте
                        packets.Add(packet);
                    }
                }
            //});
        }

        private void UpdateReceivePacketClient(IPacket packet)
        {
            byte id = GetId(packet);
            //if (id != 0x21) ClientMain.World.Log.Log("P:{0:X}", id);
            switch (id)
            {
                case 0x01: Handle01KeepAlive((PacketS01KeepAlive)packet); break;
                case 0x03: Handle03TimeUpdate((PacketS03TimeUpdate)packet); break;
                case 0x04: Handle04EntityEquipment((PacketS04EntityEquipment)packet); break;
                //case 0x06: Handle06UpdateHealth((PacketS06UpdateHealth)packet); break;
                case 0x07: Handle07Respawn((PacketS07Respawn)packet); break;
                case 0x08: Handle08PlayerPosLook((PacketS08PlayerPosLook)packet); break;
                case 0x0B: Handle0BAnimation((PacketS0BAnimation)packet); break;
                case 0x0C: Handle0CSpawnPlayer((PacketS0CSpawnPlayer)packet); break;
                case 0x0D: Handle0DCollectItem((PacketS0DCollectItem)packet); break;
                //case 0x0E: Handle0ESpawnItem((PacketS0ESpawnItem)packet); break;
                case 0x0F: Handle0FSpawnMob((PacketS0FSpawnMob)packet); break;
                case 0x12: Handle12EntityVelocity((PacketS12EntityVelocity)packet); break;
                case 0x13: Handle13DestroyEntities((PacketS13DestroyEntities)packet); break;
                case 0x14: Handle14EntityMotion((PacketS14EntityMotion)packet); break;
                case 0x19: Handle19EntityStatus((PacketS19EntityStatus)packet); break;
                case 0x1C: Handle1CEntityMetadata((PacketS1CEntityMetadata)packet); break;
                case 0x21: Handle21ChunkData((PacketS21ChunkData)packet); break;
                case 0x22: Handle22MultiBlockChange((PacketS22MultiBlockChange)packet); break;
                case 0x23: Handle23BlockChange((PacketS23BlockChange)packet); break;
                case 0x25: Handle25BlockBreakAnim((PacketS25BlockBreakAnim)packet); break;
                case 0x27: Handle27Explosion((PacketS27Explosion)packet); break;
                case 0x29: Handle29SoundEffect((PacketS29SoundEffect)packet); break;
                case 0x2A: Handle2AParticles((PacketS2AParticles)packet); break;
                case 0x2D: Handle2DOpenWindow((PacketS2DOpenWindow)packet); break;
                case 0x2F: Handle2FSetSlot((PacketS2FSetSlot)packet); break;
                case 0x30: Handle30WindowItems((PacketS30WindowItems)packet); break;
                case 0x31: Handle31WindowProperty((PacketS31WindowProperty)packet); break;
                case 0x39: Handle39PlayerAbilities((PacketS39PlayerAbilities)packet); break;
                case 0x3A: Handle3AMessage((PacketS3AMessage)packet); break;

                case 0xF1: HandleF1Disconnect((PacketSF1Disconnect)packet); break;
            }
        }

        public void Clear() => packets.Clear();

        /// <summary>
        /// Игровой такт клиента
        /// </summary>
        public void Update()
        {
            packets.Step();
            int count = packets.CountBackward;
            for (int i = 0; i < count; i++)
            {
                UpdateReceivePacketClient(packets.GetNext());
            }
        }

        /// <summary>
        /// Пакет связи
        /// </summary>
        private void Handle00Pong(PacketS00Pong packet) => ClientMain.SetPing(packet.GetClientTime());

        /// <summary>
        /// KeepAlive
        /// </summary>
        private void Handle01KeepAlive(PacketS01KeepAlive packet) => ClientMain.TrancivePacket(new PacketC01KeepAlive(packet.GetTime()));

        /// <summary>
        /// Пакет соединения с сервером
        /// </summary>
        private void Handle02JoinGame(PacketS02JoinGame packet)
        {
            ClientMain.Player.SetDataPlayer(packet.GetId(), packet.GetUuid(), ClientMain.ToNikname());
            ClientMain.GameModeBegin();
            // отправляем настройки
            ClientMain.TrancivePacket(new PacketC15ClientSetting(Setting.OverviewChunk));

            //EntityChicken entityChicken = new EntityChicken(ClientMain.World);
            //entityChicken.SetEntityId(101);
            //entityChicken.SetPosition(ClientMain.Player.Position + new vec3(3, 0, 0));
            //ClientMain.World.SpawnEntityInWorld(entityChicken);
        }

        /// <summary>
        /// Пакет синхронизации времени с сервером
        /// </summary>
        private void Handle03TimeUpdate(PacketS03TimeUpdate packet)
        {
            ClientMain.SetTickCounter(packet.GetTime());
        }

        /// <summary>
        /// Пакет оборудования сущности
        /// </summary>
        private void Handle04EntityEquipment(PacketS04EntityEquipment packet)
        {
            EntityLiving entity = ClientMain.World.GetEntityLivingByID(packet.GetId());
            if (entity != null)
            {
                entity.SetCurrentItemOrArmor(packet.GetSlot(), packet.GetItemStack());
            }
        }

        /// <summary>
        /// Пакет пораметров здоровья игрока
        /// </summary>
        //private void Handle06UpdateHealth(PacketS06UpdateHealth packet)
        //{
        //    ClientMain.Player.SetHealth(packet.GetHealth());
        //    ClientMain.Player.PerformHurtAnimation();
        //}

        /// <summary>
        /// Пакет перезапуска игрока
        /// </summary>
        private void Handle07Respawn(PacketS07Respawn packet)
        {
            ClientMain.Player.Respawn();
        }

        /// <summary>
        /// Пакет расположения игрока, при старте, телепорт, рестарте и тп
        /// </summary>
        private void Handle08PlayerPosLook(PacketS08PlayerPosLook packet)
        {
            ClientMain.Player.SetPosLook(packet.GetPos(), packet.GetYaw(), packet.GetPitch());
            ClientMain.Player.UpFrustumCulling();
        }

        /// <summary>
        /// Пакет анимации сущности
        /// </summary>
        private void Handle0BAnimation(PacketS0BAnimation packet)
        {
            EntityLiving entity = ClientMain.World.GetEntityLivingByID(packet.GetId());
            if (entity != null)
            {
                switch (packet.GetAnimation())
                {
                    case PacketS0BAnimation.EnumAnimation.SwingItem: entity.SwingItem(); break;
                    case PacketS0BAnimation.EnumAnimation.Hurt: entity.PerformHurtAnimation(); break;
                    case PacketS0BAnimation.EnumAnimation.Fall: entity.ParticleFall(10); break;
                    case PacketS0BAnimation.EnumAnimation.Recovery: entity.PerformRecoveryAnimation(); break;
                }
            }
        }

        /// <summary>
        /// Пакет спавна других игроков
        /// </summary>
        private void Handle0CSpawnPlayer(PacketS0CSpawnPlayer packet)
        {
            // Удачный вход сетевого игрока, типа приветствие
            // Или после смерти
            EntityPlayerMP entity = new EntityPlayerMP(ClientMain.World);
            entity.SetDataPlayer(packet.GetId(), packet.GetUuid(), packet.GetName());
            entity.SetPosLook(packet.GetPos(), packet.GetYaw(), packet.GetPitch());
            entity.Inventory.SetCurrentItemAndArmor(packet.GetStacks());
            ArrayList list = packet.GetList();
            if (list != null && list.Count > 0)
            {
                entity.MetaData.UpdateWatchedObjectsFromList(list);
            }

            //entity.FlagSpawn = true;
            //ClientMain.World.SpawnEntityInWorld(entity);
            ClientMain.World.AddEntityToWorld(entity.Id, entity);

            //EntityChicken entityChicken = new EntityChicken(ClientMain.World);
            //entityChicken.SetEntityId(101);
            //entityChicken.SetPosition(packet.GetPos() + new vec3(3, 0, 0));
            //ClientMain.World.SpawnEntityInWorld(entityChicken);
            // entity.FlagSpawn = false;
        }

        /// <summary>
        /// Пакет передачи сущности предмета к сущности игрока
        /// </summary>
        private void Handle0DCollectItem(PacketS0DCollectItem packet)
        {
            EntityBase entityItem = ClientMain.World.GetEntityByID(packet.GetItemId());
            if (entityItem != null)
            {
                EntityLiving entity = ClientMain.World.GetEntityLivingByID(packet.GetEntityId());
                if (entity == null) entity = ClientMain.Player;

                ClientMain.Sample.PlaySound(AssetsSample.MobChickenPlop, .2f);
                ClientMain.World.RemoveEntityFromWorld(packet.GetItemId());
            }
        }

        /// <summary>
        /// Пакет спавна мобов
        /// </summary>
        private void Handle0FSpawnMob(PacketS0FSpawnMob packet)
        {
            EntityBase entity = Entities.CreateEntityByEnum(ClientMain.World, packet.GetEnum());
            entity.SetEntityId(packet.GetId());
            if (entity is EntityLiving entityLiving)
            {
                entityLiving.SetPosLook(packet.GetPos(), packet.GetYaw(), packet.GetPitch());
            }
            else
            {
                entity.SetPosition(packet.GetPos());
                // Надо задать позицию с сервера, на земле не важено
                entity.SetMotionServer(packet.GetPos(), false);
            }
            ArrayList list = packet.GetList();
            if (list != null && list.Count > 0)
            {
                entity.MetaData.UpdateWatchedObjectsFromList(list);
            }
            ClientMain.World.AddEntityToWorld(entity.Id, entity);
        }

        
        private void Handle12EntityVelocity(PacketS12EntityVelocity packet)
        {
            EntityLiving entity = ClientMain.World.GetEntityLivingByID(packet.GetId());
            if (entity != null)
            {
                entity.AddVelocity(packet.GetMotion());
            }
        }

        /// <summary>
        /// Пакет удаление сущностей
        /// </summary>
        private void Handle13DestroyEntities(PacketS13DestroyEntities packet)
        {
            int count = packet.GetIds().Length;
            for(int i = 0; i < count; i++)
            {
                ClientMain.World.RemoveEntityFromWorld(packet.GetIds()[i]);
            }
        }

        /// <summary>
        /// Пакет перемещения сущности
        /// </summary>
        private void Handle14EntityMotion(PacketS14EntityMotion packet)
        {
            EntityBase entity = ClientMain.World.GetEntityByID(packet.GetId());
            if (entity != null)
            {
                if (entity is EntityLiving entityLiving)
                {
                    entityLiving.SetMotionServer(
                        packet.GetPos(), packet.GetYaw(), packet.GetPitch(),
                        packet.OnGround());
                }
                else if (entity is EntityItem entityItem)
                {
                    entityItem.SetMotionServer(packet.GetPos(), packet.OnGround());
                }
                else if (entity is EntityThrowable entityThrowable)
                {
                    entityThrowable.SetMotionServer(packet.GetPos(), packet.OnGround());
                }
            }
        }

        /// <summary>
        /// Пакет статуса сущности, умирает, урон и прочее
        /// </summary>
        private void Handle19EntityStatus(PacketS19EntityStatus packet)
        {
            switch (packet.GetStatus())
            {
                case PacketS19EntityStatus.EnumStatus.Die: ClientMain.Player.OnDeathClient(packet.GetText()); break;
            }
        }

            /// <summary>
            /// Пакет дополнительных данных сущности
            /// </summary>
            private void Handle1CEntityMetadata(PacketS1CEntityMetadata packet)
        {
            EntityBase entity = ClientMain.World.GetEntityByID(packet.GetId());
            ArrayList list = packet.GetList();
            if (entity != null && list != null && list.Count > 0)
            {
                entity.MetaData.UpdateWatchedObjectsFromList(list);
            }

            //PacketS1CEntityMetadata.EnumData data = packet.GetEnumData();
            //if (data == PacketS1CEntityMetadata.EnumData.Amount)
            //{
            //    EntityBase entity = ClientMain.World.GetEntityByID(packet.GetId());
            //    if (entity != null && entity is EntityItem entityItem)
            //    {
            //        entityItem.Stack.SetAmount(packet.GetAmount());
            //    }
            //} else if (data == PacketS1CEntityMetadata.EnumData.SneakingSprinting)
            //{
            //    EntityLiving entityLiving = ClientMain.World.GetEntityLivingByID(packet.GetId());
            //    if (entityLiving != null)
            //    {
            //        entityLiving.SetSneakingSprinting(packet.IsSneaking(), packet.IsSprinting());
            //    }
            //}
        }

        private void Handle21ChunkData(PacketS21ChunkData packet)
        {
            ClientMain.World.ChunkPrClient.PacketChunckData(packet);

            // Заносим в дополнительный поток пакетов вокселей, 
            // чтоб минимизировать загрузку основного клиентского потока где есть Tick, ибо будет проседать fps
            //ClientMain.World.AddPacketChunkQueue(packet);
        }

        private void Handle22MultiBlockChange(PacketS22MultiBlockChange packet)
        {
            packet.ReceivedBlocks(ClientMain.World);
        }

        private void Handle23BlockChange(PacketS23BlockChange packet)
        {
            ClientMain.World.SetBlockStateClient(packet.GetBlockPos(), packet.GetBlockState());
        }

        private void Handle25BlockBreakAnim(PacketS25BlockBreakAnim packet)
        {
            ClientMain.World.SendBlockBreakProgress(packet.GetBreakerId(), packet.GetBlockPos(), packet.GetProgress());
        }

        private void Handle27Explosion(PacketS27Explosion packet)
        {
            ClientMain.Player.AddVelocity(packet.motion);
        }

        private void Handle29SoundEffect(PacketS29SoundEffect packet)
        {
            ClientMain.World.PlaySound(ClientMain.Player, packet.GetAssetsSample(), 
                packet.GetPosition(), packet.GetVolume(), packet.GetPitch());
        }

        private void Handle2AParticles(PacketS2AParticles packet)
        {
            ClientMain.World.SpawnParticle(packet.GetParticle(), packet.GetCount(), packet.GetPosition(), packet.GetOffset(), packet.GetMotion(), packet.GetItems());
            //int count = packet.GetCount();
            //if (count == 1)
            //{
            //    ClientMain.World.SpawnParticle(packet.GetParticle(), packet.GetPosition(), packet.Getoffset(), packet.GetItems());
            //}
            //else
            //{
            //    for (int i = 0; i < count; i++)
            //    {
            //        ClientMain.World.SpawnParticle(packet.GetParticle(), packet.GetPosition(), packet.Getoffset(), packet.GetItems());
            //    }
            //}
        }

        private void Handle2DOpenWindow(PacketS2DOpenWindow packet)
        {
            ClientMain.SetScreen(ObjectKey.GameWindowOpen, packet.GetWindowType());
        }

        /// <summary>
        /// Редактирование слота игрока
        /// </summary>
        private void Handle2FSetSlot(PacketS2FSetSlot packet)
        {
            int slot = packet.GetSlot();
            if (slot < 100 || slot == 255)
            {
                ClientMain.Player.Inventory.SetInventorySlotContents(slot, packet.GetItemStack());
                if (slot == ClientMain.Player.Inventory.CurrentItem)
                {
                    ClientMain.Player.ItemInWorldManagerDestroyAbout();
                }
            }
            else
            {
                // Пришёл стак для склада
                ClientMain.Screen.AcceptNetworkPackage(packet);
            }
        }

        private void Handle30WindowItems(PacketS30WindowItems packet)
        {
            if (packet.IsInventory())
            {
                ClientMain.Player.Inventory.SetMainAndArmor(packet.GetStacks());
            }
            else
            {
                ClientMain.Screen.AcceptNetworkPackage(packet);
            }
        }

        private void Handle31WindowProperty(PacketS31WindowProperty packet)
        {
            PacketS31WindowProperty.EnumAction action = packet.GetAction();
            if (action == PacketS31WindowProperty.EnumAction.CloseWindow)
            {
                ClientMain.SetScreen(ObjectKey.GameWindowClose);
            }
            else if (action == PacketS31WindowProperty.EnumAction.CraftStop)
            {
                ClientMain.Player.Inventory.CraftedClient();
            }
            else if (action == PacketS31WindowProperty.EnumAction.ArrayRecipe)
            {
                ClientMain.Screen.AcceptNetworkPackage(packet);
            }
        }

        /// <summary>
        /// Задать атрибуты игроку
        /// </summary>
        private void Handle39PlayerAbilities(PacketS39PlayerAbilities packet)
        {
            ClientMain.Player.SetPlayerAbilities(packet);
        }

        /// <summary>
        /// Пакет чата
        /// </summary>
        private void Handle3AMessage(PacketS3AMessage packet)
        {
            ClientMain.World.WorldRender.ScreenGame.PersistantChatGUI.AddMessage(packet.GetMessage(), packet.IsConsole());
        }

        #region ConnectionDisconnect

        /// <summary>
        /// Пакет соединения
        /// </summary>
        private void HandleF0Connection(PacketSF0Connection packet)
        {
            if (packet.IsBegin())
            {
                ClientMain.BeginWorldConnect();
            }
            else if (packet.IsConnect())
            {
                // connect
                ClientMain.TrancivePacket(new PacketC02LoginStart(ClientMain.ToNikname(), true));
            }
            else
            {
                // disconnect с причиной
                ClientMain.ExitingWorld(packet.GetCause());
            }
            
        }

        
        /// <summary>
        /// Дисконект игрока
        /// </summary>
        private void HandleF1Disconnect(PacketSF1Disconnect packet)
        {
            ClientMain.World.RemoveEntityFromWorld(packet.GetId());
        }

        #endregion
    }
}
