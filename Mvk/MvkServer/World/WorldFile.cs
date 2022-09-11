using MvkServer.Glm;
using MvkServer.NBT;
using System.IO;

namespace MvkServer.World
{
    /// <summary>
    /// Объект работы с файлами мира для сохранения
    /// </summary>
    public class WorldFile
    {
        /// <summary>
        /// Имя файла информации о мире
        /// </summary>
        public const string NAME_FILE_WORLD = "level.dat";

        /// <summary>
        /// Мир сервера
        /// </summary>
        public WorldServer World { get; private set; }
        /// <summary>
        /// Игровой слот
        /// </summary>
        public int Slot { get; private set; }
        /// <summary>
        /// Путь к сохранению миров
        /// </summary>
        public string PathCore { get; private set; }
        /// <summary>
        /// Путь к корню мира
        /// </summary>
        public string PathWorld { get; private set; }
        /// <summary>
        /// Путь к игрокам
        /// </summary>
        public string PathPlayers { get; private set; }
        /// <summary>
        /// Путь к регионам
        /// </summary>
        public string PathRegions { get; private set; }

        private readonly string slot;

        public WorldFile()
        {
            PathCore = "Saves" + Path.DirectorySeparatorChar;
        }

        public WorldFile(WorldServer world, int slot) : this()
        {
            World = world;
            Slot = slot;
            this.slot = slot.ToString();
            PathWorld = PathCore + slot + Path.DirectorySeparatorChar;
            PathPlayers = PathWorld + "Players" + Path.DirectorySeparatorChar;
            PathRegions = PathWorld + "Regions" + Path.DirectorySeparatorChar;

            CheckPath(PathCore);
            CheckPath(PathWorld);
            CheckPath(PathPlayers);
            CheckPath(PathRegions);
        }

        public string GetNameFileWorld() => "";

        /// <summary>
        /// Проверка пути, если нет, то создаём
        /// </summary>
        public void CheckPath(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// Получить название мира
        /// </summary>
        public string NameWorldData(string path)
        {
            string pf = path + Path.DirectorySeparatorChar + NAME_FILE_WORLD;
            if (File.Exists(pf))
            {
                TagCompound nbt = NBTTools.ReadFromFile(pf, true);
                long tick = nbt.GetLong("DayTime");
                
                long h = tick / 72000;
                long m = (tick - h * 72000) / 1200;
                return nbt.GetString("LevelName") + " " 
                    + h + ":" + (m < 10 ? "0" + m : m.ToString());
            }
            //сломан
            return "gui.world.broken";
        }

        /// <summary>
        /// Сохранить инфу мира
        /// </summary>
        public void WorldInfoWrite(TagCompound nbt)
        {
            NBTTools.WriteToFile(nbt, PathWorld + NAME_FILE_WORLD, true);
        }

        /// <summary>
        /// Загрузить инфу мира
        /// </summary>
        public TagCompound WorldInfoRead(WorldServer world)
        {
            string pathFile = PathWorld + NAME_FILE_WORLD;
            if (File.Exists(pathFile))
            {
                return NBTTools.ReadFromFile(pathFile, true);
            }
            return null;
        }

        /// <summary>
        /// Сохранить данные игрока, name это UUID игрока
        /// </summary>
        public void PlayerDataWrite(TagCompound nbt, string name)
        {
            NBTTools.WriteToFile(nbt, PathPlayers + name, true);
        }

        /// <summary>
        /// Загрузить данные игрока, name это UUID игрока
        /// </summary>
        public TagCompound PlayerDataRead(string name)
        {
            string pathFile = PathPlayers + name;
            if (File.Exists(pathFile))
            {
                return NBTTools.ReadFromFile(pathFile, true);
            }
            return null;
        }

        

        /// <summary>
        /// Записать чанк
        /// </summary>
        //public void ChunkDataWrite(TagCompound nbt, RegionFile region, vec2i posCh)
        //{
        //    //region.
        //    //string path = GetCorePath() + slot
        //    //    + Path.DirectorySeparatorChar + TREE_PLAERS + Path.DirectorySeparatorChar;
        //    //CheckPath(path);
        //    //string pf = path + name;
        //    //NBTTools.Write(nbt, pf, true);
        //}

        ///// <summary>
        ///// Прочесть чанк
        ///// </summary>
        ///// <returns></returns>
        //public TagCompound ChunkDataRead(RegionFile region, vec2i posCh)
        //{

        //    return null;
        //}

        /// <summary>
        /// Удалить папку с файлами
        /// Directory.Delete(path, true); // выплёвывает исключение
        /// </summary>
        public static void DeleteDirectory(string path)
        {
            // https://overcoder.net/q/11892/%D0%BD%D0%B5%D0%B2%D0%BE%D0%B7%D0%BC%D0%BE%D0%B6%D0%BD%D0%BE-%D1%83%D0%B4%D0%B0%D0%BB%D0%B8%D1%82%D1%8C-%D0%BA%D0%B0%D1%82%D0%B0%D0%BB%D0%BE%D0%B3-%D1%81-%D0%BF%D0%BE%D0%BC%D0%BE%D1%89%D1%8C%D1%8E-directorydelete-%D0%BF%D1%83%D1%82%D1%8C-%D0%B8%D1%81%D1%82%D0%B8%D0%BD%D0%B0
            if (Directory.Exists(path))
            {
                string[] files = Directory.GetFiles(path);
                string[] dirs = Directory.GetDirectories(path);

                foreach (string file in files)
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }
                foreach (string dir in dirs)
                {
                    DeleteDirectory(dir);
                }

                bool b = true;
                while (b)
                {
                    try
                    {
                        Directory.Delete(path, false);
                        b = false;
                    }
                    catch { }
                }
            }
        }
    }
}
