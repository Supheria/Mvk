using MvkServer.Command;
using MvkServer.Entity;
using MvkServer.Entity.List;
using MvkServer.Glm;
using MvkServer.NBT;
using MvkServer.Network;
using MvkServer.Network.Packets.Client;
using MvkServer.Network.Packets.Server;
using MvkServer.Util;
using MvkServer.World;
using MvkServer.World.Chunk;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;

namespace MvkServer.Management
{
    /// <summary>
    /// Объект управления пользователями на сервере
    /// КОНКРЕТНО пользователи, с которыми работаем по сети. НЕ ОБРАБАТЫВАТЬ сущности это будет в другом месте!!!
    /// </summary>
    public partial class PlayerManager
    {
        /// <summary>
        /// Серверный объект мира
        /// </summary>
        public WorldServer World { get; private set; }
        /// <summary>
        /// Менеджер команд
        /// </summary>
        private ManagerCommand managerCommand; 
        /// <summary>
        /// Объект логотладки
        /// </summary>
        private Profiler profiler;
        /// <summary>
        /// Дополнение для сервера обзора
        /// </summary>
        public static int addServer = 4;
        /// <summary>
        /// Обзор чанков круглый
        /// </summary>
        private bool isOverviewChunkCircle = true;
        /// <summary>
        /// Массив игроков EntityPlayerServer
        /// </summary>
        private List<EntityPlayerServer> players = new List<EntityPlayerServer>();
        /// <summary>
        /// Чанки игроков
        /// </summary>
        private Dictionary<vec2i, PlayerInstance> playerInstances = new Dictionary<vec2i, PlayerInstance>();
        /// <summary>
        /// Список PlayerInstance которые надо обновить
        /// </summary>
        private List<PlayerInstance> playerInstancesToUpdate = new List<PlayerInstance>();
        /// <summary>
        /// Этот список используется, когда чанк должен быть обработан (каждые 8000 тиков)
        /// </summary>
        private List<PlayerInstance> playerInstanceList = new List<PlayerInstance>();
        /// <summary>
        /// Список игроков которые надо запустить в ближайшем такте
        /// </summary>
        private List<EntityPlayerServer> playerStartList = new List<EntityPlayerServer>();
        /// <summary>
        /// Список игроков котрые надо выгрузить в ближайшем такте
        /// </summary>
        private List<ushort> playerRemoveList = new List<ushort>();

        /// <summary>
        /// время, которое используется, чтобы проверить, следует ли рассчитывать playerInstanceList 
        /// </summary>
        private long previousTotalWorldTime;

        /// <summary>
        /// Последний порядковый номер игрока с момента запуска
        /// </summary>
        private ushort lastPlayerId = 0;

        public PlayerManager(WorldServer worldServer)
        {
            World = worldServer;
            managerCommand = new ManagerCommand(World);
            profiler = new Profiler(worldServer.ServerMain.Log);
            addServer = isOverviewChunkCircle ? 6 : 4;
        }

        #region OverviewChunk

        /// <summary>
        /// Удалить фрагменты чанков вокруг игрока
        /// </summary>
        private void RemoveMountedMovingPlayer(EntityPlayerServer entityPlayer)
        {
            int radius = entityPlayer.OverviewChunk + addServer;
            int chx = entityPlayer.ChunkPosManaged.x;
            int chz = entityPlayer.ChunkPosManaged.y;
            int x, z;
            int xmin = chx - radius;
            int xmax = chx + radius;
            int zmin = chz - radius;
            int zmax = chz + radius;

            for (x = xmin; x <= xmax; x++)
            {
                for (z = zmin; z <= zmax; z++)
                {
                    PlayerInstance playerInstance = GetPlayerInstance(new vec2i(x, z), false);
                    if (playerInstance != null)
                    {
                        playerInstance.RemovePlayer(entityPlayer, true);
                    }
                }
            }
            entityPlayer.LoadingChunks.Clear();
        }

