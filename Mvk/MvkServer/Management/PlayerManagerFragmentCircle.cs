using MvkServer.Entity.List;
using MvkServer.Glm;
using MvkServer.Util;
using System.Collections;
using System.Collections.Generic;

namespace MvkServer.Management
{
    /// <summary>
    /// Дополнение к объекту управления пользователями на сервере
    /// Фрагменты чанков вокруг игрока в виде круга
    /// </summary>
    public partial class PlayerManager
    {
        /// <summary>
        /// Добавить чанки обзора на сервере при необходимости
        /// </summary>
        /// <param name="entityPlayer">Объект игрока</param>
        /// <param name="chx">координата X центра чанка</param>
        /// <param name="chz">координата X центра чанка</param>
        /// <param name="radius">радиус обзора в чанках</param>
        /// <param name="xMinCheck">минимальная кордината проверки по X</param>
        /// <param name="xMaxCheck">максимальная кордината проверки по X</param>
        /// <param name="zMinCheck">минимальная кордината проверки по Z</param>
        /// <param name="zMaxCheck">максимальная кордината проверки по Z</param>
        private void OverviewChunkAddServerCircle(EntityPlayerServer entityPlayer,
            int chx, int chz, int radius,
            int chx2, int chz2, int radius2)
        {
            int i;
            vec2i vec;
            int count = MvkStatic.DistSqrt37Light[radius];
            List<vec2i> listMask = new List<vec2i>();
            int light = radius - 1;
            for (i = 0; i < count; i++)
            {
                vec = MvkStatic.DistSqrt37[i];
                if (vec.x < light && vec.x > -light && vec.y < light && vec.y > -light)
                {
                    listMask.Add(new vec2i(vec.x + chx, vec.y + chz));
                }
            }

            count = MvkStatic.DistSqrt37Light[radius2];
            light = radius2 - 1;
            for (i = 0; i < count; i++)
            {
                vec = MvkStatic.DistSqrt37[i];
                if (vec.x < light && vec.x > -light && vec.y < light && vec.y > -light)
                {
                    listMask.Remove(new vec2i(vec.x + chx2, vec.y + chz2));
                }
            }
            count = listMask.Count;
            for (i = 0; i < count; i++)
            {
                GetPlayerInstance(listMask[i], true).AddPlayer(entityPlayer, true, false);
            }
        }

      //  private ArrayMvk<vec2i> listMask = new ArrayMvk<vec2i>(1369);

        /// <summary>
        /// Добавить чанки обзора для клиента при необходимости
        /// </summary>
        /// <param name="entityPlayer">Объект игрока</param>
        /// <param name="chx">координата X центра чанка</param>
        /// <param name="chz">координата X центра чанка</param>
        /// <param name="radius">радиус обзора в чанках</param>
        /// <param name="xMinCheck">минимальная кордината проверки по X</param>
        /// <param name="xMaxCheck">максимальная кордината проверки по X</param>
        /// <param name="zMinCheck">минимальная кордината проверки по Z</param>
        /// <param name="zMaxCheck">максимальная кордината проверки по Z</param>
        private void OverviewChunkAddClientCircle(EntityPlayerServer entityPlayer,
            int chx, int chz, int radius,
            int chx2, int chz2, int radius2)
        {
            int i;
            vec2i vec;
            int count = MvkStatic.DistSqrt37Light[radius];
            List<vec2i> listMask = new List<vec2i>();
            for (i = 0; i < count; i++)
            {
                vec = MvkStatic.DistSqrt37[i];
                listMask.Add(new vec2i(vec.x + chx, vec.y + chz));
            }

            count = MvkStatic.DistSqrt37Light[radius2];
            for (i = 0; i < count; i++)
            {
                vec = MvkStatic.DistSqrt37[i];
                listMask.Remove(new vec2i(vec.x + chx2, vec.y + chz2));
            }
            count = listMask.Count;
            for (i = 0; i < count; i++)
            {
                entityPlayer.LoadedChunks.Add(GetPlayerInstance(listMask[i], true).CurrentChunk);
            }
        }

        /// <summary>
        /// Убрать чанки обзора при необходимости
        /// </summary>
        /// <param name="entityPlayer">Объект игрока</param>
        /// <param name="isServer">Флаг для сервера ли</param>
        /// <param name="chx">координата X центра чанка</param>
        /// <param name="chz">координата X центра чанка</param>
        /// <param name="radius">радиус обзора в чанках</param>
        /// <param name="xMinCheck">минимальная кордината проверки по X</param>
        /// <param name="xMaxCheck">максимальная кордината проверки по X</param>
        /// <param name="zMinCheck">минимальная кордината проверки по Z</param>
        /// <param name="zMaxCheck">максимальная кордината проверки по Z</param>
        private void OverviewChunkPutAwayCircle(EntityPlayerServer entityPlayer, bool isServer,
            int chx, int chz, int radius,
            int chx2, int chz2, int radius2)
        {
            int i;
            vec2i vec;
            int count = MvkStatic.DistSqrt37Light[radius];
            List<vec2i> listMask = new List<vec2i>();

            if (isServer)
            {
                int light = radius - 1;
                for (i = 0; i < count; i++)
                {
                    vec = MvkStatic.DistSqrt37[i];
                    if (vec.x < light && vec.x > -light && vec.y < light && vec.y > -light)
                    {
                        listMask.Add(new vec2i(vec.x + chx, vec.y + chz));
                    }
                }

                count = MvkStatic.DistSqrt37Light[radius2];
                light = radius2 - 1;
                for (i = 0; i < count; i++)
                {
                    vec = MvkStatic.DistSqrt37[i];
                    if (vec.x < light && vec.x > -light && vec.y < light && vec.y > -light)
                    {
                        listMask.Remove(new vec2i(vec.x + chx2, vec.y + chz2));
                    }
                }
            }
            else
            {
                for (i = 0; i < count; i++)
                {
                    vec = MvkStatic.DistSqrt37[i];
                    listMask.Add(new vec2i(vec.x + chx, vec.y + chz));
                }

                count = MvkStatic.DistSqrt37Light[radius2];
                for (i = 0; i < count; i++)
                {
                    vec = MvkStatic.DistSqrt37[i];
                    listMask.Remove(new vec2i(vec.x + chx2, vec.y + chz2));
                }
            }

            count = listMask.Count;
            PlayerInstance playerInstance;
            for (i = 0; i < count; i++)
            {
                playerInstance = GetPlayerInstance(listMask[i], false);
                if (playerInstance != null)
                {
                    playerInstance.RemovePlayer(entityPlayer, isServer);
                }
            }

            //int x, z, i;
            //PlayerInstance playerInstance;
            //vec2i vec;
            //int count = MvkStatic.DistSqrt37Light[radius];
            //for (i = 0; i < count; i++)
            //{
            //    vec = MvkStatic.DistSqrt37[i];
            //    x = vec.x + chx;
            //    z = vec.y + chz;
            //    if (x < xMinCheck || x > xMaxCheck || z < zMinCheck || z > zMaxCheck)
            //    { 
            //        playerInstance = GetPlayerInstance(new vec2i(x, z), false);
            //        if (playerInstance != null)
            //        {
            //            playerInstance.RemovePlayer(entityPlayer, isServer);
            //        }
            //    }
            //}
        }
    }
}
