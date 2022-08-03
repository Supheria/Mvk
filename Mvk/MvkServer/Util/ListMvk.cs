using System;
using System.Runtime.CompilerServices;

namespace MvkServer.Util
{
    /// <summary>
    /// Усовершенствованный лист от Мефистофель, работы без мусора, чтоб не пересоздавать
    /// </summary>
    public class ListMvk<T>
    {
        private T[] buffer;
        private int size;
        private int count;

        public ListMvk(int size = 100)
        {
            this.size = size;
            buffer = new T[size];
        }

        public int Count => count;

        public T this[int index] => buffer[index];

        public void Add(T item)
        {
            if (size < count + 1)
            {
                size = (int)(size * 1.5f);
                Array.Resize(ref buffer, size);
            }
            buffer[count++] = item;
        }

        public void AddRange(T[] items)
        {
            int count = items.Length;
            if (size < this.count + count)
            {
                size = (int)(size + count + (size * 0.3f));
                Array.Resize(ref buffer, size);
            }
            for (int i = 0; i < count; i++)
                buffer[this.count + i] = items[i];

            this.count += count;
        }
        
        public T[] ToArray()
        {
            T[] result = new T[count];
            Array.Copy(buffer, result, count);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => count = 0;

        //public bool Contains(T item)
        //{
        //    for (int i = 0; i < count; i++) 
        //        if (buffer[i].Equals(item)) return true;
        //    return false;
        //}
    }
}
