using System;
using System.IO;
using System.IO.Compression;

namespace MvkServer.NBT
{
    /// <summary>
    /// Класс отвечающий за сохранение и чтение NBT
    /// </summary>
    public class NBTTools
    {
        /// <summary>
        /// Записать NBT в файл по адресу path
        /// </summary>
        public static void WriteToFile(TagCompound compound, string path, bool compress)
        {
            try
            {
                using (NBTStream nbtStream = new NBTStream())
                {
                    nbtStream.WriteByte(compound.GetId());
                    if (compound.GetId() != 0)
                    {
                        nbtStream.WriteUTF("");
                        compound.Write(nbtStream);
                    }
                    nbtStream.Seek(0, SeekOrigin.Begin);
                    if (compress)
                    {
                        using (GZipStream zipStream = new GZipStream(new FileStream(path, FileMode.Create),
                            CompressionMode.Compress)) nbtStream.CopyTo(zipStream);
                    }
                    else
                    {
                        using (FileStream fileStream = new FileStream(path, FileMode.Create))
                            nbtStream.CopyTo(fileStream);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Прочесть NBT в файл по адресу path
        /// </summary>
        public static TagCompound ReadFromFile(string path, bool compress)
        {
            try
            { 
                TagCompound compound = new TagCompound();
                if (File.Exists(path))
                {
                    using (NBTStream nbtStream = new NBTStream())
                    {
                        if (compress)
                        {
                            using (GZipStream zipStream = new GZipStream(new FileStream(path, FileMode.Open),
                                CompressionMode.Decompress)) Read(zipStream, nbtStream, compound);
                        }
                        else
                        {
                            using (FileStream fileStream = new FileStream(path, FileMode.Open))
                                Read(fileStream, nbtStream, compound);
                        }
                    }
                }
                return compound;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void Read(Stream stream, NBTStream nbtStream, TagCompound compound)
        {
            stream.CopyTo(nbtStream);
            nbtStream.Seek(0, SeekOrigin.Begin);
            byte id = nbtStream.Byte();
            if (id == 10)
            {
                nbtStream.ReadUTF();
                compound.Read(nbtStream);
            }
        }

        /// <summary>
        /// Записать NBT в массив байт
        /// </summary>
        public static byte[] WriteToBytes(TagCompound compound, bool compress)
        {
            try
            {
                using (NBTStream nbtStream = new NBTStream())
                {
                    nbtStream.WriteByte(compound.GetId());
                    if (compound.GetId() != 0)
                    {
                        nbtStream.WriteUTF("");
                        compound.Write(nbtStream);
                    }
                    nbtStream.Seek(0, SeekOrigin.Begin);
                    if (compress)
                    {
                        using (MemoryStream outStream = new MemoryStream())
                        {
                            using (GZipStream zipStream = new GZipStream(outStream, CompressionMode.Compress))
                                nbtStream.CopyTo(zipStream);
                            return outStream.ToArray();
                        }
                    }
                    else
                    {
                        return nbtStream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Записать NBT в массив байт
        /// </summary>
        public static TagCompound ReadToBytes(byte[] buffer, bool compress)
        {
            try
            {
                if (buffer != null && buffer.Length > 0)
                {
                    TagCompound compound = new TagCompound();
                    using (NBTStream nbtStream = new NBTStream())
                    {
                        if (compress)
                        {
                            using (GZipStream zipStream = new GZipStream(new MemoryStream(buffer), CompressionMode.Decompress))
                                Read(zipStream, nbtStream, compound);
                        }
                        else
                        {
                            using (MemoryStream outStream = new MemoryStream(buffer))
                                Read(outStream, nbtStream, compound);
                        }
                    }
                    return compound;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
