﻿using MvkAssets;
using MvkClient.Actions;
using MvkClient.Renderer;
using MvkClient.Renderer.Font;
using MvkClient.Setitings;
using MvkServer.Glm;
using SharpGL;
using System;
using System.Collections.Generic;

namespace MvkClient.Gui
{
    public class Screen : IDisposable
    {
        /// <summary>
        /// Колекция всех контролов
        /// </summary>
        public List<Control> Controls { get; protected set; } = new List<Control>();
        /// <summary>
        /// Ширина окна
        /// </summary>
        public int Width { get; private set; }
        /// <summary>
        /// Высота окна
        /// </summary>
        public int Height { get; private set; }
        /// <summary>
        /// Координата мыши
        /// </summary>
        public vec2i MouseCoord { get; private set; }
        /// <summary>
        /// Основной клиент
        /// </summary>
        public Client ClientMain { get; protected set; }
        /// <summary>
        /// Является ли окно контейнером во время игры
        /// </summary>
        public bool IsConteiner { get; protected set; } = false;
        /// <summary>
        /// Откуда зашёл
        /// </summary>
        protected EnumScreenKey where;
        /// <summary>
        /// Объект OpenGL
        /// </summary>
        protected static OpenGL gl;
        /// <summary>
        /// Размер интерфеса
        /// </summary>
        protected int sizeInterface;
        /// <summary>
        /// Тип фона
        /// </summary>
        protected EnumBackground background = EnumBackground.Menu;
        /// <summary>
        /// Графический лист
        /// </summary>
        protected uint dList;

        protected Screen() => sizeInterface = Setting.SizeInterface;
        public Screen(Client client) : this() => ClientMain = client;

        public void Initialize()
        {
            gl = GLWindow.gl;
            Width = GLWindow.WindowWidth;
            Height = GLWindow.WindowHeight;
            Init();
            Resized();
        }

        protected virtual void Init() { }