        /// <summary>
        /// обновлять фрагменты чанков вокруг игрока, перемещаемого логикой сервера
        /// </summary>
        public void UpdateMountedMovingPlayer(EntityPlayerServer entityPlayer)
        {
            bool isFilter = false;

            // Проверяем изменение обзора чанка
            if (!entityPlayer.SameOverviewChunkPrev())
            {
                int radius = entityPlayer.OverviewChunk;
                int radiusPrev = entityPlayer.OverviewChunkPrev;
                int radiusMax = Mth.Max(radius, radiusPrev);
                int chx = entityPlayer.ChunkPosManaged.x;
                int chz = entityPlayer.ChunkPosManaged.y;

                UpdateMountedMovingPlayerOverviewChunk(entityPlayer, chx, chz, radiusMax, radiusPrev, radius, false);
                if (radiusPrev != 0) radiusPrev += addServer;
                UpdateMountedMovingPlayerOverviewChunk(entityPlayer, chx, chz, radiusMax + addServer, radiusPrev, radius + addServer, true);

                entityPlayer.UpOverviewChunkPrev();
                isFilter = true;
            }

            // Проверяем смещение чанка на выбранный параметр, если есть начинаем обработку
            vec2i chunkCoor = entityPlayer.GetChunkPos();
            // Активация смещения при смещении количество чанков
            int bias = MvkGlobal.MOVING_CHUNK_BIAS;
            if (Mth.Abs(chunkCoor.x - entityPlayer.ChunkPosManaged.x) >= bias || Mth.Abs(chunkCoor.y - entityPlayer.ChunkPosManaged.y) >= bias)
            {
                // Смещение чанка
                int radiusPlay = entityPlayer.OverviewChunk;
                int radius = entityPlayer.OverviewChunk + addServer;
                int chx = chunkCoor.x;
                int chz = chunkCoor.y;
                int chmx = entityPlayer.ChunkPosManaged.x;
                int chmz = entityPlayer.ChunkPosManaged.y;
                int dx = chx - chmx;
                int dz = chz - chmz;

                // Проверка перемещения обзора чанков у клиента
                UpdateMountedMovingPlayerRadius(entityPlayer, chx, chz, chmx, chmz, radiusPlay, false);
                // Проверка перемещения обзора чанков в кэше сервера клиента (+2)
                UpdateMountedMovingPlayerRadius(entityPlayer, chx, chz, chmx, chmz, radius, true);

                entityPlayer.SetChunkPosManaged(new vec2i(chmx + dx, chmz + dz));
                isFilter = true;
            }

            if (isFilter)
            {
                FilterChunkLoadQueue(entityPlayer);
            }
        }

        /// <summary>
        /// Обнолвение фрагмента при изменении обзора, для клиента или для кэш сервера
        /// </summary>
        private void UpdateMountedMovingPlayerOverviewChunk(EntityPlayerServer entityPlayer, int chx, int chz,
            int radius, int radiusPrev, int radiusMin, bool isServer)
        {
            int xmin = chx - radius;
            int xmax = chx + radius;
            int zmin = chz - radius;
            int zmax = chz + radius;

            if (entityPlayer.OverviewChunk > entityPlayer.OverviewChunkPrev)
            {
                // Увеличиваем обзор
                int xmin2 = chx - radiusPrev;
                int xmax2 = chx + radiusPrev;
                int zmin2 = chz - radiusPrev;
                int zmax2 = chz + radiusPrev;

                if (isOverviewChunkCircle)
                {
                    if (isServer)
                    {
                        OverviewChunkAddServerCircle(entityPlayer, chx, chz, radius, chx, chz, radiusPrev);
                    }
                    else
                    {
                        OverviewChunkAddClientCircle(entityPlayer, chx, chz, radius, chx, chz, radiusPrev);
                    }
                }
                else
                {
                    if (isServer)
                    {
                        OverviewChunkAddServerSquare(entityPlayer, xmin, xmax, zmin, zmax, xmin2, xmax2, zmin2, zmax2);
                    }
                    else
                    {
                        OverviewChunkAddClientSquare(entityPlayer, xmin, xmax, zmin, zmax, xmin2, xmax2, zmin2, zmax2);
                    }
                }
            }
            else
            {
                // Уменьшить обзор
                int xmin2 = chx - radiusMin;
                int xmax2 = chx + radiusMin;
                int zmin2 = chz - radiusMin;
                int zmax2 = chz + radiusMin;
                if (isOverviewChunkCircle)
                {
                    OverviewChunkPutAwayCircle(entityPlayer, isServer, chx, chz, radius, chx, chz, radiusMin);
                }
                else
                {
                    OverviewChunkPutAwaySquare(entityPlayer, isServer, xmin, xmax, zmin, zmax, xmin2, xmax2, zmin2, zmax2);
                }
            }
        }

