using MvkServer.Entity;
using MvkServer.Entity.List;
using MvkServer.Glm;
using MvkServer.Inventory;
using MvkServer.Item;
using MvkServer.Network.Packets.Client;
using MvkServer.Network.Packets.Server;
using MvkServer.Util;
using MvkServer.World.Block;
using System.Net.Sockets;

namespace MvkServer.Network
{
    /// <summary>
    /// Обработка серверных пакетов для клиента
    /// </summary>
    public class ProcessServerPackets : ProcessPackets
    {
        /// <summary>
        /// Основной сервер
        /// </summary>
        public Server ServerMain { get; protected set; }

        private long networkTickCount;
        private long lastPingTime;
        private long lastSentPingPacket;
        private uint pingKeySend;
        /// <summary>
        /// Массив очередей пакетов
        /// </summary>
        private DoubleList<SocketPacket> packets = new DoubleList<SocketPacket>();
        /// <summary>
        /// Класс для очередей пакетов
        /// </summary>
        private struct SocketPacket
        {
            public Socket socket;
            public IPacket packet;
        }

        public ProcessServerPackets(Server server) : base(false) => ServerMain = server;

        /// <summary>
        /// Передача данных для сервера
        /// </summary>
        public void ReceiveBuffer(Socket socket, byte[] buffer) => ReceivePacket(socket, buffer);

        protected override void ReceivePacketServer(Socket socket, IPacket packet)
        {
            switch (GetId(packet))
            {
                case 0x00:
                    Handle00Ping(socket, (PacketC00Ping)packet);
                    break;
                case 0x02:
                    // Мира ещё нет, он в стадии создании, первый старт первого игрока
                    Handle02LoginStart(socket, (PacketC02LoginStart)packet);
                    break;
                default:
                    // Мир есть, заносим в пакет с двойным буфером, для обработки в такте
                    packets.Add(new SocketPacket() { socket = socket, packet = packet });
                    break;

            }
        }

        private void UpdateReceivePacketServer(Socket socket, IPacket packet)
        {
            switch (GetId(packet))
            {
                case 0x01: Handle01KeepAlive(socket, (PacketC01KeepAlive)packet); break;
                case 0x03: Handle03UseEntity(socket, (PacketC03UseEntity)packet); break;
                case 0x04: Handle04PlayerPosition(socket, (PacketC04PlayerPosition)packet); break;
                case 0x05: Handle05PlayerLook(socket, (PacketC05PlayerLook)packet); break;
                case 0x06: Handle06PlayerPosLook(socket, (PacketC06PlayerPosLook)packet); break;
                case 0x07: Handle07PlayerDigging(socket, (PacketC07PlayerDigging)packet); break;
                case 0x08: Handle08PlayerBlockPlacement(socket, (PacketC08PlayerBlockPlacement)packet); break;
                case 0x09: Handle09HeldItemChange(socket, (PacketC09HeldItemChange)packet); break;
                case 0x0A: Handle0AAnimation(socket, (PacketC0AAnimation)packet); break;
                case 0x0C: Handle0CPlayerAction(socket, (PacketC0CPlayerAction)packet); break;
                case 0x0E: Handle0EPacketClickWindow(socket, (PacketC0EPacketClickWindow)packet); break;
                case 0x10: Handle10CreativeInventoryAction(socket, (PacketC10CreativeInventoryAction)packet); break;
                case 0x14: HandleC14Message(socket, (PacketC14Message)packet); break;
                case 0x15: Handle15ClientSetting(socket, (PacketC15ClientSetting)packet); break;
                case 0x16: Handle16ClientStatus(socket, (PacketC16ClientStatus)packet); break;
            }
        }

        public void Clear() => packets.Clear();

        /// <summary>
        /// Такт сервера
        /// </summary>
        public void Update()
        {
            networkTickCount++;
            if (networkTickCount - lastSentPingPacket > 40)
            {
                lastSentPingPacket = networkTickCount;
                lastPingTime = ServerMain.Time();
                pingKeySend = (uint)lastPingTime;
                ServerMain.ResponsePacketAll(new PacketS01KeepAlive(pingKeySend));
            }
            packets.Step();
            SocketPacket socketPacket;
            int count = packets.CountBackward;
            for (int i = 0; i < count; i++)
            {
                socketPacket = packets.GetNext();
                UpdateReceivePacketServer(socketPacket.socket, socketPacket.packet);
            }
        }