        /// <summary>
        /// Инициализация нажатие кнопки
        /// </summary>
        protected void InitButtonClick(Button button)
        {
            if (button.ScreenKey != EnumScreenKey.None)
            {
                button.Click += (sender, e) => OnFinished(button.ScreenKey);
            }
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        public void Resized()
        {
            Width = GLWindow.WindowWidth;
            Height = GLWindow.WindowHeight;
            sizeInterface = Setting.SizeInterface;
            foreach (Control control in Controls)
            {
                control.Resized();
            }
            ResizedScreen();
            RenderList();
        }
        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected virtual void ResizedScreen() { }

        /// <summary>
        /// Прорисовка
        /// </summary>
        public void Draw()
        {
            foreach (Control control in Controls)
            {
                if (control.GetType() == typeof(TextBox))
                {
                    TextBox textBox = control as TextBox;
                    if (textBox.Focus)
                    {
                        textBox.UpdateCursorCounter();
                        if (textBox.IsRender) RenderList();
                    }
                }
            }
            GLRender.ListCall(dList);
            DrawAdd();
        }

        protected virtual void DrawAdd() { }

        /// <summary>
        /// Рендер листа
        /// </summary>
        protected void RenderList()
        {
            uint list = GLRender.ListBegin();

            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            //gl.Ortho2D(0, Width, Height, 0);
            gl.Ortho(0, Width, Height, 0, -100, 100);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();

            Render();

            gl.EndList();
            GLRender.ListDelete(dList);
            dList = list;
        }

        /// <summary>
        /// Контролы
        /// </summary>
        protected virtual void RenderControls()
        {
            foreach(Control control in Controls)
            {
                GLRender.PushMatrix();
                GLRender.Translate(control.Position.x, control.Position.y, 0);
                GLRender.Scale(sizeInterface, sizeInterface, 1);
                control.Draw();
                GLRender.PopMatrix();
            }
        }
        /// <summary>
        /// Фон
        /// </summary>
        protected void RenderBackground()
        {
            if (background == EnumBackground.Menu)
            {
                gl.Enable(OpenGL.GL_TEXTURE_2D);
                GLWindow.Texture.BindTexture(AssetsTexture.OptionsBackground);
                gl.Color(.4f, .4f, .4f, 1f);
                GLRender.Rectangle(0, 0, Width, Height, 0, 0, Width / 32f, Height / 32f);
            }
            else if (background == EnumBackground.TitleMain)
            {
                gl.Enable(OpenGL.GL_TEXTURE_2D);
                GLWindow.Texture.BindTexture(AssetsTexture.Title);
                gl.Color(1.0f, 1.0f, 1.0f, 1f);
                float k = Height * 2f / (float)Width;
                if (k > 2f)
                {
                    GLRender.Rectangle(0, 0, Width, Height, 0.5f, 0, 1.0f, 0.5f);
                }
                else if (k > 1f)
                {
                    k = (k - 1.0f) / 2f;
                    GLRender.Rectangle(0, 0, Width, Height, k, 0, 1.0f, 0.5f);
                }
                else
                {
                    GLRender.Rectangle(Width - Height, 0, Width, Height, 0.5f, 0, 1.0f, 0.5f);
                    GLRender.Rectangle(0, 0, Width - Height, Height, 0f, 0, 0.5f, 0.5f);
                }
                int w = Width < 1500 ? Width / 2 : 1024;
                int h = Width < 1500 ? Width / 8 : 256;
                GLRender.Rectangle(0, 0, w, h, 0f, 0.5f, 1f, 0.75f);
                GLWindow.Texture.BindTexture(AssetsTexture.Font8);
                string text = ClientMain.NameVersion;
                vec4 colorB = new vec4(0.2f, 0.2f, 0.2f, 1f);
                vec4 color = new vec4(1.0f, 1.0f, 1.0f, 1f);
                GLRender.PushMatrix();
                GLRender.Scale(sizeInterface, sizeInterface, 1);
                FontRenderer.RenderString(11, Height / sizeInterface - 19, colorB, text, FontSize.Font8);
                FontRenderer.RenderString(10, Height / sizeInterface - 20, color, text, FontSize.Font8);
                text = "SuperAnt";
                int ws = FontRenderer.WidthString(text, FontSize.Font8) + 10;
                FontRenderer.RenderString(Width / sizeInterface - ws + 1, Height / sizeInterface - 19, colorB, text, FontSize.Font8);
                FontRenderer.RenderString(Width / sizeInterface - ws, Height / sizeInterface - 20, color, text, FontSize.Font8);
                GLRender.PopMatrix();
            }
            else if (background == EnumBackground.Game)
            {
                return;
            }
            else
            {
                gl.Disable(OpenGL.GL_TEXTURE_2D);
                vec4 colBg;
                if (background == EnumBackground.GameOver) colBg = new vec4(.4f, .1f, .1f, .5f);
                else if (background == EnumBackground.Loading) colBg = new vec4(1f, 1f, 1f, 1f);
                else colBg = new vec4(.3f, .3f, .3f, .5f);
                GLRender.Rectangle(0, 0, Width, Height, colBg);
            }
        }

        /// <summary>
        /// Прорисовка
        /// </summary>
        protected virtual void Render()
        {
            // Фон
            RenderBackground();
            // Контролы
            RenderControls();
        }

        /// <summary>
        /// Перемещение мышки
        /// </summary>
        public virtual void MouseMove(int x, int y)
        {
            MouseCoord = new vec2i(x, y);
            bool isRender = false;
            foreach (Control control in Controls)
            {
                if (control.Visible && control.Enabled) control.MouseMove(x, y);
                if (control.IsRender) isRender = true;
            }

            if (isRender) RenderList();
        }

        /// <summary>
        /// Нажатие клавиши мышки
        /// </summary>
        public void MouseDown(MouseButton button, int x, int y) => MouseUpDown(button, x, y, true);
        /// <summary>
        /// Отпущена клавиша мышки
        /// </summary>
        public void MouseUp(MouseButton button, int x, int y) => MouseUpDown(button, x, y, false);

        protected void MouseUpDown(MouseButton button, int x, int y, bool isDown)
        {
            foreach (Control control in Controls)
            {
                if (control.Visible && control.Enabled)
                {
                    if (isDown) control.MouseDown(button, x, y);
                    else control.MouseUp(button, x, y);
                }
            }
        }

        /// <summary>
        /// Нажата клавиша в char формате
        /// </summary>
        public void KeyPress(char key)
        {
            foreach (Control control in Controls)
            {
                if (control.Visible && control.Enabled && control.Focus)
                {
                    control.KeyPress(key);
                    break;
                }
            }
        }

        public void Dispose() => Delete();
        public void Delete() => GLRender.ListDelete(dList);

        public void AddControls(Control control)
        {
            control.Init(this);
            Controls.Add(control);
        }

        /// <summary>
        /// Закончен скрин
        /// </summary>
        public event ScreenEventHandler Finished;
        protected virtual void OnFinished(EnumScreenKey key) => OnFinished(new ScreenEventArgs(key));
        protected virtual void OnFinished(ScreenEventArgs e) => Finished?.Invoke(this, e);

        /// <summary>
        /// Прорисовать прямоугольник с текстурой, где расчёт текстуры через пиксели, где текстура 512*512
        /// </summary>
        protected void DrawTexturedModalRect(int x, int y, int textureX, int textureY, int width, int height)
            => GLRender.Rectangle(x, y, x + width, y + height, textureX * .001953125f, textureY * .001953125f,
                (textureX + width) * .001953125f, (textureY + height) * .001953125f);
    }
}