        /// <summary>
        /// Обнолвение фрагмента при перемещении, для клиента или для кэш сервера
        /// </summary>
        private void UpdateMountedMovingPlayerRadius(EntityPlayerServer entityPlayer,
            int chx, int chz, int chmx, int chmz, int radius, bool isServer)
        {
            int xmin = chx - radius;
            int xmax = chx + radius;
            int zmin = chz - radius;
            int zmax = chz + radius;
            int xmin2 = chmx - radius;
            int xmax2 = chmx + radius;
            int zmin2 = chmz - radius;
            int zmax2 = chmz + radius;

            if (isOverviewChunkCircle)
            {
                // Определяем добавление
                if (isServer)
                {
                    OverviewChunkAddServerCircle(entityPlayer, chx, chz, radius, chmx, chmz, radius);
                }
                else
                {
                    OverviewChunkAddClientCircle(entityPlayer, chx, chz, radius, chmx, chmz, radius);
                }
                // Определяем какие убираем
                OverviewChunkPutAwayCircle(entityPlayer, isServer, chmx, chmz, radius, chx, chz, radius);
            }
            else
            { 
                // Определяем добавление
                if (isServer)
                {
                    OverviewChunkAddServerSquare(entityPlayer, xmin, xmax, zmin, zmax, xmin2, xmax2, zmin2, zmax2);
                }
                else
                {
                    OverviewChunkAddClientSquare(entityPlayer, xmin, xmax, zmin, zmax, xmin2, xmax2, zmin2, zmax2);
                }
                // Определяем какие убираем
                OverviewChunkPutAwaySquare(entityPlayer, isServer, xmin2, xmax2, zmin2, zmax2, xmin, xmax, zmin, zmax);
            }
        }

        /// <summary>
        /// Удаляет все фрагменты из очереди загрузки фрагментов данного проигрывателя, которые не находятся в зоне видимости проигрывателя. 
        /// Обновляет список координат чанков и сортирует их с центра в даль для игрока entityPlayer
        /// </summary>
        private void FilterChunkLoadQueue(EntityPlayerServer entityPlayer)
        {
            GetPlayerInstance(entityPlayer.GetChunkPos(), true);
          // if (isOverviewChunkCircle) return; 
           // c кругом чанки иногда бывают пустые чанки, не догружены клиенту!!!
           // хотя с квадратом на увеличении и уменьшении обзора тоже получил пустые чанки
           // 24.03.2023

            Hashtable mapLoaded = entityPlayer.LoadedChunks.CloneMap();
            Hashtable mapLoading = entityPlayer.LoadingChunks.CloneMap();
            entityPlayer.LoadedChunks.Clear();
            entityPlayer.LoadingChunks.Clear();

            if (entityPlayer.DistSqrt != null)
            {
                // +4 а не addServer, так как при радиусе, в краях радиуса, 4 мало, 6 норм.
                // Но тут надо срезать, чтоб круга не было, типа квадрат со срезаными углами, 
                // чтоб сократить память.
                int radius = entityPlayer.OverviewChunk + 4;
                int count = MvkStatic.DistSqrt37Light[entityPlayer.OverviewChunk + addServer];
                vec2i chunkPosManaged = entityPlayer.ChunkPosManaged;
                vec2i vec;
                vec2i pos;
                for (int d = 0; d < count; d++)
                {
                    vec = entityPlayer.DistSqrt[d];
                    if (vec.x >= -radius && vec.x <= radius && vec.y >= -radius && vec.y <= radius)
                    {
                        pos = vec + chunkPosManaged;
                        if (mapLoaded.ContainsKey(pos))
                        {
                            entityPlayer.LoadedChunks.Add(pos);
                        }
                        entityPlayer.LoadingChunks.Add(pos);
                    }
                }
            }
        }

        ///// <summary>
        ///// Удалить фрагменты чанков вокруг игрока
        ///// </summary>
        //private void RemoveMountedMovingPlayer(EntityPlayerServer entityPlayer)
        //{
        //    if (isOverviewChunkCircle) RemoveMountedMovingPlayerCircle(entityPlayer);
        //    else RemoveMountedMovingPlayerSquare(entityPlayer);
        //}

        ///// <summary>
        ///// обновлять фрагменты чанков вокруг игрока, перемещаемого логикой сервера
        ///// </summary>
        //public void UpdateMountedMovingPlayer(EntityPlayerServer entityPlayer)
        //{
        //    if (isOverviewChunkCircle) UpdateMountedMovingPlayerCircle(entityPlayer);
        //    else UpdateMountedMovingPlayerSquare(entityPlayer);
        //}