        /// <summary>
        /// Ping-pong
        /// </summary>
        private void Handle00Ping(Socket socket, PacketC00Ping packet) => ServerMain.ResponsePacket2(socket, new PacketS00Pong(packet.GetClientTime()));

        /// <summary>
        /// KeepAlive
        /// </summary>
        private void Handle01KeepAlive(Socket socket, PacketC01KeepAlive packet)
        {
            EntityPlayerServer entityPlayer = ServerMain.World.Players.GetPlayerSocket(socket);
            if (packet.GetTime() == pingKeySend && entityPlayer != null)
            {
                entityPlayer.SetPing(lastPingTime);
            }
        }

        /// <summary>
        /// Пакет проверки логина
        /// </summary>
        private void Handle02LoginStart(Socket socket, PacketC02LoginStart packet)
        {
            EntityPlayerServer entityPlayer = new EntityPlayerServer(ServerMain, socket, packet.GetName(), ServerMain.World);

            if (packet.IsCheckDuplicate())
            {
                ServerMain.World.Players.CheckPlayer(entityPlayer);
            }
            else
            {
                ServerMain.World.Players.LoginStart(entityPlayer);
            }
        }

        /// <summary>
        /// Взаимодействие с сущностью
        /// </summary>
        private void Handle03UseEntity(Socket socket, PacketC03UseEntity packet)
        {
            // сущность которая производит действие
            EntityPlayerServer entityPlayer = ServerMain.World.Players.GetPlayerSocket(socket);
            /// сущность на которую произвели действие
            EntityLiving entity = (EntityLiving)ServerMain.World.LoadedEntityList.Get(packet.GetId());

            if (entityPlayer != null && entity != null)
            {
                // Урон
                if (packet.GetAction() == PacketC03UseEntity.EnumAction.Attack)
                {
                    vec3 vec = packet.GetVec() * .9f;
                    vec.y *= .2f;
                    entity.AttackEntityFrom(EnumDamageSource.Player, entityPlayer.Inventory.GetDamageToAttack(ServerMain.World), vec, entityPlayer);
                }
            }
        }

        /// <summary>
        /// Пакет позиции игрока
        /// </summary>
        private void Handle04PlayerPosition(Socket socket, PacketC04PlayerPosition packet)
        {
            EntityPlayerServer entityPlayer = ServerMain.World.Players.GetPlayerSocket(socket);
            if (entityPlayer != null)
            {
                entityPlayer.SetSneakingSprinting(packet.IsSneaking(), packet.IsSprinting());
                entityPlayer.SetPosition(packet.GetPos());
                entityPlayer.SetOnGroundServer(packet.OnGround());
                entityPlayer.MarkPlayerActive();
            }
        }

        /// <summary>
        /// Пакет камеры игрока
        /// </summary>
        private void Handle05PlayerLook(Socket socket, PacketC05PlayerLook packet)
        {
            EntityPlayerServer entityPlayer = ServerMain.World.Players.GetPlayerSocket(socket);
            if (entityPlayer != null)
            {
                entityPlayer.SetSneakingSprinting(packet.IsSneaking(), entityPlayer.IsSprinting());
                entityPlayer.SetRotationHead(packet.GetYaw(), packet.GetPitch());
                entityPlayer.MarkPlayerActive();
            }
        }

        /// <summary>
        /// Пакет позиции и камеры игрока
        /// </summary>
        private void Handle06PlayerPosLook(Socket socket, PacketC06PlayerPosLook packet)
        {
            EntityPlayerServer entityPlayer = ServerMain.World.Players.GetPlayerSocket(socket);
            if (entityPlayer != null)
            {
                entityPlayer.SetSneakingSprinting(packet.IsSneaking(), packet.IsSprinting());
                entityPlayer.SetPosition(packet.GetPos());
                entityPlayer.SetRotationHead(packet.GetYaw(), packet.GetPitch());
                entityPlayer.SetOnGroundServer(packet.OnGround());
                entityPlayer.MarkPlayerActive();
            }
        }

