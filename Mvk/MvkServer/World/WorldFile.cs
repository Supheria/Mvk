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

        private const string TREE_PLAERS = "Players";

        /// <summary>
        /// Мир сервера
        /// </summary>
        public WorldServer World { get; private set; }
        /// <summary>
        /// Игровой слот
        /// </summary>
        public int Slot { get; private set; }

        private readonly string slot;

        /// <summary>
        /// Путь к сохранению миров
        /// </summary>
        private readonly string corePath;

        public WorldFile()
        {
            corePath = "Saves" + Path.DirectorySeparatorChar;
        }

        public WorldFile(WorldServer world, int slot) : this()
        {
            World = world;
            Slot = slot;
            this.slot = slot.ToString();
        }

        /// <summary>
        /// Путь к сохранению миров
        /// </summary>
        public string GetCorePath() => corePath;

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
                TagCompound nbt = NBTTools.Read(pf, true);
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
            string path = GetCorePath() + slot;
            string pf = path + Path.DirectorySeparatorChar + NAME_FILE_WORLD;
            CheckPath(path);
            NBTTools.Write(nbt, pf, true);
        }

        /// <summary>
        /// Загрузить инфу мира
        /// </summary>
        public TagCompound WorldInfoRead(WorldServer world)
        {
            string path = GetCorePath() + slot;
            string pf = path + Path.DirectorySeparatorChar + NAME_FILE_WORLD;
            if (File.Exists(pf))
            {
                return NBTTools.Read(pf, true);
            }
            return null;
        }

        /// <summary>
        /// Сохранить данные игрока, name это UUID игрока
        /// </summary>
        public void PlayerDataWrite(TagCompound nbt, string name)
        {
            string path = GetCorePath() + slot 
                + Path.DirectorySeparatorChar + TREE_PLAERS + Path.DirectorySeparatorChar;
            CheckPath(path);
            string pf = path + name;
            NBTTools.Write(nbt, pf, true);
        }

        /// <summary>
        /// Загрузить данные игрока, name это UUID игрока
        /// </summary>
        public TagCompound PlayerDataRead(string name)
        {
            string path = GetCorePath() + slot
                + Path.DirectorySeparatorChar + TREE_PLAERS + Path.DirectorySeparatorChar;
            string pf = path + name;
            if (File.Exists(pf))
            {
                return NBTTools.Read(pf, true);
            }
            return null;
        }

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

                Directory.Delete(path, false);
            }
        }
    }
}
