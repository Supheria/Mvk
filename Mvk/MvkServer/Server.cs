using MvkServer.Entity.List;
using MvkServer.Glm;
using MvkServer.Network;
using MvkServer.Network.Packets.Server;
using MvkServer.Util;
using MvkServer.World;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MvkServer
{
    public class Server
    {
        /// <summary>
        /// Объект лога
        /// </summary>
        public Logger Log { get; protected set; }
        /// <summary>
        /// Дополнительный поток для генерации и загрузки мира
        /// </summary>
        public bool IsAddLoopGen { get; private set; } = false;
        /// <summary>
        /// Указывает, запущен сервер или нет. Установите значение false, чтобы инициировать завершение работы. 
        /// </summary>
        private bool serverRunning = true;
        /// <summary>
        /// Сервер уже в рабочем цикле
        /// </summary>
        public bool ServerIsInRunLoop { get; protected set; } = false;
        /// <summary>
        /// Указывает другим классам, что сервер безопасно остановлен 
        /// </summary>
        public bool ServerStopped { get; protected set; } = false;
        /// <summary>
        /// Увеличивается каждый тик 
        /// </summary>
        public uint TickCounter { get; protected set; }
        /// <summary>
        /// Серверный объект мира
        /// </summary>
        public WorldServer World { get; protected set; }

        public EntityPlayerServer test;

        /// <summary>
        /// Поток генерации мира
        /// </summary>
        private bool threadGenLoop = false;

        /// <summary>
        /// Устанавливается при появлении предупреждения «Не могу угнаться», которое срабатывает снова через 15 секунд. 
        /// </summary>
        protected long timeOfLastWarning;
        /// <summary>
        /// Хранение тактов за 1/5 секунды игры, для статистики определения среднего времени такта
        /// </summary>
        protected long[] tickTimeArray = new long[4];

        /// <summary>
        /// Принято пакетов за секунду
        /// </summary>
        private int rx = 0;
        /// <summary>
        /// Передано пакетов за секунду
        /// </summary>
        private int tx = 0;
        /// <summary>
        /// Принято пакетов за предыдущую секунду
        /// </summary>
        private int rxPrev = 0;
        /// <summary>
        /// Передано пакетов за предыдущую секунду
        /// </summary>
        private int txPrev = 0;

        /// <summary>
        /// статус запуска сервера
        /// </summary>
        protected string strNet = "";
        /// <summary>
        /// Часы для Tps
        /// </summary>
        protected Stopwatch stopwatchTps = new Stopwatch();
        /// <summary>
        /// Для перевода тактов в мили секунды Stopwatch.Frequency / 1000;
        /// </summary>
        protected long frequencyMs;
        /// <summary>
        /// Сокет для сети
        /// </summary>
        protected SocketServer server;
        /// <summary>
        /// Объект работы с пакетами
        /// </summary>
        protected ProcessServerPackets packets;
        /// <summary>
        /// Пауза в игре, только для одиночной версии
        /// </summary>
        protected bool isGamePaused = false;
        /// <summary>
        /// Обзор чанков при старте, отталкиваемся от основного клиентак
        /// </summary>
        private int overviewChunk;
        

        /// <summary>
        /// Инициализация
        /// </summary>
        /// <returns>вёрнуть цифру тактов загрузки</returns>
        public void Initialize(int slot)
        {
            Log = new Logger("");
            Log.Log("server.runing slot={0}", slot);
            World = new WorldServer(this, slot);
            packets = new ProcessServerPackets(this);
            frequencyMs = Stopwatch.Frequency / 1000;
            stopwatchTps.Start();
            // Отправляем основному игроку пинг
            ResponsePacket2(null, new PacketSF0Connection(""));
        }

        #region Net

        /// <summary>
        /// Получить истину запущен ли сетевой сервер
        /// </summary>
        public bool IsRunNet() => server != null && server.IsConnected;

        /// <summary>
        /// Запустить на сервере сеть
        /// </summary>
        public void RunNet()
        {
            if (!IsRunNet())
            {
                server = new SocketServer(32021);
                server.ReceivePacket += (sender, e) => LocalReceivePacket(e.Packet.WorkSocket, e.Packet.Bytes);
                server.Receive += Server_Receive;
                server.Run();
                isGamePaused = false;
                Log.Log("server.run.net");
            }
        }

        private void Server_Receive(object sender, ServerPacketEventArgs e)
        {
            if (e.Packet.Status == StatusNet.Disconnect)
            {
                World.Players.PlayerRemove(e.Packet.WorkSocket);
            }
            else if (e.Packet.Status == StatusNet.Connect)
            {
                // Отправляем игроку пинг
                ResponsePacket2(e.Packet.WorkSocket, new PacketSF0Connection(""));
            }
        }

        /// <summary>
        /// Локальная передача пакета
        /// </summary>
        public void LocalReceivePacket(Socket socket, byte[] buffer)
        {
            rx++;
            packets.ReceiveBuffer(socket, buffer);
        }

        /// <summary>
        /// Обновить количество клиентов
        /// </summary>
        public void UpCountClients() => strNet = IsRunNet() ? "net[" + World.Players.PlayerCount + "]" : "";


        /// <summary>
        /// Запрос завершонный
        /// </summary>
        public bool SendCompleted() => server != null && server.SendCompleted;

        /// <summary>
        /// Отправить пакет клиенту
        /// </summary>
        public void ResponsePacket2(Socket socket, IPacket packet)
        {
            Task.Factory.StartNew(() =>
            {
                using (MemoryStream writeStream = new MemoryStream())
                {
                    using (StreamBase stream = new StreamBase(writeStream))
                    {
                        writeStream.WriteByte(ProcessPackets.GetId(packet));
                        packet.WritePacket(stream);
                        byte[] buffer = writeStream.ToArray();
                        tx++;
                        ServerPacket spacket = new ServerPacket(socket, buffer);
                        if (socket != null)
                        {
                            server.SetActiveSocket(socket);
                            server.SendPacket(buffer);
                        }
                        else
                        {
                            OnRecievePacket(new ServerPacketEventArgs(spacket));
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Отправить пакет всем клиентам
        /// </summary>
        public void ResponsePacketAll(IPacket packet)
        {
            if (World != null)
            {
                World.Players.SendToAll(packet);
            }
        }

        #endregion


        /// <summary>
        /// Задать обзор чанков от стартового клиента
        /// </summary>
        public void SetOverviewChunk(int overviewChunk) => this.overviewChunk = overviewChunk;

        /// <summary>
        /// Запрос остановки сервера
        /// </summary>
        public void ServerStop()
        {
            if (IsAddLoopGen)
            {
                threadGenLoop = false; // Сначала останавливаем поток генерации
            }
            else
            {
                serverRunning = false;
            }
        }

        /// <summary>
        /// Метод должен работать в отдельном потоке
        /// </summary>
        private void ServerLoop()
        {
            try
            {
                StartServerLoop();
                long currentTime = stopwatchTps.ElapsedMilliseconds;
                long cacheTime = 0;
                Log.Log("server.runed");

                // Рабочий цикл сервера
                while (serverRunning)
                {
                    long realTime = stopwatchTps.ElapsedMilliseconds;
                    // Разница в милсекунда с прошлого такта
                    long differenceTime = realTime - currentTime;
                    if (differenceTime < 0) differenceTime = 0;

                    // Если выше 2 секунд задержка
                    if (differenceTime > 2000 && currentTime - timeOfLastWarning >= 15000)
                    {
                        // Не успеваю!Изменилось ли системное время, или сервер перегружен?
                        // Отставание на {differenceTime} мс, пропуск тиков({differenceTime / 50}) 
                        Log.Log("Не успеваю! Отставание на {0} мс, пропуск тиков {1}", differenceTime, differenceTime / 50);
                        differenceTime = 2000;
                        timeOfLastWarning = currentTime;
                    }

                    cacheTime += differenceTime;
                    currentTime = realTime;

                    
                    //if (false)
                    //{
                    //    // проверка игроков, что все спят
                    //    // Почему-то если спят, то отрабатывать накопленные такты не надо
                    //    Tick();
                    //    cacheTime = 0;
                    //}
                    //else
                    {
                        while (cacheTime > 50)
                        {
                            cacheTime -= 50;
                            if (!isGamePaused) Tick();
                        }
                    }

                    Thread.Sleep(Mth.Max(1, 50 - (int)cacheTime));
                    ServerIsInRunLoop = true;
                }
            }
            catch (Exception ex)
            {
                Logger.Crach(ex);
            }
            finally
            {
                threadGenLoop = false;
                serverRunning = false;
                StopServerLoop();
            }
        }

        /// <summary>
        /// Запустить сервер в отдельном потоке
        /// </summary>
        public void StartServer()
        {
            Thread myThread = new Thread(ServerLoop);
            myThread.Start();
        }

        protected void StartServerLoop()
        {
            serverRunning = true;
            // Запускаем постоянный поток генерации мира
            if (IsAddLoopGen)
            {
                Thread myThread = new Thread(ServerGenLoop);
                myThread.Start();
            }

            EntityPlayerServer entityPlayer = World.Players.GetEntityPlayerMain();
            vec2i pos = entityPlayer != null ? entityPlayer.GetChunkPos() : new vec2i(0, 0);

            int radius = Mth.Min(MvkGlobal.OVERVIEW_CHUNK_START, overviewChunk);
            OnLoadStepCount((radius + radius + 1) * (radius + radius + 1));

            // Запуск чанков для старта
            for (int x = -radius; x <= radius; x++)
            {
                for (int z = -radius; z <= radius; z++)
                {
                    World.ChunkPrServ.LoadingChunk(new vec2i(pos.x + x, pos.y + z));
                    OnLoadingTick();
                }
            }
            return;
        }

        /// <summary>
        /// Сохраняет все необходимые данные для подготовки к остановке сервера
        /// </summary>
        private void StopServerLoop()
        {
            Log.Log("server.stoping");
            World.WorldStoping();
            packets.Clear();
            if (server != null) server.Stop();
            OnLogDebug("");
            ServerStopped = true;
            Log.Log("server.stoped");
            Log.Close();
            OnStoped();
        }

        /// <summary>
        /// Основная функция вызывается run () в каждом цикле. 
        /// </summary>
        protected void Tick()
        {
            long realTime = stopwatchTps.ElapsedTicks;
            TickCounter++;

            packets.Update();
            // Выполнение такта

            //if (test != null)
            //{
            //    World.Players.LoginStart(test);
            //    test = null;
            //}
            //Random r = new Random();
            //int rn = r.Next(100);
            //Thread.Sleep(rn);

            // Прошла минута, или 1200 тактов
            if (TickCounter % 1200 == 0)
            {
                Log.Tick();
            }


            // Прошла секунда, или 20 тактов
            if (TickCounter % 20 == 0)
            {
                UpCountClients();

                if (TickCounter % 600 == 0)
                {
                    // раз в 30 секунд обновляем тик с клиентом
                    ResponsePacketAll(new PacketS03TimeUpdate(TickCounter));
                }

                //this.serverConfigManager.sendPacketToAllPlayersInDimension(new S03PacketTimeUpdate(
                //var4.getTotalWorldTime(), 
                //var4.getWorldTime(), 
                //var4.getGameRules().getGameRuleBooleanValue("doDaylightCycle")),

                //var4.provider.getDimensionId());
                rxPrev = rx;
                txPrev = tx;
                rx = 0;
                tx = 0;
            }

            try
            {
                World.Tick();
            }
            catch (Exception e)
            {
                Log.Error("Server.Error.Tick {0}", e.Message);
                throw;
            }

            try
            {
                World.UpdateEntities();
            }
            catch (Exception e)
            {
                Log.Error("Server.Error.UpdateEntities {0}", e.Message);
                throw;
            }

            World.UpdateTrackedEntities();


            // ---------------
            long differenceTime = stopwatchTps.ElapsedTicks - realTime;

            // Прошла 1/5 секунда, или 4 такта
            if (TickCounter % 4 == 0)
            {
                // лог статистика за это время
                OnLogDebug(ToStringDebugTps());

                if (MvkGlobal.IS_DRAW_DEBUG_CHUNK)
                {
                    // отладка чанков
                    DebugChunk chunks = new DebugChunk()
                    {
                        listChunkServer = World.ChunkPr.GetListDebug(),
                        listChunkPlayers = World.Players.GetListDebug(),
                        isRender = true
                    };
                    OnLogDebugCh(chunks);
                }

                //tickRx = 0;
                //tickTx = 0;
            }

            if (TickCounter % 900 == 0) // 900 = 45 сек
            {
                // Запуск сохранения чанков
                World.profiler.StartSection("BeginSaving");
                World.ChunkPrServ.BeginSaving();
                World.profiler.EndSection();
            }

            World.profiler.StartSection("TickSaving");
            // Сохраняем пакет чанков в регионы 
            World.ChunkPrServ.TickSaving();
            World.profiler.EndStartSection("SaveToFileRegions");
            // Сохраняем регионы в файл
            World.ChunkPrServ.SavingRegions();
            World.profiler.EndSection();

            // фиксируем время выполнения такта
            tickTimeArray[TickCounter % 4] = differenceTime;
        }

        /// <summary>
        /// Отдельный поток для генерации чанков
        /// </summary>
        private void ServerGenLoop()
        {
            try
            {
                threadGenLoop = true;
                Log.Log("server.threadGenLoop.run");

                while (threadGenLoop && serverRunning)
                {
                    // Тут генерация чанков
                    World.ChunkPrServ.LoadGenRequest();
                    Thread.Sleep(1);
                }

                Log.Log("server.threadGenLoop.stop");
                // После остановки потока генерации останавливаем основной поток
                serverRunning = false;
            }
            catch (Exception ex)
            {
                Logger.Crach(ex);
            }
            finally
            {
                threadGenLoop = false;
                serverRunning = false;
            }
        }

        /// <summary>
        /// Задать паузу для одиночной игры
        /// </summary>
        public void SetGamePauseSingle(bool value)
        {
            isGamePaused = !IsRunNet() && value;
            OnLogDebug(ToStringDebugTps());
        }

        /// <summary>
        /// Получить время в милисекундах с момента запуска проекта
        /// </summary>
        public long Time() => stopwatchTps.ElapsedMilliseconds;
        /// <summary>
        /// Задать игровое время в тактах
        /// </summary>
        public void SetDayTime(uint dayTime) => TickCounter = dayTime; 

        /// <summary>
        /// Строка для дебага, формируется по запросу
        /// </summary>
        protected string ToStringDebugTps()
        {
            // Среднее время выполнения 4 тактов, должно быть меньше 50
            float averageTime = Mth.Average(tickTimeArray) / frequencyMs;
            // TPS за последние 4 тактов (1/5 сек), должен быть 20
            float tps = averageTime > 50f ? 50f / averageTime * 20f : 20f;
            return string.Format("{0:0.00} tps {1:0.00} ms Rx {2} Tx {3} {4}{6}\r\n{5}", 
                tps, averageTime, rxPrev, txPrev, strNet, World.ToStringDebug(), isGamePaused ? "PAUSE" : "");
        }

        #region Event

        /// <summary>
        /// Событие такта для объекта с ключом
        /// </summary>
        public event EventHandler LoadingTick;
        protected virtual void OnLoadingTick() => LoadingTick?.Invoke(this, new EventArgs());
        /// <summary>
        /// Событие количество шигов загрузки
        /// </summary>
        public event IntEventHandler LoadStepCount;
        protected virtual void OnLoadStepCount(int count) => LoadStepCount?.Invoke(this, new IntEventArgs(count));
        /// <summary>
        /// Событие остановки сервера
        /// </summary>
        public event EventHandler Stoped;
        protected virtual void OnStoped() => Stoped?.Invoke(this, new EventArgs());
        /// <summary>
        /// Событие лог для дебага
        /// </summary>
        public event StringEventHandler LogDebug;
        protected virtual void OnLogDebug(string text) => LogDebug?.Invoke(this, new StringEventArgs(text));
        /// <summary>
        /// Событие лог для дебага листа чанков
        /// </summary>
        public event ObjectEventHandler LogDebugCh;
        protected virtual void OnLogDebugCh(DebugChunk list) => LogDebugCh?.Invoke(this, new ObjectEventArgs(list));

        /// <summary>
        /// Событие получить от сервера пакет
        /// </summary>
        public event ServerPacketEventHandler RecievePacket;
        protected void OnRecievePacket(ServerPacketEventArgs e) => RecievePacket?.Invoke(this, e);

        #endregion
    }
}
