using MvkAssets;
using MvkClient.Entity;
using MvkClient.Renderer;
using MvkClient.Renderer.Font;
using MvkServer.Glm;
using MvkServer.Item;
using MvkServer.Item.List;
using MvkServer.Util;

namespace MvkClient.Gui
{
    /// <summary>
    /// Скрин во время игры
    /// Тут всё игрока, инвентарь на экране, ХП и другие опции, эффект воды и прочего
    /// </summary>
    public class ScreenInGame : ScreenBase
    {
        /// <summary>
        /// Цвет воды для эффектов
        /// </summary>
        public static vec3 colorWaterEff = new vec3(0f, .1f, .4f);
        /// <summary>
        /// Цвет лавы для эффектов
        /// </summary>
        public static vec3 colorLavaEff = new vec3(.8f, .27f, .04f);
        /// <summary>
        /// Цвет нефти для эффектов
        /// </summary>
        public static vec3 colorOilEff = new vec3(0f, 0f, .1f);

        /// <summary>
        /// Экземпляр ChatGUI, который сохраняет все предыдущие данные чата
        /// </summary>
        public GuiChatList PersistantChatGUI { get; private set; }

        public ScreenInGame(Client client) : base(client)
        {
            PersistantChatGUI = new GuiChatList(this);
            Initialize();
        }

        /// <summary>
        /// Такт игрового времени
        /// </summary>
        public override void Tick() => PersistantChatGUI.Tick();

        /// <summary>
        /// Прорисовка
        /// </summary>
        /// <param name="timeIndex">Получить коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1, где 0 это начало, 1 финиш</param>
        public void Draw(float timeIndex)
        {
            //vec3 pos = ClientMain.Player.GetPositionFrame(timeIndex);
            //pos.y += ClientMain.Player.GetEyeHeight();
            SizeInterface = Setitings.Setting.SizeInterface;

            // Матрица проекции камеры для 2д, GUI
            Ortho2D();

            bool visible = !ClientMain.Player.IsInvisible();

            // Прицел
            if (visible && ClientMain.Player.ViewCamera == EnumViewCamera.Eye)
            {
                GLRender.PushMatrix();
                GLRender.Translate(Width / 2, Height / 2, 0);
                GLRender.Scale(SizeInterface, SizeInterface, 1);
                GLRender.Texture2DEnable();
                GLWindow.Texture.BindTexture(AssetsTexture.Icons);
                GLRender.Color(1f);
                GLRender.Rectangle(-16, -16, 16, 16, .9375f, .9375f, 1f, 1f);
                GLRender.PopMatrix();
            }
            // Эффект урона
            DrawEffDamage(ClientMain.Player.DamageTime, timeIndex);
            
            // Эффект воды если надо
            DrawEffEyes(timeIndex);
            if (visible)
            {
                if (ClientMain.Player.InFire() && ClientMain.Player.ViewCamera == EnumViewCamera.Eye)
                {
                    // Огонь
                    DrawFire();
                }

                // Центровка
                GLRender.PushMatrix();
                GLRender.Translate(Width / 2, Height, 0);
                GLRender.Scale(SizeInterface, SizeInterface, 1);
                GLRender.Texture2DEnable();

                // Инвенатрь
                DrawInventory();
                // Статусы
                DrawStat();

                GLRender.PopMatrix();
            }

            // Чат
            if (ClientMain.Screen.IsEmptyScreen() || ClientMain.Screen.GetTypeScreen() != typeof(ScreenChat))
            {
                PersistantChatGUI.DrawInGame();
            }
        }

        #region Draws

