using MvkServer.NBT;
using System;
using System.IO;

namespace MvkServer.World
{
    public class RegionFile
    {
        /// <summary>
        /// Сылка на объект мира сервера
        /// </summary>
        private readonly WorldServer world;
        /// <summary>
        /// Координата региона X = chX >> 5
        /// </summary>
        private readonly int regionX;
        /// <summary>
        /// Координата региона Z = chZ >> 5
        /// </summary>
        private readonly int regionZ;

        /// <summary>
        /// Путь к папке с регионами
        /// </summary>
        private readonly string path;
        /// <summary>
        /// Имя файла
        /// </summary>
        private readonly string fileName;
        /// <summary>
        /// Массив смещений чанков (z << 5 | x)
        /// </summary>
        private readonly int[] offsets = new int[1024];
        /// <summary>
        /// Массив буфферов всех чанков в регионе (z << 5 | x)
        /// </summary>
        private readonly byte[][] buffers = new byte[1024][];

        public RegionFile(WorldServer world, int x, int z)
        {
            this.world = world;
            regionX = x;
            regionZ = z;
            path = world.File.PathRegions;
            fileName = "r." + x + "." + z + ".mca";
            ReadCreate();
        }

        /// <summary>
        /// Прочесть, если нету создать
        /// </summary>
        private void ReadCreate()
        {
            string pathFile = path + fileName;
            try
            {
                if (File.Exists(pathFile))
                {
                    // Читаем
                    using (FileStream fileStream = new FileStream(pathFile, FileMode.Open))
                    using (NBTStream stream = new NBTStream())
                    {
                        fileStream.Seek(0, SeekOrigin.Begin);
                        fileStream.CopyTo(stream);
                        stream.Seek(0, SeekOrigin.Begin);
                        int i, offset, size;
                        for (i = 0; i < 1024; i++)
                        {
                            offsets[i] = stream.ReadInt();
                        }
                        for (i = 0; i < 1024; i++)
                        {
                            offset = offsets[i];
                            if (offset > 0)
                            {
                                stream.Seek((offset >> 8) * 4096, SeekOrigin.Begin);
                                size = stream.ReadInt();
                                stream.ReadByte();
                                buffers[i] = new byte[size];
                                stream.Read(buffers[i], 0, size);
                            }
                        }
                    }
                }
                return;
            }
            catch(Exception ex)
            {
                world.Log.Error("Server.Error.RegionFile.ReadCreate({1}). {0}", ex.Message, fileName);
            }
        }

        /// <summary>
        /// Обновить оффсеты
        /// </summary>
        //private void OffsetRefresh()
        //{
        //    int offset = 4096;
        //    int count;
        //    for (int i = 0; i < 1024; i++)
        //    {
        //        if (buffers[i] == null) buffers[i] = new byte[0];
        //        count = buffers[i].Length;
        //        if (count > 0)
        //        {
        //            count = ((count + 5) >> 12) + 1;
        //            offsets[i] = offset;
        //            offset += (count << 12);// 4096;
        //            // count + 5;
        //        }
        //    }
        //}

        /// <summary>
        /// Сохранить
        /// </summary>
        public void WriteToFile()
        {
            string pathFile = path + fileName;
            try
            {
                byte[] emp = new byte[4096];
                int i, count, box;
                int offset = 4096;
                using (NBTStream stream = new NBTStream())
                {
                    for (i = 0; i < 1024; i++)
                    {
                        if (buffers[i] == null) buffers[i] = new byte[0];
                        count = buffers[i].Length;
                        if (count > 0)
                        {
                            box = ((count + 5) >> 12) + 1;
                            stream.WriteInt((offset / 4096) << 8 | box);
                            offset += (box << 12);
                        }
                        else
                        {
                            stream.WriteInt(0);
                        }
                        //stream.WriteInt(offsets[i]);
                    }
                    for (i = 0; i < 1024; i++)
                    {
                        count = buffers[i].Length;
                        if (count > 0)
                        {
                            box = ((count + 5) >> 12) + 1;
                            stream.WriteInt(count);
                            stream.WriteByte(1); // 1 = gzip; 2 = zlib
                            stream.Write(buffers[i], 0, count);
                            stream.Write(emp, 0, (box << 12) - buffers[i].Length - 5);
                        }
                    }
                    stream.Seek(0, SeekOrigin.Begin);
                    using (FileStream fileStream = new FileStream(pathFile, FileMode.Create))
                            stream.CopyTo(fileStream);
                }
            }
            catch (Exception ex)
            {
                world.Log.Error("Server.Error.RegionFile.WriteToFile({1}). {0}", ex.Message, fileName);
            }
        }

        /// <summary>
        /// Записать чанк в регион
        /// </summary>
        public void WriteChunk(TagCompound nbt, int chX, int chZ)
        {
            int x = chX & 31;
            int z = chZ & 31;
            buffers[(z * 32 + x)] = NBTTools.WriteToBytes(nbt, true);
        }
        /// <summary>
        /// Прочесть чанк с региона
        /// </summary>
        public TagCompound ReadChunk(int chX, int chZ)
        {
            int x = chX & 31;
            int z = chZ & 31;
            return NBTTools.ReadToBytes(buffers[(z * 32 + x)], true);
        }
    }
}