        private void Handle07PlayerDigging(Socket socket, PacketC07PlayerDigging packet)
        {
            EntityPlayerServer entityPlayer = ServerMain.World.Players.GetPlayerSocket(socket);
            if (entityPlayer != null)
            {
                if (packet.GetDigging() == PacketC07PlayerDigging.EnumDigging.Destroy)
                {
                    // Мгновенное разрушение блока
                    entityPlayer.TheItemInWorldManager.Destroy(packet.GetBlockPos());
                }
                else if (packet.GetDigging() == PacketC07PlayerDigging.EnumDigging.Stop)
                {
                    // Окончено разрушение, блок сломан
                    entityPlayer.TheItemInWorldManager.DestroyStop();
                }
                else if (packet.GetDigging() == PacketC07PlayerDigging.EnumDigging.Start)
                {
                    // Начато разрушение
                    entityPlayer.TheItemInWorldManager.DestroyStart(packet.GetBlockPos());
                }
                else if (packet.GetDigging() == PacketC07PlayerDigging.EnumDigging.Hit)
                {
                    // Первый удар
                    entityPlayer.TheItemInWorldManager.HitOnBlock(packet.GetBlockPos(),
                        ServerMain.World.GetBlockState(packet.GetBlockPos()).GetBlock(), true);
                }
                else
                {
                    // Отмена разрушения
                    entityPlayer.TheItemInWorldManager.DestroyAbout();
                }
                entityPlayer.MarkPlayerActive();
            }
        }

        /// <summary>
        /// Пакет установки блока
        /// </summary>
        private void Handle08PlayerBlockPlacement(Socket socket, PacketC08PlayerBlockPlacement packet)
        {
            EntityPlayerServer entityPlayer = ServerMain.World.Players.GetPlayerSocket(socket);
            
            if (entityPlayer != null)
            {
                byte flag = packet.GetFlag();
                if (flag == 0)
                {
                    // установка блока
                    entityPlayer.TheItemInWorldManager.Put(packet.GetBlockPos(), packet.GetSide(), packet.GetFacing(), entityPlayer.Inventory.CurrentItem);
                }
                else if(flag == 1)
                {
                    // Действие на блок, клик правой клавишей мыши
                    BlockPos blockPos = packet.GetBlockPos();
                    BlockState blockState = ServerMain.World.GetBlockState(blockPos);
                    blockState.GetBlock().OnBlockActivated(ServerMain.World, entityPlayer, blockPos, blockState, packet.GetSide(), packet.GetFacing());
                }
                else if (flag == 2)
                {
                    // Действие предмета в руке, клик правой клавишей мыши
                    ItemStack itemStack = entityPlayer.Inventory.GetCurrentItem();
                    itemStack.UseItemRightClick(ServerMain.World, entityPlayer);
                }
                entityPlayer.MarkPlayerActive();
            }
        }

        /// <summary>
        /// Пакет выбранный слот у игрока
        /// </summary>
        private void Handle09HeldItemChange(Socket socket, PacketC09HeldItemChange packet)
        {
            EntityPlayerServer entityPlayer = ServerMain.World.Players.GetPlayerSocket(socket);
            
            if (entityPlayer != null)
            {
                int slot = packet.GetSlotId();
                if (slot >= 0 && slot < InventoryPlayer.COUNT)
                {
                    entityPlayer.Inventory.SetCurrentItem(packet.GetSlotId());
                    entityPlayer.MarkPlayerActive();
                }
                else
                {
                    ServerMain.Log.Log(entityPlayer.GetName() + " пытался установить недопустимый переносимый предмет");
                }
            }
        }

        /// <summary>
        /// Пакет анимации игрока
        /// </summary>
        private void Handle0AAnimation(Socket socket, PacketC0AAnimation packet)
        {
            EntityPlayerServer entityPlayer = ServerMain.World.Players.GetPlayerSocket(socket);
            if (entityPlayer != null)
            {
                entityPlayer.SwingItem();
                entityPlayer.MarkPlayerActive();
            }
        }

        /// <summary>
        /// Пакет действия игрока
        /// </summary>
        private void Handle0CPlayerAction(Socket socket, PacketC0CPlayerAction packet)
        {
            EntityPlayerServer entityPlayer = ServerMain.World.Players.GetPlayerSocket(socket);
            if (entityPlayer != null)
            {
                PacketC0CPlayerAction.EnumAction action = packet.GetAction();
                if (action == PacketC0CPlayerAction.EnumAction.Fall)
                {
                    // Падение с высоты
                    if (packet.GetParam() > 5)
                    {
                        float damage = packet.GetParam() - 5;
                        entityPlayer.AttackEntityFrom(EnumDamageSource.Fall, damage);
                        ServerMain.World.Tracker.SendToAllTrackingEntity(entityPlayer, new PacketS0BAnimation(entityPlayer.Id, PacketS0BAnimation.EnumAnimation.Fall));
                    }
                    entityPlayer.MarkPlayerActive();
                }
                else if (action == PacketC0CPlayerAction.EnumAction.StopUsingItem)
                {
                    // Прекратили действие предмета
                    entityPlayer.StopUsingItem();
                }
                else if (action == PacketC0CPlayerAction.EnumAction.ThrowOutCurrentItem)
                {
                    // Выбросить текущий предмет
                    entityPlayer.Inventory.ThrowOutCurrentItem();
                }
            }
        }

