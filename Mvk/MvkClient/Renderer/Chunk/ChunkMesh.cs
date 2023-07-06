using MvkClient.Util;
using SharpGL;
using System;

namespace MvkClient.Renderer.Chunk
{
    /// <summary>
    /// Объект сетки чанка
    /// </summary>
    public class ChunkMesh : IDisposable
    {
        /// <summary>
        /// Пометка изменения
        /// </summary>
        public bool IsModifiedRender { get; private set; } = false;
        /// <summary>
        /// Статус обработки сетки
        /// </summary>
        public StatusMesh Status { get; private set; } = StatusMesh.Null;

        private OpenGL gl;
        private readonly uint[] vao = new uint[1];
        private readonly uint[] vbo = new uint[1];

        /// <summary>
        /// Количество вершин
        /// </summary>
        private int countVertices = 0;
        /// <summary>
        /// Количество полигонов
        /// </summary>
        private int countPoligon = 0;
        /// <summary>
        /// Создавался ли объект BindBufferNew
        /// </summary>
        private bool empty = true;
        /// <summary>
        /// Объект буфера
        /// </summary>
        private BufferData bufferData;

        public ChunkMesh()
        {
            gl = GLWindow.gl;
            bufferData = new BufferData();
        }

        /// <summary>
        /// Изменить статус на рендеринг
        /// </summary>
        public void StatusRendering()
        {
            IsModifiedRender = false;
            Status = StatusMesh.Rendering;
        }
        /// <summary>
        /// Изменить статус отменить рендеринг
        /// </summary>
        public void NotRendering() => IsModifiedRender = false;
        /// <summary>
        /// Пометка псевдо чанка для рендера
        /// </summary>
        public void SetModifiedRender() => IsModifiedRender = true;
        
        /// <summary>
        /// Буфер внесён
        /// </summary>
        public void SetBuffer(byte[] buffer)
        {
            countPoligon = buffer.Length / 84;
            countVertices = buffer.Length / 28;
            bufferData.ConvertByte(buffer);
            Status = StatusMesh.Binding;
        }

        /// <summary>
        /// Занести буфер в OpenGL 
        /// </summary>
        public void BindBuffer()
        {
            if (bufferData.body && countPoligon > 0)
            {
                if (!empty) BindBufferReload();
                else BindBufferNew();
                bufferData.Free();
                Status = StatusMesh.Wait;
            }
            else
            {
                Delete();
            }
        }

        private void BindBufferNew()
        {
            gl.GenVertexArrays(1, vao);
            gl.BindVertexArray(vao[0]);
            gl.GenBuffers(1, vbo);
            gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, vbo[0]);
            gl.BufferData(OpenGL.GL_ARRAY_BUFFER, bufferData.size, bufferData.data, OpenGL.GL_DYNAMIC_DRAW);
            gl.VertexAttribPointer(0, 3, OpenGL.GL_FLOAT, false, 28, new IntPtr(0));
            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(1, 2, OpenGL.GL_FLOAT, false, 28, new IntPtr(12));
            gl.EnableVertexAttribArray(1);
            gl.VertexAttribIPointer(2, 1, OpenGL.GL_INT, 28, new IntPtr(20));
            gl.EnableVertexAttribArray(2);
            gl.VertexAttribIPointer(3, 1, OpenGL.GL_INT, 28, new IntPtr(24));
            gl.EnableVertexAttribArray(3);
            gl.BindVertexArray(0);
            empty = false;
        }

        /// <summary>
        /// Перезаписать полигоны, не создавая и не меняя длинну одной точки
        /// </summary>
        private void BindBufferReload()
        {
            gl.BindVertexArray(vao[0]);
            gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, vbo[0]);
            // https://habr.com/ru/articles/311808/
            // GL_STATIC_DRAW: данные либо никогда не будут изменяться, либо будут изменяться очень редко;
            // GL_DYNAMIC_DRAW: данные будут меняться довольно часто;
            // GL_STREAM_DRAW: данные будут меняться при каждой отрисовке.
            gl.BufferData(OpenGL.GL_ARRAY_BUFFER, bufferData.size, bufferData.data, OpenGL.GL_DYNAMIC_DRAW);
        }

        /// <summary>
        /// Прорисовать
        /// </summary>
        public void Draw()
        {
            if (!empty && countPoligon > 0)
            {
                Debug.CountPoligon += countPoligon;
                Debug.CountMesh++;
                gl.BindVertexArray(vao[0]);
                gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, countVertices);
                gl.BindVertexArray(0);
            }
        }

        /// <summary>
        /// Удаление если объект удалиться сам
        /// </summary>
        public void Dispose() => Delete();

        /// <summary>
        /// Удалить
        /// </summary>
        public void Delete()
        {
            countPoligon = 0;
            if (!empty)
            {
                gl.DeleteVertexArrays(1, vao);
                gl.DeleteBuffers(1, vbo);
                empty = true;
            }
            Status = StatusMesh.Null;
            bufferData.Free();
        }

        /// <summary>
        /// Статус обработки сетки
        /// </summary>
        public enum StatusMesh
        {
            /// <summary>
            /// Пустой
            /// </summary>
            Null,
            /// <summary>
            /// Ждём
            /// </summary>
            Wait,
            /// <summary>
            /// Процесс рендеринга
            /// </summary>
            Rendering,
            /// <summary>
            /// Процесс связывания сетки с OpenGL
            /// </summary>
            Binding
        }
    }
}
