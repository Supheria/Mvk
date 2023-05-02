using MvkServer.Entity.List;
using MvkServer.Glm;

namespace MvkServer.Management
{
    /// <summary>
    /// Дополнение к объекту управления пользователями на сервере
    /// Фрагменты чанков вокруг игрока в виде квадрата
    /// </summary>
    public partial class PlayerManager
    {
        /// <summary>
        /// Добавить чанки обзора на сервере при необходимости
        /// </summary>
        /// <param name="entityPlayer">Объект игрока</param>
        /// <param name="xMinFor">минимальная кордината массива по X</param>
        /// <param name="xMaxFor">максимальная кордината массива по X</param>
        /// <param name="zMinFor">минимальная кордината массива по Z</param>
        /// <param name="zMaxFor">максимальная кордината массива по Z</param>
        /// <param name="xMinCheck">минимальная кордината проверки по X</param>
        /// <param name="xMaxCheck">максимальная кордината проверки по X</param>
        /// <param name="zMinCheck">минимальная кордината проверки по Z</param>
        /// <param name="zMaxCheck">максимальная кордината проверки по Z</param>
        private void OverviewChunkAddServerSquare(EntityPlayerServer entityPlayer, 
            int xMinFor, int xMaxFor, int zMinFor, int zMaxFor,
            int xMinCheck, int xMaxCheck, int zMinCheck, int zMaxCheck)
        {
            int x, z;
            for (x = xMinFor; x <= xMaxFor; x++)
            {
                for (z = zMinFor; z <= zMaxFor; z++)
                {
                    if (x < xMinCheck || x > xMaxCheck || z < zMinCheck || z > zMaxCheck)
                    {
                        GetPlayerInstance(new vec2i(x, z), true).AddPlayer(entityPlayer, true, false);
                    }
                }
            }
        }

        /// <summary>
        /// Добавить чанки обзора для клиента при необходимости
        /// </summary>
        /// <param name="entityPlayer">Объект игрока</param>
        /// <param name="xMinFor">минимальная кордината массива по X</param>
        /// <param name="xMaxFor">максимальная кордината массива по X</param>
        /// <param name="zMinFor">минимальная кордината массива по Z</param>
        /// <param name="zMaxFor">максимальная кордината массива по Z</param>
        /// <param name="xMinCheck">минимальная кордината проверки по X</param>
        /// <param name="xMaxCheck">максимальная кордината проверки по X</param>
        /// <param name="zMinCheck">минимальная кордината проверки по Z</param>
        /// <param name="zMaxCheck">максимальная кордината проверки по Z</param>
        private void OverviewChunkAddClientSquare(EntityPlayerServer entityPlayer,
            int xMinFor, int xMaxFor, int zMinFor, int zMaxFor,
            int xMinCheck, int xMaxCheck, int zMinCheck, int zMaxCheck)
        {
            int x, z;
            for (x = xMinFor; x <= xMaxFor; x++)
            {
                for (z = zMinFor; z <= zMaxFor; z++)
                {
                    if (x < xMinCheck || x > xMaxCheck || z < zMinCheck || z > zMaxCheck)
                    {
                        entityPlayer.LoadedChunks.Add(GetPlayerInstance(new vec2i(x, z), true).CurrentChunk);
                    }
                }
            }
        }

        /// <summary>
        /// Убрать чанки обзора при необходимости
        /// </summary>
        /// <param name="entityPlayer">Объект игрока</param>
        /// <param name="isServer">Флаг для сервера ли</param>
        /// <param name="xMinFor">минимальная кордината массива по X</param>
        /// <param name="xMaxFor">максимальная кордината массива по X</param>
        /// <param name="zMinFor">минимальная кордината массива по Z</param>
        /// <param name="zMaxFor">максимальная кордината массива по Z</param>
        /// <param name="xMinCheck">минимальная кордината проверки по X</param>
        /// <param name="xMaxCheck">максимальная кордината проверки по X</param>
        /// <param name="zMinCheck">минимальная кордината проверки по Z</param>
        /// <param name="zMaxCheck">максимальная кордината проверки по Z</param>
        private void OverviewChunkPutAwaySquare(EntityPlayerServer entityPlayer, bool isServer,
            int xMinFor, int xMaxFor, int zMinFor, int zMaxFor,
            int xMinCheck, int xMaxCheck, int zMinCheck, int zMaxCheck)
        {
            int x, z;
            PlayerInstance playerInstance;
            for (x = xMinFor; x <= xMaxFor; x++)
            {
                for (z = zMinFor; z <= zMaxFor; z++)
                {
                    if (x < xMinCheck || x > xMaxCheck || z < zMinCheck || z > zMaxCheck)
                    {
                        playerInstance = GetPlayerInstance(new vec2i(x, z), false);
                        if (playerInstance != null)
                        {
                            playerInstance.RemovePlayer(entityPlayer, isServer);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Обнолвение фрагмента при изменении обзора, для клиента или для кэш сервера
        /// </summary>
        //private void UpdateMountedMovingPlayerOverviewChunkCircle(EntityPlayerServer entityPlayer, int chx, int chz,
        //    int radius, int radiusPrev, int radiusMin, bool isCache)
        //{
        //    int x, z, i, count;
        //    vec2i vec;
        //    count = MvkStatic.DistSqrt37Light[radius];
        //    for (i = 0; i < count; i++)
        //    {
        //        vec = MvkStatic.DistSqrt37[i];
        //        x = vec.x + chx;
        //        z = vec.y + chz;
        //        if (entityPlayer.OverviewChunk > entityPlayer.OverviewChunkPrev)
        //        {
        //            if (x < chx - radiusPrev || x > chx + radiusPrev || z < chz - radiusPrev || z > chz + radiusPrev)
        //            {
        //                // Увеличиваем обзор
        //                if (isCache)
        //                {
        //                    GetPlayerInstance(new vec2i(x, z), true).AddPlayer(entityPlayer, true, false);
        //                }
        //                else
        //                {
        //                    entityPlayer.LoadedChunks.Add(GetPlayerInstance(new vec2i(x, z), true).CurrentChunk);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            if (x < chx - radiusMin || x > chx + radiusMin || z < chz - radiusMin || z > chz + radiusMin)
        //            {
        //                // Уменьшить обзор
        //                PlayerInstance playerInstance = GetPlayerInstance(new vec2i(x, z), false);
        //                if (playerInstance != null)
        //                {
        //                    playerInstance.RemovePlayer(entityPlayer, isCache);
        //                }
        //            }
        //        }

        //    }
        //}

    }
}