        #endregion

        #region Player

        /// <summary>
        /// Отправить всем сообщение
        /// </summary>
        public void SendToAllMessage(string message, bool isConsole = true) 
            => SendToAll(new PacketS3AMessage(message, isConsole));

        /// <summary>
        /// Отправить всем игрокам пакет
        /// </summary>
        public void SendToAll(IPacket packet)
        {
            for (int i = 0; i < players.Count; i++)
            {
                players[i].SendPacket(packet);
            }
        }

        /// <summary>
        /// Отправить всем игрокам пакет, которые видят этот чанк
        /// </summary>
        public void SendToAllPlayersWatchingChunk(IPacket packet, vec2i currentChunk)
        {
            PlayerInstance playerInstance = GetPlayerInstance(currentChunk, false);
            if (playerInstance != null)
            {
                playerInstance.SendToAllPlayersWatchingChunk(packet);
            }
        }

        /// <summary>
        /// Проверка на игрока с таким же именем в игре
        /// </summary>
        /// <returns>false - игрок уже играет</returns>
        private bool CheckPlayerAdd(EntityPlayerServer entityPlayer)
        {
            EntityPlayerServer entityPlayerOld = GetPlayerId(entityPlayer.Id);
            return entityPlayerOld == null;
        }

        /// <summary>
        /// Добавить игрока
        /// </summary>
        private void PlayerAdd(EntityPlayerServer entityPlayer)
        {
            // Лог запуска игрока
            World.ServerMain.Log.Log("server.player.entry {0} [{1}]", entityPlayer.GetName(), entityPlayer.UUID);

            //entityPlayer.SetPosLook(new vec3(random.Next(32) - 56, 0, random.Next(32) - 16), -0.9f, -.8f);
            //SpawnPositionCheck(entityPlayer);

            ReadEntityPlayerFromFile(entityPlayer);
            
            entityPlayer.SetChunkPosManaged(entityPlayer.GetChunkPos());
            //entityPlayer.SetOverviewChunk(entityPlayer.OverviewChunk, 1);
            vec2i posCh = entityPlayer.GetChunkPos();
            
            // 1
            players.Add(entityPlayer);
            entityPlayer.UpPositionChunk();
            // Добавляем игрок в конкретный чанк
            GetPlayerInstance(entityPlayer.PositionChunk, true).AddPlayer(entityPlayer, true, true);

            //entityPlayer.FlagSpawn = true;
            // Тут проверяем место положение персонажа, и заносим при запуске
            World.SpawnEntityInWorld(entityPlayer);
            //entityPlayer.FlagSpawn = false;
        }

        /// <summary>
        /// Присвоить порядковый номер сущности
        /// </summary>
        public void NewEntity(EntityBase entity) => entity.SetEntityId(++lastPlayerId);

        /// <summary>
        /// Проверка вертикально позиции для спавна
        /// </summary>
        private void SpawnPositionCheck(EntityPlayerServer entity)
        {
            ChunkBase chunk = World.GetChunk(entity.GetChunkPos());
            if (chunk != null)
            {
                int x = (int)entity.Position.x & 15;
                int z = (int)entity.Position.z & 15;
                int y0 = (int)entity.Position.y;
                bool b = true;
                for (int y = ChunkBase.COUNT_HEIGHT_BLOCK; y > 0; y--)
                {
                    if (!chunk.GetBlockState(x, y, z).GetBlock().IsAir)
                    {
                        y0 = y + 1;
                        b = false;
                        break;
                    }
                }
                if (b) y0++;
                entity.SetPosition(new vec3(entity.Position.x, y0, entity.Position.z));
                entity.FlagSpawn = false;
            }
            else
            {
                entity.FlagSpawn = true;
            }
        }

        public void SpawnPositionTest(EntityPlayerServer entityPlayer)
        {
            // TODO::2023-03-16 Стартовая позиция!
            Rand random = new Rand();
            //entityPlayer.SetPosLook(new vec3(random.Next(32) + 56, 0, random.Next(32) + 16), -0.9f, -.8f);
            //entityPlayer.SetPosLook(new vec3(66, 0, -15), -0.9f, -.8f);
            //entityPlayer.SetGameMode(1);

            // РОБИНЗОН
            //entityPlayer.SetPosLook(new vec3(random.Next(16) - 160, 0, random.Next(16) + 368), -0.9f, -.8f);
            entityPlayer.SetPosLook(new vec3(random.Next(16) - 400, 0, random.Next(16) + 704), 3.14f, .2f);
            ///
            //entityPlayer.SetPosLook(new vec3(random.Next(32) - 950046, 0, random.Next(32) + 950046) , -0.9f, -.8f);
            SpawnPositionCheck(entityPlayer);
            // entityPlayer.SetPosLook(new vec3(random.Next(-16, 16) - 50040, 30, random.Next(-16, 16)), -0.9f, -.8f);

        }