        /// <summary>
        /// Прорисовка статусов
        /// </summary>
        private void DrawStat()
        {
            int armor = 9; // параметр брони 0 - 16
            int health = Mth.Ceiling(ClientMain.Player.GetHealth()); // хп 0 - 16
            int endurance = 16; // выносливось 0 - 16
            int food = 16; // параметр голода 0 - 16
            int air = ClientMain.Player.GetAir();
            int air0 = 0;
            int uH = 0;
            int uA = 0;

            // Level
            if (ClientMain.Player.XpBarCap() > 0)
            {
                GLRender.Color(1f);
                GLWindow.Texture.BindTexture(AssetsTexture.Icons);
                int levelBar = (int)(ClientMain.Player.Experience * 398f);
                DrawTexturedModalRect(-199, -67, 0, 496, 398, 8); // Фон
                if (levelBar > 0) DrawTexturedModalRect(-199, -67, 0, 504, levelBar, 8); // Значение
            }

            // Level Text
            if (ClientMain.Player.ExperienceLevel > 0)
            {
                string str = ClientMain.Player.ExperienceLevel.ToString();
                FontSize fontSize = FontSize.Font12;
                GLWindow.Texture.BindTexture(Assets.ConvertFontToTexture(fontSize));
                int x = FontRenderer.WidthString(str, fontSize) / -2;
                int y = -73;
                vec3 colorbg = new vec3(0);
                FontRenderer.RenderString(x + 1, y, str, fontSize, colorbg);
                FontRenderer.RenderString(x - 1, y, str, fontSize, colorbg);
                FontRenderer.RenderString(x, y + 1, str, fontSize, colorbg);
                FontRenderer.RenderString(x, y - 1, str, fontSize, colorbg);
                FontRenderer.RenderString(x, y, str, fontSize, new vec3(.52f, .9f, .2f));//.5f, 1f, .125f
            }

            GLRender.Color(1f);
            GLWindow.Texture.BindTexture(AssetsTexture.Icons);

            // Armor static
            if (armor > 0)
            {
                DrawTexturedModalRect(-256, -78, 0, 19, 19, 19);
                DrawTexturedModalRect(-256, -78, 76, 19, 19, 19);
            }

            // Health static
            if (ClientMain.Player.DamageTime > 0)
            {
                if (ClientMain.Player.DamageTime == 1 || ClientMain.Player.DamageTime == 5) uH = 19;
                else if (ClientMain.Player.DamageTime == 2 || ClientMain.Player.DamageTime == 4) uH = 38;
                else if (ClientMain.Player.DamageTime == 3) uH = 57;
            }
            DrawTexturedModalRect(-228, -78, uH, 0, 19, 19);
            DrawTexturedModalRect(-228, -78, 76, 0, 19, 19);

            // Endurance static
            DrawTexturedModalRect(209, -78, 0, 76, 19, 19);
            DrawTexturedModalRect(209, -78, 76, 76, 19, 19);

            // Food static
            DrawTexturedModalRect(237, -78, 0, 57, 19, 19);
            DrawTexturedModalRect(237, -78, 76, 57, 19, 19);

            // Air static
            if (air < 300)
            {
                air0 = Mth.Ceiling((air - 2f) * 16f / 300f);
                int air1 = Mth.Ceiling(air * 16f / 300f) - air0;

                if (air1 == 0)
                {
                    uA = 0;
                    DrawTexturedModalRect(265, -78, 0, 38, 19, 19);
                }
                else
                {
                    uA = 19;
                    DrawTexturedModalRect(265, -78, 19, 38, 19, 19);
                }
            }

            // Динамик
            for (int i = 0; i < 8; i++)
            {
                int y = -16 - i * 6;
                int i16 = i * 2 + 1;

                // Armor dinamic
                if (armor > 0)
                {
                    DrawTexturedModalRect(-256, y, 0, 473, 19, 7); // Фон
                    if (i16 < armor) DrawTexturedModalRect(-256, y, 95, 473, 19, 7); // Целая и половина
                    else if (i16 == armor) DrawTexturedModalRect(-256, y, 95, 473, 9, 7); // Целая и половина
                }

                // Health dinamic
                DrawTexturedModalRect(-228, y, uH, 473, 19, 7); // Фон
                if (i16 < health) DrawTexturedModalRect(-228, y, 76, 473, 19, 7); // Целая и половина
                else if (i16 == health) DrawTexturedModalRect(-228, y, 76, 473, 9, 7); // Целая и половина

                // Endurance dinamic
                DrawTexturedModalRect(209, y, 0, 473, 19, 7); // Фон
                if (i16 < endurance) DrawTexturedModalRect(209, y, 152, 473, 19, 7); // Целая и половина
                else if (i16 == endurance) DrawTexturedModalRect(209, y, 152, 473, 9, 7); // Целая и половина

                // Food dinamic
                DrawTexturedModalRect(237, y, 0, 473, 19, 7); // Фон
                if (i16 < food) DrawTexturedModalRect(237, y, 133, 473, 19, 7); // Целая и половина
                if (i16 == food) DrawTexturedModalRect(237, y, 133, 473, 9, 7); // Целая и половина

                // Air dinamic
                if (air < 300)
                {
                    DrawTexturedModalRect(265, y, uA, 473, 19, 7); // Фон
                    if (i16 < air0) DrawTexturedModalRect(265, y, 114, 473, 19, 7); // Целая и половина
                    else if (i16 == air0) DrawTexturedModalRect(265, y, 114, 473, 9, 7); // Целая и половина
                }
            }
        }

