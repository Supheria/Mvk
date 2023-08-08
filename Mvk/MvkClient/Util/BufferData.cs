﻿using System;
using System.Runtime.InteropServices;

namespace MvkClient.Util
{
    /// <summary>
    /// Объект конвертирует массив в неуправляемую память
    /// </summary>
    public struct BufferData
    {
        /// <summary>
        /// Размер
        /// </summary>
        public int size;
        /// <summary>
        /// Указатель данных
        /// </summary>
        public IntPtr data;
        /// <summary>
        /// имеется ли тело
        /// </summary>
        public bool body;

        /// <summary>
        /// Занести в память массив из float
        /// </summary>
        public void ConvertByte(byte[] data)
        {
            if (data.Length == 0)
            {
                Free();
            }
            else
            {
                //TODO::2023-07-30 заменить и потестить с замером времени Marshal.UnsafeAddrOfPinnedArrayElement()
                size = data.Length * sizeof(byte);
                this.data = Marshal.AllocHGlobal(size);
                Marshal.Copy(data, 0, this.data, data.Length);
                body = true;
            }
        }

        /// <summary>
        /// Освободить память
        /// </summary>
        public void Free()
        {
            if (body)
            {
                Marshal.FreeHGlobal(data);
                body = false;
            }
        }
    }
}