        /// <summary>
        /// Удалить всех игроков при остановки сервера
        /// </summary>
        public void PlayersRemoveStopingServer()
        {
            for (int i = 0; i < players.Count; i++)
            {
                playerRemoveList.Add(players[i].Id);
            }
            // Надо отработать Update
        }

        /// <summary>
        /// Удалить игрока
        /// </summary>
        public void PlayerRemove(Socket socket)
        {
            EntityPlayerServer entityPlayer = GetPlayerSocket(socket);
            if (entityPlayer != null)
            {
                playerRemoveList.Add(entityPlayer.Id);
            }
        }

        /// <summary>
        /// Удалить игрока
        /// </summary>
        private void PlayerRemove(ushort id)
        {
            EntityPlayerServer entityPlayer = GetPlayerId(id);

            if (entityPlayer != null)
            {
                World.ServerMain.Log.Log("server.player.entry.repeat {0} [{1}]", entityPlayer.GetName(), entityPlayer.UUID);
                World.Tracker.RemovePlayerFromTrackers(entityPlayer);
                RemoveMountedMovingPlayer(entityPlayer);
                World.RemoveEntity(entityPlayer);
                players.Remove(entityPlayer);
                WriteEntityPlayerToFile(entityPlayer);
                //ResponsePacketAll(new PacketSF1Disconnect(entityPlayer.Id), entityPlayer.Id);
            }
        }