        /// <summary>
        /// Прорисовка инвентаря
        /// </summary>
        private void DrawInventory()
        {
            int scale = 26; // 38
            int size = 50; // 66
            int w = size * -4;
            int h = -size - 8;
            vec4 colorText = new vec4(1);
            FontSize fontSize = FontSize.Font12;

            for (int i = 0; i < 8; i++)
            {
                GLWindow.Texture.BindTexture(AssetsTexture.Widgets);
                GLRender.Color(1f);

                int w0 = w + i * size;

                // Фон
                GLRender.Rectangle(w0, h, w0 + size, h + size, 0, .46875f, 0.1953125f, .6640625f);
                
                if (ClientMain.Player.Inventory.CurrentItem == i)
                {
                    // Выбранный
                    //GLRender.Rectangle(w0, h, w0 + size, h + size, 0.1953125f, .46875f, 0.390625f, .6640625f);
                    GLRender.Rectangle(w0 - 2, h - 2, w0 + 52, h + 52, 0.1953125f, .46875f, 0.40625f, .6796875f);
                }

                // Прорисовка предмета в стаке если есть
                ItemStack itemStack = ClientMain.Player.Inventory.GetStackInSlot(i);
                if (itemStack != null)
                {
                    int w1 = w0 + size / 2;
                    int h1 = h + size / 2;
                    if (itemStack.Item.EItem == EnumItem.Block && itemStack.Item is ItemBlock itemBlock)
                    {
                        // Прорисовка блока
                        ClientMain.World.WorldRender.GetBlockGui(itemBlock.Block.EBlock).Render(w1, h1, scale);
                    }
                    else
                    {
                        // Прорисовка предмета
                        ClientMain.World.WorldRender.GetItemGui(itemStack.Item.EItem).Render(w1, h1, scale);
                    }
                    if (itemStack.Amount > 1)
                    {
                        GLWindow.Texture.BindTexture(Assets.ConvertFontToTexture(fontSize));
                        string str = itemStack.Amount.ToString();
                        int ws = FontRenderer.WidthString(str, fontSize);
                        FontRenderer.RenderString(w1 + 20 - ws, h1 + 9, str, fontSize, new vec3(1), 1, true, 0, 1);
                    }
                    if (itemStack.ItemDamage > 0)
                    {
                       // fontSize = FontSize.Font8;
                        GLWindow.Texture.BindTexture(Assets.ConvertFontToTexture(fontSize));
                        string str = itemStack.ItemDamage.ToString();
                        //int ws = FontRenderer.WidthString(str, fontSize);
                        FontRenderer.RenderString(w0 + 6, h1 + 9, str, fontSize, new vec3(1), 1, true, 0, 1);
                    }
                }
            }
        }

        /// <summary>
        /// Эффект глазз, вводе, лаве и тп
        /// </summary>
        private void DrawEffEyes(float timeIndex)
        {
            bool enable;
            vec4 color;

            if (ClientMain.Player.WhereEyesEff == EntityPlayerSP.WhereEyes.Air)
            {
                enable = false;
                color = new vec4();
            }
            else if (ClientMain.Player.WhereEyesEff == EntityPlayerSP.WhereEyes.Water)
            {
                enable = true;
                color = new vec4(colorWaterEff, .3f);
            }
            else if (ClientMain.Player.WhereEyesEff == EntityPlayerSP.WhereEyes.Lava)
            {
                enable = true;
                color = new vec4(colorLavaEff, .85f);
            }
            else
            {
                enable = true;
                color = new vec4(colorOilEff, .9f);
            }

            if (enable)
            {
                GLRender.Texture2DDisable();
                GLRender.Rectangle(0, 0, Width, Height, color);
            }
        }

        /// <summary>
        /// Эффект урона
        /// </summary>
        private void DrawEffDamage(float damageTime, float timeIndex)
        {
            if (ClientMain.Player.IsHurtAnimation && ClientMain.Player.DamageTime > 0 && ClientMain.Player.ViewCamera == EnumViewCamera.Eye)
            {
                float dt = Mth.Sqrt((damageTime + timeIndex - 1f) / 5f * 1.6f);
                if (dt > 1f) dt = 1f; 
                GLRender.Texture2DDisable();
                GLRender.Rectangle(0, 0, Width, Height, new vec4(0.7f, 0.4f, 0.3f, 0.7f * dt));
            }
        }

        /// <summary>
        /// Огонь
        /// </summary>
        private void DrawFire()
        {
            uint time = ClientMain.TickCounter;
            int frame = (int)(time - (time / 32) * 32);

            float u1 = .84375f;
            float u2 = .859375f;
            float v1 = frame * .015625f;
            float v2 = v1 + .015625f;

            GLRender.PushMatrix();
            GLRender.Texture2DEnable();
            GLWindow.Texture.BindTexture(AssetsTexture.AtlasBlocks);
            GLRender.Color(1, 1, 1, .3f);
            GLRender.Rectangle(0, 0, Width, Height, u1, v1, u2, v2);
            GLRender.PopMatrix();
        }

        #endregion
    }
}
