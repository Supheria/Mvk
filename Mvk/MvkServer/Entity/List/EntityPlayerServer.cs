using MvkServer.Glm;
using MvkServer.Management;
using MvkServer.Network;
using MvkServer.Network.Packets.Server;
using MvkServer.Util;
using MvkServer.World;
using MvkServer.World.Chunk;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace MvkServer.Entity.List
{
    /// <summary>
    /// Сущность игрока для сервера
    /// </summary>
    public class EntityPlayerServer : EntityPlayer
    {
        /// <summary>
        /// Сетевой сокет клиента
        /// </summary>
        public Socket SocketClient { get; protected set; }
        /// <summary>
        /// Основной сервер
        /// </summary>
        public Server ServerMain { get; protected set; }
        /// <summary>
        /// Cписок, содержащий все чанки которые нужны клиенту согласно его обзору для загрузки
        /// </summary>
        public MapListVec2i LoadedChunks { get; protected set; } = new MapListVec2i();
        /// <summary>
        /// Список чанков которые нужно проверить на загрузку или генерацию,
        /// должен формироваться по дистанции от игрока
        /// </summary>
        public MapListVec2i LoadingChunks { get; protected set; } = new MapListVec2i();
        /// <summary>
        /// Пинг клиента в мс
        /// </summary>
        public int Ping { get; protected set; } = -1;

        /// <summary>
        /// ItemInWorldManager, принадлежащий этому игроку
        /// </summary>
        public ItemInWorldManager TheItemInWorldManager { get; private set; }
        /// <summary>
        /// Флаг для загрузки всех ближайших сущностей в трекере
        /// 0 - старт, 1 - загрузка, 2 - далее, нет загрузки
        /// </summary>
        public byte TrackerBeginFlag { get; private set; } = 0;
        /// <summary>
        /// Флаг спавна
        /// </summary>
        public bool FlagSpawn { get; set; } = false;
        /// <summary>
        /// Флаг начального спавна
        /// </summary>
        public bool FlagBeginSpawn { get; set; } = false;

        private Profiler profiler;

        /// <summary>
        /// Список сущностей не игрок, которые в ближайшем тике будут удалены
        /// </summary>
        private MapListId destroyedItemsNetCache = new MapListId();

        /// <summary>
        /// Почледнее активное время игрока
        /// </summary>
        protected long playerLastActiveTime = 0;

        //private List<vec2i> LoadingChunks = new List<vec2i>();

        // должен быть список чанков которые может видеть игрок
        // должен быть список чанков которые надо догрузить игроку

        public EntityPlayerServer(Server server, Socket socket, string name, WorldServer world) : base(world)
        {
            ServerMain = server;
            SocketClient = socket;
            base.name = name;
            UUID = GetHash(name);
            profiler = new Profiler(server.Log);
            TheItemInWorldManager = new ItemInWorldManager(world, this);
        }

        /// <summary>
        /// Задать время пинга
        /// </summary>
        public void SetPing(long time) => Ping = (Ping * 3 + (int)(ServerMain.Time() - time)) / 4;

        /// <summary>
        /// Задать положение сидя и бега
        /// </summary>
        public void SetSneakingSprinting(bool sneaking, bool sprinting)
        {
            if (IsSneaking() != sneaking)
            {
                SetSneaking(sneaking);
                if (sneaking) Sitting(); else Standing();
            }
            SetSprinting(sprinting);
            ActionAdd(EnumActionChanged.IsSneaking);
            ActionAdd(EnumActionChanged.IsSprinting);
        }

        /// <summary>
        /// Получить хэш по строке
        /// </summary>
        protected string GetHash(string input)
        {
            MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
        }


        /// <summary>
        /// Надо сделать респавн в ближайшем тике
        /// </summary>
        public override void Respawn()
        {
            base.Respawn();
            destroyedItemsNetCache.Clear();
        }

        /// <summary>
        /// Преверка спавна и респавна игроков
        /// дла запуска чанков потом игрока
        /// </summary>
        public void UpdateSpawn()
        {
            if (FlagSpawn || FlagBeginSpawn)
            {
                // запрос на спавн
                if (!World.IsRemote && World is WorldServer worldServer)
                {
                    if (FlagSpawn)
                    {
                        Respawn();
                        worldServer.Players.SpawnPositionTest(this);
                        worldServer.SpawnEntityInWorld(this);
                    }
                    if (!FlagSpawn)
                    {
                        if (FlagBeginSpawn)
                        {
                            worldServer.Players.ResponsePacketJoinGame(this);
                            FlagBeginSpawn = false;
                        }
                        else
                        {
                            SendPacket(new PacketS07Respawn());
                            SendPacket(new PacketS08PlayerPosLook(Position, RotationYawHead, RotationPitch));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Вызывается для обновления позиции / логики объекта
        /// </summary>
        public override void Update()
        {
            if (TrackerBeginFlag < 2) TrackerBeginFlag++;

            // Tут base.Update не надо, так-как это обрабатывается на клиенте, 
            // тут отправление перемещение игрокам если оно надо

            EntityUpdateLocation();
            UpdateItemInUse();
            EntityUpdateServer();

            profiler.StartSection("TheItemInWorldManager.UpdateBlock");
            TheItemInWorldManager.UpdateBlock();
            profiler.EndSection();

            // если нет хп обновлям смертельную картинку
            if (GetHealth() <= 0f) DeathUpdate();

            // Обновления предметов которые могут видеть игроки, что в руке, броня
            UpdateItems();

            // Обработка соприкосновении игрока с другими сущностями
            if (GetHealth() > 0 && !World.IsRemote && !IsInvisible())
            {
                AxisAlignedBB axis = BoundingBox.Expand(new vec3(1.5f, .8f, 1.5f));
                List<EntityBase> list = World.GetEntitiesWithinAABB(ChunkBase.EnumEntityClassAABB.All, axis, Id);
                for (int i = 0; i < list.Count; i++)
                {
                    if (!list[i].IsDead)
                    {
                        list[i].OnCollideWithPlayer(this);
                    }
                }
            }

            // Отправляем запрос на удаление сущностей которые не видим
            if (destroyedItemsNetCache.Count > 0)
            {
                List<ushort> ids = new List<ushort>();
                while (destroyedItemsNetCache.Count > 0)
                {
                    ids.Add(destroyedItemsNetCache.FirstRemove());
                }
                SendPacket(new PacketS13DestroyEntities(ids.ToArray()));
            }

            UpdatePlayer();
        }

        /// <summary>
        /// Вызывается только на сервере у игроков для передачи перемещения
        /// </summary>
        private void UpdatePlayer()
        {
            try
            {
                int i = 0;
                List<vec2i> loadedNull = new List<vec2i>();
                int radius = OverviewChunk;
                vec2i chunkCoor = GetChunkPos();
                int chx = chunkCoor.x;
                int chz = chunkCoor.y;
                while (LoadedChunks.Count > 0 && i < MvkGlobal.COUNT_PACKET_CHUNK_TPS)
                {
                    profiler.StartSection("LoadChunk");
                    // передача пакетов чанков по сети
                    vec2i pos = LoadedChunks.FirstRemove();
                    ChunkBase chunk = World.GetChunk(pos);
                    // NULL по сути не должен быть!!!
                    if (chunk != null && chunk.IsSendChunk)
                    {
                        profiler.EndStartSection("PacketS21ChunckData");
                        PacketS21ChunkData packet = new PacketS21ChunkData(chunk, true, 65535);
                        //World.Log.Log("1UP:[{0}] {1} {2}", chunk.Position, 65535, chunk.GetDebugAllSegment());
                        //World.Log.Log("1UP:[{0}]", chunk.Position);
                        profiler.EndStartSection("ResponsePacket");
                        SendPacket(packet);
                        i++;
                    }
                    else
                    {
                        loadedNull.Add(pos);
                    }
                    profiler.EndSection();
                }

                // Если чанки не попали, мы возращаем в массив
                int count = loadedNull.Count;
                if (count > 0) {
                    count--;
                    for (int j = count; j >= 0; j--)
                    {
                        LoadedChunks.Insert(loadedNull[j]);
                    }
                }

                if (ActionChanged.HasFlag(EnumActionChanged.Position) || !SameOverviewChunkPrev())
                {
                    //ServerMain.World.Players.ResponsePacketAll(
                    //    new PacketB20Player().PositionYawPitch(Position, RotationYawHead, RotationYaw, RotationPitch, IsSneaking, OnGround, Id),
                    //    Id
                    //);
                    profiler.StartSection("UpdateMountedMovingPlayer");
                    // обновлять фрагменты чанков вокруг игрока, перемещаемого логикой сервера
                    ((WorldServer)World).Players.UpdateMountedMovingPlayer(this);
                    profiler.EndSection();
                    // isMotionServer = false;
                }
            }
            catch (Exception ex)
            {
                Logger.Crach(ex);
                throw;
            }
        }

        /// <summary>
        /// Сущность которую надо удалить у клиента
        /// </summary>
        public void SendRemoveEntity(EntityBase entity)
        {
            if (entity is EntityPlayer)
            {
                SendPacket(new PacketS13DestroyEntities(new ushort[] { entity.Id }));
            }
            else
            {
                destroyedItemsNetCache.Add(entity.Id);
            }
        }

        /// <summary>
        /// Обновить обзор прошлого такта
        /// </summary>
        public void UpOverviewChunkPrev()
        {
            OverviewChunkPrev = OverviewChunk;
            DistSqrt = MvkStatic.DistSqrtTwo2d[OverviewChunk + PlayerManager.addServer];
        }

        /// <summary>
        /// Вызывается на сервере, когда здоровье игрока достигает 0
        /// </summary>
        public void OnDeathPlayerServer(WorldServer worldServer, EnumDamageSource source, EntityLiving entityAttacks = null)
        {
            SetHealth(0);
            string message;
            if (entityAttacks != null && (source == EnumDamageSource.Player
                || source == EnumDamageSource.CauseMobDamage
                || source == EnumDamageSource.Piece
                || source == EnumDamageSource.Kill))
            {
                message = string.Format("Death {1} {0} {2}", source, name, entityAttacks.GetName());
            }
            else
            {
                message = string.Format("{1} {0}", source, name);
            }

            // Начало смерти
            SendPacket(new PacketS19EntityStatus(
                PacketS19EntityStatus.EnumStatus.Die, message));
            // Всем сообщение
            worldServer.Players.SendToAllMessage(message);
            // В лог сообщение
            worldServer.ServerMain.Log.Log(message);
        }

        /// <summary>
        /// Отправить сетевой пакет этому игроку
        /// </summary>
        public void SendPacket(IPacket packet) => ServerMain.ResponsePacket2(SocketClient, packet);

        /// <summary>
        /// Отправить системное сообщение конкретному игроку
        /// </summary>
        public void SendMessage(string message) => SendPacket(new PacketS3AMessage(message, true));

        /// <summary>
        /// Отправить атрибуты игрока клиенту
        /// </summary>
        public void SendPlayerAbilities() => SendPacket(new PacketS39PlayerAbilities(this));

        /// <summary>
        /// Пометка активности игрока
        /// </summary>
        public void MarkPlayerActive() => playerLastActiveTime = ServerMain.TickCounter;

        /// <summary>
        /// Обновить инвентарь игрока
        /// </summary>
        public void SendUpdateInventory() => SendPacket(new PacketS30WindowItems(Inventory.GetMainAndArmor()));

        /// <summary>
        /// Задать режим игры игрока
        /// </summary>
        /// <param name="gm">0 - выживания, 1 - творческий, 2 - наблюдателя</param>
        public void SetGameMode(int gm)
        {
            IsCreativeMode = gm == 1;
            NoClip = gm == 2;
            SetInvisible(NoClip);
            DisableDamage = AllowFlying = gm != 0;
            SendPlayerAbilities();
        }

        /// <summary>
        /// Изменить позицию игрока на стороне сервера
        /// </summary>
        public void SetPositionServer(vec3 position)
        {
            SetPosition(position);
            SendPacket(new PacketS08PlayerPosLook(Position, RotationYawHead, RotationPitch));
        }

        public override string ToString()
        {
            return "#" + Id + " " + name + "\r\n" + base.ToString();
        }
    }
}