        /// <summary>
        /// По сокету найти игрока
        /// </summary>
        public EntityPlayerServer GetPlayerSocket(Socket socket)
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].SocketClient == socket) return players[i];
            }
            return null;
        }

        /// <summary>
        /// По id найти игрока
        /// </summary>
        public EntityPlayerServer GetPlayerId(ushort id)
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].Id == id) return players[i];
            }
            return null;
        }

        /// <summary>
        /// Получить основного игрока который создал сервер
        /// </summary>
        public EntityPlayerServer GetEntityPlayerMain() => GetPlayerSocket(null);

        /// <summary>
        /// Количество игроков
        /// </summary>
        public int PlayerCount => players.Count;
        /// <summary>
        /// ПустойЮ нет игроков
        /// </summary>
        public bool IsEmpty() => players.Count == 0;

        /// <summary>
        /// Находится ли хоть один игрок в этом чанке
        /// </summary>
        /// <param name="pos">позиция чанка</param>
        public bool IsTherePlayerChunk(vec2i pos) => GetPlayerInstance(pos, false) != null;

        /// <summary>
        /// Находится ли игрок в этом чанке
        /// </summary>
        /// <param name="entityPlayer">игрок</param>
        /// <param name="pos">позиция чанка</param>
        public bool IsPlayerWatchingChunk(EntityPlayerServer entityPlayer, vec2i pos)
        {
            PlayerInstance chunkPlayer = GetPlayerInstance(pos, false);
                return chunkPlayer != null && chunkPlayer.Contains(entityPlayer) 
                && entityPlayer.LoadedChunks.Contains(chunkPlayer.CurrentChunk);
        }

        #endregion

        /// <summary>
        /// Получить объект связи чанков с игроками если его нет, создать объект при isNew=true
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="isNew">если null создать</param>
        private PlayerInstance GetPlayerInstance(vec2i pos, bool isNew)
        {
            PlayerInstance playerInstance = playerInstances.ContainsKey(pos) ?
                playerInstance = playerInstances[pos] as PlayerInstance : null;

            if (isNew && playerInstance == null)
            {
                playerInstance = new PlayerInstance(this, pos);
                
                playerInstances.Add(pos, playerInstance);
                playerInstanceList.Add(playerInstance);
            }

            return playerInstance;
        }

        /// <summary>
        /// Проверяем на дублирование игрока, если такой-же есть ошибка и возращаем false
        /// </summary>
        public bool CheckPlayer(EntityPlayerServer entityPlayer)
        {
            if (CheckPlayerAdd(entityPlayer))
            {
                entityPlayer.SendPacket(new PacketSF0Connection(true));
                return true;
            }
            else
            {
                World.ServerMain.Log.Log("server.player.entry.duplicate {0} [{1}]", entityPlayer.GetName(), entityPlayer.UUID);
                // Игрок с таким именем в игре!
                entityPlayer.SendPacket(new PacketSF0Connection("world.player.duplicate"));
                return false;
            }
        }

        /// <summary>
        /// Запуска игрока
        /// </summary>
        public void LoginStart(EntityPlayerServer entityPlayer)
        {
            // Проверка игрока с повторным именем прошла
            if (entityPlayer.SocketClient == null)
            {
                // основной игрок, запуск сервера!!!
                PlayerAdd(entityPlayer);
                entityPlayer.FlagBeginSpawn = true;
                // После того как загрузиться запуститься метод LoginStart() для запуска пакета PacketS12Success
                World.ServerMain.StartServer();
                    
            } else
            {
                // Для тактового запуска игрока
                playerStartList.Add(entityPlayer);
            }
        }

        /// <summary>
        /// Запуск игрока из игрового такта, это сетевые игроки
        /// </summary>
        private void LoginStartTick(EntityPlayerServer entityPlayer)
        {
            if (entityPlayer.SocketClient != null)
            {
                // Сетевой игрок подключён, сразу отправляем пакет PacketJoinGame
                PlayerAdd(entityPlayer);
                ResponsePacketJoinGame(entityPlayer);
            }
        }

        /// <summary>
        /// Начало игры, прошли проверку на игрока, теперь в игре
        /// </summary>
        public void ResponsePacketJoinGame(EntityPlayerServer player)
        {
            player.SendPacket(new PacketS02JoinGame(player.Id, player.UUID));
            player.SendPacket(new PacketS08PlayerPosLook(player.Position, player.RotationYawHead, player.RotationPitch));
            player.SendPacket(new PacketS03TimeUpdate(World.ServerMain.TickCounter));
            player.SendPlayerAbilities();
            player.SendUpdateInventory();
        }
        
        /// <summary>
        /// Пакет настроек клиента
        /// </summary>
        public void ClientSetting(Socket socket, PacketC15ClientSetting packet)
        {
            EntityPlayerServer player = GetPlayerSocket(socket);
            if (player != null)
            {
                // Смена обзора
                player.SetOverviewChunk(packet.GetOverviewChunk());
            }
        }

        /// <summary>
        /// Пакет команд и чата клиента
        /// </summary>
        public void ClientMessage(Socket socket, PacketC14Message packet)
        {
            EntityPlayerServer player = GetPlayerSocket(socket);
            string message = packet.GetCommandSender().GetMessage();

            if (packet.IsCommand())
            {
                // Пишем в лог все
                World.Log.Log("{0}>>{1}", player != null ? player.GetName() : "[NULL]", message);
                message = managerCommand.ExecutionCommand(packet.GetCommandSender().SetPlayer(player));
                if (player != null && message != "")
                {
                    player.SendMessage(message);
                }
            }
            else
            {
                if (player != null)
                {
                    World.Log.Log("<{0}>: {1}", player.GetName(), message);
                    SendToAllMessage(ChatStyle.Aqua + player.GetName() + ": " + ChatStyle.Reset + message, false);
                }
            }
        }

        /// <summary>
        /// Пакет статуса клиента
        /// </summary>
        public void ClientStatus(Socket socket, PacketC16ClientStatus.EnumState state)
        {
            EntityPlayerServer player = GetPlayerSocket(socket);
            if (player != null)
            {
                if (state == PacketC16ClientStatus.EnumState.Respawn)
                {
                    // Респавн игрока
                    player.FlagSpawn = true;
                }
            }
        }

        public void Update()
        {
            // Удаляем игроков
            while (playerRemoveList.Count > 0)
            {
                ushort id = playerRemoveList[0];
                playerRemoveList.RemoveAt(0);
                PlayerRemove(id);

                for (int i = 0; i < playerStartList.Count; i++)
                {
                    if (playerStartList[i].Id == id)
                    {
                        playerRemoveList.RemoveAt(i);
                        break;
                    }
                }
            }
            // Добавляем игроков
            while (playerStartList.Count > 0)
            {
                EntityPlayerServer entityPlayer = playerStartList[0];
                playerStartList.RemoveAt(0);
                LoginStartTick(entityPlayer);
            }
            // Обновление игроков спавна
            for (int i = 0; i < players.Count; i++)
            {
                players[i].UpdateSpawn();
            }
            
        }
        /// <summary>
        /// Обновляет все экземпляры игроков, которые необходимо обновить 
        /// </summary>
        public void UpdatePlayerInstances()
        {
            profiler.StartSection("LoadingChunks");
            try
            {
                for (int i = 0; i < players.Count; i++)
                {
                    EntityPlayerServer player = players[i];
                    // Количество чанков для загрузки или генерации за такт, было 5
                    //int load = MvkGlobal.COUNT_CHUNK_LOAD_OR_GEN_TPS;
                    int number = 0;
                    // Всего проверки чанков за такт
                    //int all = 100; // 2023-04-29 убрал, вроде из-за него вылетали не прогрузившие чанки
                    //World.ChunkPrServ.LoadGenRequestCount

                    while (player.LoadingChunks.Count > 0 
                        && number < MvkGlobal.COUNT_CHUNK_LOAD_OR_GEN_TPS)// && all > 0)
                    {
                        vec2i posCh = player.LoadingChunks.FirstRemove();
                        if (!World.ChunkPrServ.IsChunkLoaded(posCh))
                        {
                            World.ChunkPrServ.LoadGenAdd(posCh);
                            number++;
                        }
                        //all--;
                    }
                }
            }
            catch (Exception ex)
            {
                World.ServerMain.Log.Error("PlayerManager.UpdatePlayerInstances.LoadingChunks");
                Logger.Crach(ex);
                throw;
            }
            profiler.EndSection();

            //Hashtable playersClone = players.Clone() as Hashtable;
            //foreach (EntityPlayerServer player in playersClone.Values)
            //{
            //    try
            //    {
            //        profiler.StartSection("UpdatePlayer");
            //        player.UpdatePlayer();
            //        profiler.EndSection();
            //    }
            //    catch
            //    {
            //        World.ServerMain.Log.Error("PlayerManager.UpdatePlayerInstances.UpdatePlayer");
            //        throw;
            //    }
            //}

            // Чистка чанков по времени
            uint time = World.ServerMain.TickCounter;
            //if (time - previousTotalWorldTime > MvkGlobal.CHUNK_CLEANING_TIME)
            //{
            //    previousTotalWorldTime = time;
            //    try
            //    {
            //        profiler.StartSection("ChunkCoordPlayers");
            //        // Занести 
            //        Hashtable htCh = playerInstances.Clone() as Hashtable;
            //        foreach (PlayerInstance ccp in htCh.Values)
            //        {
            //            ccp.FixOverviewChunk();
            //            if (ccp.CountPlayers() == 0)
            //            {
            //                playerInstances.Remove(ccp.Position);
            //                world.ChunkPrServ.DroppedChunks.Add(ccp.Position);
            //            }
            //        }
            //        // Добавить в список удаляющих чанков которые не полного статуса
            //        world.ChunkPrServ.DroopedChunkStatusMin(players);
            //        profiler.EndSection();
            //    }
            //    catch
            //    {
            //        world.ServerMain.Log.Error("PlayerManager.UpdatePlayerInstances.ChunkCoordPlayers");
            //        throw;
            //    }
            //}

            //int j = 0;
            //while(playerInstanceListLoad.Count > 0 && j < 20)
            //{
            //    World.ChunkPrServ.LoadChunk(playerInstanceListLoad[0]);
            //    playerInstanceListLoad.RemoveAt(0);
            //    j++;
            //}



            if (time - previousTotalWorldTime > MvkGlobal.CHUNK_CLEANING_TIME) // 8000
            {
                previousTotalWorldTime = time;
                World.ChunkPrServ.CleanChunksNotPlayer();
                for (int i = 0; i < playerInstanceList.Count; i++)
                {
                    playerInstanceList[i].Update();
                    playerInstanceList[i].ProcessChunk();
                }
                playerInstancesToUpdate.Clear();
            }
            else
            {
                while(playerInstancesToUpdate.Count > 0)
                {
                    playerInstancesToUpdate[0].Update();
                    playerInstancesToUpdate.RemoveAt(0);
                }
            }

        }

        /// <summary>
        /// Удалить игрока с конкретного чанка 
        /// </summary>
        /// <param name="pos">позиция чанка</param>
        /// <param name="player">игрок</param>
        public void PlayerInstancesRemove(vec2i pos, PlayerInstance player)
        {
            playerInstances.Remove(pos);
            playerInstanceList.Remove(player);
        }
        public void PlayerInstancesToUpdateRemove(PlayerInstance player) => playerInstancesToUpdate.Remove(player);
        public void PlayerInstancesToUpdateAdd(PlayerInstance player) => playerInstancesToUpdate.Add(player);

        /// <summary>
        /// Количество чанков у всех игроков
        /// </summary>
        public int CountPlayerInstances() => playerInstances.Count;

        /// <summary>
        /// Флаг блока который был изменён
        /// </summary>
        public void FlagBlockForUpdate(BlockPos blockPos)
        {
            PlayerInstance playerInstance = GetPlayerInstance(blockPos.GetPositionChunk(), false);
            if (playerInstance != null)
            {
                playerInstance.FlagBlockForUpdate(blockPos.GetPosition0());
            }
        }

        /// <summary>
        /// Флаг блока который был изменён
        /// </summary>
        public void FlagBlockForUpdate(int x, int y, int z)
        {
            PlayerInstance playerInstance = GetPlayerInstance(new vec2i(x >> 4, z >> 4), false);
            if (playerInstance != null)
            {
                playerInstance.FlagBlockForUpdate(new vec3i(x & 15, y, z & 15));
            }
        }

        ///// <summary>
        ///// Флаг псевдочанка который был изменён
        ///// </summary>
        ///// <param name="ch">координаты чанка</param>
        ///// <param name="y">координата псевдочанка</param>
        //public void FlagChunkForUpdate(vec2i ch, int y)
        //{
        //    PlayerInstance playerInstance = GetPlayerInstance(ch, false);
        //    if (playerInstance != null)
        //    {
        //        playerInstance.FlagChunkForUpdate(y);
        //    }
        //}

        /// <summary>
        /// Отправить процесс разрущения блока
        /// </summary>
        /// <param name="breakerId">id сущности который ломает блок</param>
        /// <param name="pos">позиция блока</param>
        /// <param name="progress">сколько тактом блок должен разрушаться</param>
        public void SendBlockBreakProgress(int breakerId, BlockPos pos, int progress)
        {
            for (int i = 0; i < players.Count; i++)
            {
                EntityPlayerServer entityPlayer = players[i];
                if (entityPlayer != null && entityPlayer.Id != breakerId)
                {
                    if (pos.DistanceNotSqrt(entityPlayer.Position) < 1024f)
                    {
                        // растояние меньше 32 блоков
                        entityPlayer.SendPacket(new PacketS25BlockBreakAnim(breakerId, pos, progress));
                    }
                }
            }
        }


        /// <summary>
        /// Список чанков для отладки
        /// </summary>
        [Obsolete("Список чанков только для отладки")]
        public List<vec2i> GetListDebug()
        {
            List<vec2i> list = new List<vec2i>();
            foreach (PlayerInstance playerInstance in playerInstances.Values)
            {
                if (playerInstance.CountPlayers() > 0) list.Add(playerInstance.CurrentChunk);
            }
            return list;
        }

        public string ToStringDebug()
        {
            string strPlayers = "";
            if (PlayerCount > 0)
            {
                for (int i = 0; i < players.Count; i++)
                {
                    EntityPlayerServer entity = players[i];
                    strPlayers += entity.GetName() + " [p" + entity.Ping + "|" + (entity.IsDead ? "Dead" : ("h" + entity.GetHealth())) + (entity.IsSprinting() ? "S" : "-") + "]";
                }
            }
            return strPlayers;
        }

        /// <summary>
        /// Записать данные игрок в файл
        /// </summary>
        private void WriteEntityPlayerToFile(EntityPlayerServer entityPlayer)
        {
            TagCompound nbt = new TagCompound();
            entityPlayer.WriteEntityToNBT(nbt);
            World.File.PlayerDataWrite(nbt, entityPlayer.UUID);
        }

        /// <summary>
        /// Прочесть данные игрока с файла
        /// </summary>
        private bool ReadEntityPlayerFromFile(EntityPlayerServer entityPlayer)
        {
            TagCompound nbt = World.File.PlayerDataRead(entityPlayer.UUID);
            if (nbt == null)
            {
                SpawnPositionTest(entityPlayer);
                return false;
            }
            entityPlayer.ReadEntityFromNBT(nbt);
            return true;
        }
    }
}
