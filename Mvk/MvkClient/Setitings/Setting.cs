using MvkClient.Renderer;
using System;
using System.IO;

namespace MvkClient.Setitings
{
    public class Setting
    {
        /// <summary>
        /// Имя файла настроек
        /// </summary>
        protected static string fileName = "options.ini";

        /// <summary>
        /// Общая громкость
        /// </summary>
        public static int SoundVolume { get; set; } = 100;
        /// <summary>
        /// Громкость музыки
        /// </summary>
        public static int MusicVolume { get; set; } = 100;
        /// <summary>
        /// Желаемый FPS
        /// </summary>
        public static int Fps { get; set; } = 60;
        /// <summary>
        /// Обзор чанков
        /// </summary>
        public static int OverviewChunk { get; set; } = 16;
        /// <summary>
        /// Имя игрока
        /// </summary>
        public static string Nickname { get; set; } = "Ant";
        /// <summary>
        /// Язык
        /// </summary>
        public static int Language { get; set; } = 1;
        /// <summary>
        /// IP адрес сервера
        /// </summary>
        public static string IpAddress { get; set; } = "127.0.0.1";
        /// <summary>
        /// Плавное освещение
        /// </summary>
        public static bool SmoothLighting { get; set; } = true;
        /// <summary>
        /// Размер интерфеса в опциях
        /// </summary>
        public static int SizeInterfaceOptions { get; private set; } = 1;
        /// <summary>
        /// Размер интерфеса с учётом размера окна
        /// </summary>
        public static int SizeInterface { get; private set; } = 1;
        /// <summary>
        /// Зерно случайного числа для мира
        /// </summary>
        public static long SeedBegin { get; set; } = 0;

        public static float ToFloatSoundVolume() => SoundVolume / 100f;
        public static float ToFloatMusicVolume() => MusicVolume / 100f;

        /// <summary>
        /// Задать разер интерфейса
        /// </summary>
        public static void SetSizeInterface(int size)
        {
            SizeInterfaceOptions = size;
            UpSizeInterface();
        }

        /// <summary>
        /// Обновить размер интерфейса за счёт окна
        /// </summary>
        public static void UpSizeInterface()
        {
            if (SizeInterfaceOptions > 1 && GLWindow.WindowHeight < 880)
            {
                SizeInterface = 1;
            }
            else
            {
                SizeInterface = SizeInterfaceOptions;
            }
        }

        /// <summary>
        /// Загрузить настройки
        /// </summary>
        public static void Load()
        {
            if (File.Exists(fileName))
            {
                //получить доступ к  существующему либо создать новый
                using (StreamReader file = new StreamReader(fileName))
                {
                    while (true)
                    {
                        // Читаем строку из файла во временную переменную.
                        string strLine = file.ReadLine();

                        // Если достигнут конец файла, прерываем считывание.
                        if (strLine == null) break;

                        // комментарий
                        if (strLine.Length == 0 || strLine.Substring(0, 1) == "#") continue;

                        string[] vs = strLine.Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries);

                        if (Check(vs, "SoundVolume")) SoundVolume = int.Parse(vs[1]);
                        else if (Check(vs, "MusicVolume")) MusicVolume = int.Parse(vs[1]);
                        else if (Check(vs, "Fps")) Fps = int.Parse(vs[1]);
                        else if (Check(vs, "OverviewChunk")) OverviewChunk = int.Parse(vs[1]);
                        else if (Check(vs, "Nickname")) Nickname = vs[1].ToString();
                        else if (Check(vs, "Language")) Language = int.Parse(vs[1]);
                        else if (Check(vs, "IpAddress")) IpAddress = vs[1].ToString();
                        else if (Check(vs, "SmoothLighting")) SmoothLighting = int.Parse(vs[1]) == 1;
                        else if (Check(vs, "SizeInterface")) SetSizeInterface(int.Parse(vs[1]));
                        else if (Check(vs, "SeedBegin")) SeedBegin = int.Parse(vs[1]);
                    }
                }
            }
        }

        /// <summary>
        /// Проверить команду
        /// </summary>
        protected static bool Check(string[] vs, string name) 
            => vs.Length == 2 && vs[0].Length > name.Length - 1 && vs[0].Substring(0, name.Length) == name;

        /// <summary>
        /// Сохранить настройки
        /// </summary>
        public static void Save()
        {
            using (StreamWriter file = new StreamWriter(fileName))
            {
                file.WriteLine("# Mvk Project - (c)2021");
                file.WriteLine("# File Created: {0:dd.MM.yyyy HH:mm.ss}\r\n", DateTime.Now);

                file.WriteLine("SoundVolume: " + SoundVolume.ToString());
                file.WriteLine("MusicVolume: " + MusicVolume.ToString());
                file.WriteLine("Fps: " + Fps.ToString());
                file.WriteLine("OverviewChunk: " + OverviewChunk.ToString());
                file.WriteLine("Nickname: " + Nickname.ToString());
                file.WriteLine("Language: " + Language.ToString());
                file.WriteLine("IpAddress: " + IpAddress.ToString());
                file.WriteLine("SmoothLighting: " + (SmoothLighting ? "1" : "0"));
                file.WriteLine("SizeInterface: " + SizeInterfaceOptions.ToString());
                file.WriteLine("SeedBegin: " + SeedBegin.ToString());
                file.Close();
            }
        }
    }
}
