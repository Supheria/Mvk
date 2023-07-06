using System;

namespace MvkServer.Util
{
    /// <summary>
    /// Усовершенствованный лист от Мефистофель, работы без мусора, чтоб не пересоздавать
    /// Выделяем объём кеша, но увеличивать не может!!!
    /// </summary>
    public class ArrayMvk<T>
    {
        public int count;
        public T[] buffer;

        /// <summary>
        /// Создаём, с выделенным объёмом
        /// </summary>
        public ArrayMvk(int maxSize = 1000) => buffer = new T[maxSize];

        public T this[int index] => buffer[index];

        public void Add(T item)
        {
            try
            {
                buffer[count++] = item;
            }
            catch
            {
                Logger.Crach("ArrayMvk.Add count {0}", count);
            }
        }

        public void AddNull(int count) => this.count += count;

        public void AddRange(T[] items)
        {
            try
            {
                int count = items.Length;
                for (int i = 0; i < count; i++)
                {
                    buffer[this.count + i] = items[i];
                }
                this.count += count;
            }
            catch
            {
                Logger.Crach("ArrayMvk.AddRange count {0}", count);
            }
        }

        public void AddFloat(T[] items)
        {
            for (int i = 0; i < 4; i++)
            {
                buffer[count + i] = items[i];
            }
            count += 4;
        }

        public void AddRange(ArrayMvk<T> items)
        {
            int count = items.count;
            for (int i = 0; i < count; i++)
            {
                buffer[this.count + i] = items[i];
            }
            this.count += count;
        }

        public T[] ToArray()
        {
            T[] result = new T[count];
            Array.Copy(buffer, result, count);
            return result;
        }

        public void Clear() => count = 0;


        public void Combine(T[] items)
        {
            int count = items.Length;
            if (count > 0)
            {
                Buffer.BlockCopy(items, 0, buffer, this.count, count);
                this.count += count;
            }
        }

        //public void Sort() => Array.Sort(buffer);
    }
}
