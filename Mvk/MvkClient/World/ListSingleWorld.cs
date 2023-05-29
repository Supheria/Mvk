﻿using MvkAssets;
using MvkServer.World;
using System.IO;

namespace MvkClient.World
{
    /// <summary>
    /// Объект списков миров для одиночной игры 5 шт
    /// </summary>
    public class ListSingleWorld
    {
        /// <summary>
        /// Основной клиент
        /// </summary>
        public Client ClientMain { get; private set; }
        /// <summary>
        /// Список названий миров
        /// </summary>
        public string[] NameWorlds { get; private set; } = new string[5];
        /// <summary>
        /// Пустые мирв
        /// </summary>
        public bool[] EmptyWorlds { get; private set; } = new bool[5];

        private WorldFile worldFile;


        public ListSingleWorld(Client client)
        {
            ClientMain = client;
            worldFile = new WorldFile();
        }

        /// <summary>
        /// Загрузка миров
        /// </summary>
        public void Initialize(int remove)
        {
            string path;
            int number;
            for (int i = 0; i < 5; i++)
            {
                number = i + 1;
                path = worldFile.PathCore + number.ToString();
                if (remove != number && Directory.Exists(path))
                {
                    NameWorlds[i] = worldFile.NameWorldData(path);
                    EmptyWorlds[i] = false;
                }
                else
                {
                    NameWorlds[i] = GetNameSlot(i + 1);
                    EmptyWorlds[i] = true;
                }
            }
        }

        /// <summary>
        /// Удалить мировой слот
        /// </summary>
        public void WorldRemove(int slot) 
            => WorldFile.DeleteDirectory(worldFile.PathCore + slot.ToString());

        /// <summary>
        /// Получить название мира по слоту
        /// </summary>
        public static string GetNameSlot(int slot, bool empty = true)
        {
            string prefix = empty ? ".empty" : "";
            string result = "";
            switch (slot)
            {
                case 1: result = "gui.world.name.robinson" + prefix; break;
                case 2: result = "gui.world.name.sandbox" + prefix; break;
                default: result = "gui.world.empty"; break;
            }
            return Language.T(result);
        }
    }
}
