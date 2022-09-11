using MvkServer.Entity;
using MvkServer.Entity.Player;
using MvkServer.NBT;
using MvkServer.Util;
using System;
using System.Diagnostics;

namespace MvkServer.World
{
    /// <summary>
    /// Объект информации мира, который умеет сохранять
    /// </summary>
    public class WorldInfo
    {
        /// <summary>
        /// Мир сервера
        /// </summary>
        public WorldServer World { get; private set; }
        /// <summary>
        /// Зерно генерации случайных чисел
        /// </summary>
        public long Seed { get; private set; } = 2;
        /// <summary>
        /// Тестовый параметр, мир весь для креатива
        /// </summary>
        public bool IsCreativeMode { get; private set; }
        /// <summary>
        /// Имя мира
        /// </summary>
        private readonly string nameWorld;

        public WorldInfo(WorldServer world)
        {
            World = world;
            try
            {
                TagCompound nbt = World.File.WorldInfoRead(world);
                if (nbt == null)
                {
                    nbt = new TagCompound();
                    DefaultInfo(nbt, World.File.Slot);
                }
                nameWorld = nbt.GetString("LevelName");
                Seed = nbt.GetInt("Seed");
                World.ServerMain.SetDayTime((uint)nbt.GetLong("DayTime"));
                IsCreativeMode = nbt.GetBool("Creative");
            }
            catch (Exception ex)
            {
                Logger.Crach(ex);
            }
        }

        /// <summary>
        /// Значения мира по умолчанию
        /// </summary>
        /// <param name="nbt">Объект NBT</param>
        /// <param name="slot">Слот мира</param>
        private void DefaultInfo(TagCompound nbt, int slot)
        {
            nbt.SetString("LevelName", "World #" + slot);
            nbt.SetLong("Seed", slot);
            nbt.SetLong("DayTime", 0);
            nbt.SetBool("Creative", slot > 3);
        }

        /// <summary>
        /// Сохранить инфу мира
        /// </summary>
        public void WriteInfo()
        {
            try
            {
                TagCompound nbt = new TagCompound();
                nbt.SetString("LevelName", nameWorld);
                nbt.SetLong("Seed", Seed);
                nbt.SetLong("DayTime", World.ServerMain.TickCounter);
                nbt.SetBool("Creative", IsCreativeMode);

                World.File.WorldInfoWrite(nbt);

                World.Log.Log("server.saving.worlds");
                // Сохраняем чанки в регионы 
                World.ChunkPrServ.SaveChunks();
                // Сохраняем регионы в файл
                World.Regions.WriteToFile(true);
            }
            catch(Exception ex)
            {
                Logger.Crach(ex);
            }
        }
    }
}