        /// <summary>
        /// Пакет кликов по окну инвентаря, крафта и других подобных окон
        /// </summary>
        private void Handle0EPacketClickWindow(Socket socket, PacketC0EPacketClickWindow packet)
        {
            EntityPlayerServer entityPlayer = ServerMain.World.Players.GetPlayerSocket(socket);
            if (entityPlayer != null)
            {
                PacketC0EPacketClickWindow.EnumAction action = packet.GetAction();

                if (action == PacketC0EPacketClickWindow.EnumAction.Open)
                {
                    entityPlayer.SendAccessRecipe(packet.GetNumber());
                }
                else if (action == PacketC0EPacketClickWindow.EnumAction.Close)
                {
                    entityPlayer.Inventory.CraftStop();
                }
                else if (action == PacketC0EPacketClickWindow.EnumAction.ThrowOutAir)
                {
                    entityPlayer.Inventory.ThrowOutAir();
                }
                else if (action == PacketC0EPacketClickWindow.EnumAction.ClickLeftSlot)
                {
                    entityPlayer.Inventory.ClickInventorySlotContents(packet.GetNumber(), false);
                }
                else if (action == PacketC0EPacketClickWindow.EnumAction.ClickRightSlot)
                {
                    entityPlayer.Inventory.ClickInventorySlotContents(packet.GetNumber(), true);
                }
                else if (action == PacketC0EPacketClickWindow.EnumAction.CraftOne)
                {
                    entityPlayer.Inventory.CraftBeginServer(packet.GetNumber(), false);
                }
                else if (action == PacketC0EPacketClickWindow.EnumAction.CraftMax)
                {
                    entityPlayer.Inventory.CraftBeginServer(packet.GetNumber(), true);
                }
            }
        }

        /// <summary>
        /// Пакет действия креативного инвентаря
        /// </summary>
        private void Handle10CreativeInventoryAction(Socket socket, PacketC10CreativeInventoryAction packet)
        {
            EntityPlayerServer entityPlayer = ServerMain.World.Players.GetPlayerSocket(socket);
            if (entityPlayer != null)
            {
                if (packet.GetSlotId() == -1)
                {
                    // Drop
                    entityPlayer.DropItem(packet.GetStack(), true);
                }
                else
                {
                    entityPlayer.Inventory.SetInventorySlotContents(packet.GetSlotId(), packet.GetStack().Item == null ? null : packet.GetStack());
                    entityPlayer.Inventory.SendSlot(packet.GetSlotId());
                }
            }

            // ServerMain.World.Players.ClientSetting(socket, packet);
        }

        /// <summary>
        /// Пакет чата
        /// </summary>
        private void HandleC14Message(Socket socket, PacketC14Message packet) 
            => ServerMain.World.Players.ClientMessage(socket, packet);

        /// <summary>
        /// Пакет настроек клиента
        /// </summary>
        private void Handle15ClientSetting(Socket socket, PacketC15ClientSetting packet) 
            => ServerMain.World.Players.ClientSetting(socket, packet);

        /// <summary>
        /// Пакет статуса клиента
        /// </summary>
        private void Handle16ClientStatus(Socket socket, PacketC16ClientStatus packet) 
            => ServerMain.World.Players.ClientStatus(socket, packet.GetState());

        ///// <summary>
        ///// Отправить изменение по здоровью
        ///// </summary>
        //private void ResponseHealth(EntityLiving entity)
        //{
        //    if (entity is EntityPlayerServer)
        //    {
        //        ((EntityPlayerServer)entity).SendPacket(new PacketS06UpdateHealth(entity.Health));
        //    }

        //    if (entity.Health > 0)
        //    {
        //        // Анимация урона
        //        ServerMain.World.Tracker.SendToAllTrackingEntity(entity, new PacketS0BAnimation(entity.Id,
        //            PacketS0BAnimation.EnumAnimation.Hurt));
        //    } else
        //    {
        //        // Начала смерти
        //        ServerMain.World.Tracker.SendToAllTrackingEntity(entity, new PacketS19EntityStatus(entity.Id,
        //            PacketS19EntityStatus.EnumStatus.Die));
        //    }
        //}


    }
}
