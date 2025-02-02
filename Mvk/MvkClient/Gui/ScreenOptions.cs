﻿using MvkAssets;
using MvkClient.Setitings;
using MvkServer.Glm;
using System;

namespace MvkClient.Gui
{
    public class ScreenOptions : Screen
    {
        protected Label label;
        protected Label labelSeed;
        protected Label labelNickname;
        protected Label labelLanguage;
        protected Button buttonCancel;
        protected Button buttonDone;
        protected Button buttonLanguage;
        protected Button buttonNet;
        protected Button buttonSmoothLighting;
        protected Slider sliderFps;
        protected Slider sliderChunk;
        protected Slider sliderSoundVolume;
        protected Slider sliderMusicVolume;
        protected Slider sliderSizeInterface;
        protected TextBox textBoxSeed;
        protected TextBox textBoxNickname;

        private int cacheLanguage;
        private bool cacheSmoothLighting;

        public ScreenOptions(Client client, EnumScreenKey where) : base(client)
        {
            cacheLanguage = Setting.Language;
            cacheSmoothLighting = Setting.SmoothLighting;
            this.where = where;

            label = new Label(Language.T("gui.options"), FontSize.Font16);
            labelSeed = new Label(Language.T("gui.seed"), FontSize.Font12)
            {
                Width = 160,
                Alight = EnumAlight.Right
            };
            textBoxSeed = new TextBox(Setting.SeedBegin == 0 ? "" : Setting.SeedBegin.ToString(), TextBox.EnumRestrictions.Number, 8) { Width = 160 };
            labelNickname = new Label(Language.T("gui.nikname"), FontSize.Font12)
            {
                Width = 160,
                Alight = EnumAlight.Right
            };
            textBoxNickname = new TextBox(Setting.Nickname, TextBox.EnumRestrictions.Name) { Width = 256 };
            
            sliderFps = new Slider(10, 260, 10, Language.T("gui.fps"))
            {
                Width = 256,
                Value = Setting.Fps
            };
            sliderFps.AddParam(260, Language.T("gui.maxfps"));
            sliderChunk = new Slider(2, 32, 1, Language.T("gui.overview.chunks"))
            {
                Width = 256,
                Value = Setting.OverviewChunk
            };
            sliderSoundVolume = new Slider(0, 100, 1, Language.T("gui.volume.sound"))
            {
                Width = 256,
                Value = Setting.SoundVolume
            };
            sliderSoundVolume.AddParam(0, Language.T("gui.volume.off"));
            sliderMusicVolume = new Slider(0, 100, 1, Language.T("gui.volume.music"))
            {
                Width = 256,
                Value = Setting.MusicVolume,
                Enabled = false
            };
            sliderSizeInterface = new Slider(1, 2, 1, Language.T("gui.size.interface"))
            {
                Width = 192,
                Value = Setting.SizeInterfaceOptions
            };
            labelLanguage = new Label(Language.T("gui.language"), FontSize.Font12)
            {
                Width = 160,
                Alight = EnumAlight.Right
            };
            buttonLanguage = new Button(Language.GetName(cacheLanguage)) { Width = 160 };
            buttonLanguage.Click += ButtonLanguage_Click;
            buttonNet = new Button(Language.T("gui.net")) { Width = 160 };
            buttonNet.Click += ButtonNet_Click;
            buttonSmoothLighting = new Button(ButtonSmoothLightingName()) { Width = 320 };
            buttonSmoothLighting.Click += ButtonSmoothLighting_Click;
            buttonDone = new Button(Language.T("gui.apply")) { Width = 256 };
            buttonDone.Click += ButtonDone_Click;
            buttonCancel = new Button(where, Language.T("gui.cancel")) { Width = 256 };
            InitButtonClick(buttonCancel);
            if (where == EnumScreenKey.InGameMenu)
            {
                background = EnumBackground.GameWindow;
                textBoxNickname.Enabled = false;
            }
        }

        /// <summary>
        /// Нажата клавиша
        /// </summary>
        public override void KeyDown(int key)
        {
            if (key == 27)// ESC
            {
                ClientMain.Screen.GameMode();
            }
        }

        protected override void Init()
        {
            AddControls(label);
            AddControls(labelNickname);
            AddControls(labelLanguage);
            AddControls(textBoxNickname);
            AddControls(sliderFps);
            AddControls(sliderChunk);
            AddControls(sliderSoundVolume);
            AddControls(sliderMusicVolume);
            AddControls(sliderSizeInterface);
            AddControls(buttonSmoothLighting);
            AddControls(buttonDone);
            AddControls(buttonCancel);
            AddControls(buttonLanguage);
            if (where != EnumScreenKey.InGameMenu)
            {
                AddControls(labelSeed);
                AddControls(textBoxSeed);
            }
            if (ClientMain.IsServerLocalRun())
            {
                if (ClientMain.IsOpenNet())
                {
                    buttonNet.Enabled = false;
                    buttonNet.SetText(Language.T("gui.net.on"));
                }
                AddControls(buttonNet);
            }
        }
        
        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void ResizedScreen()
        {
            int h = 72 * SizeInterface;
            int hMax = h + 360 * SizeInterface;
            if (hMax > Height) h -= hMax - Height;

            label.Position = new vec2i(Width / 2 - 200 * SizeInterface, h - 44 * SizeInterface);
            labelSeed.Position = new vec2i(Width / 2 - 162 * SizeInterface, h);
            textBoxSeed.Position = new vec2i(Width / 2 + 2 * SizeInterface, h);
            labelNickname.Position = new vec2i(Width / 2 - 162 * SizeInterface, h + 44 * SizeInterface);
            textBoxNickname.Position = new vec2i(Width / 2 + 2 * SizeInterface, h + 44 * SizeInterface);
            sliderSoundVolume.Position = new vec2i(Width / 2 - 258 * SizeInterface, h + 88 * SizeInterface);
            sliderMusicVolume.Position = new vec2i(Width / 2 + 2 * SizeInterface, h + 88 * SizeInterface);
            sliderFps.Position = new vec2i(Width / 2 - 258 * SizeInterface, h + 132 * SizeInterface);
            sliderChunk.Position = new vec2i(Width / 2 + 2 * SizeInterface, h + 132 * SizeInterface);
            sliderSizeInterface.Position = new vec2i(Width / 2 - 258 * SizeInterface, h + 176 * SizeInterface);
            buttonSmoothLighting.Position = new vec2i(Width / 2 - 62 * SizeInterface, h + 176 * SizeInterface);
            labelLanguage.Position = new vec2i(Width / 2 - 162 * SizeInterface, h + 220 * SizeInterface);
            buttonLanguage.Position = new vec2i(Width / 2 + 2 * SizeInterface, h + 220 * SizeInterface);
            buttonNet.Position = new vec2i(Width / 2 + 2 * SizeInterface, h + 264 * SizeInterface);
            buttonDone.Position = new vec2i(Width / 2 - 258 * SizeInterface, h + 308 * SizeInterface);
            buttonCancel.Position = new vec2i(Width / 2 + 2 * SizeInterface, h + 308 * SizeInterface);
        }

        private void ButtonLanguage_Click(object sender, EventArgs e)
        {
            cacheLanguage = Language.Next(cacheLanguage);
            buttonLanguage.SetText(Language.GetName(cacheLanguage));
        }

        private string ButtonSmoothLightingName() => Language.T("gui.smooth.lighting." + (cacheSmoothLighting ? "on" : "off"));

        private void ButtonSmoothLighting_Click(object sender, EventArgs e)
        {
            cacheSmoothLighting = !cacheSmoothLighting;
            buttonSmoothLighting.SetText(ButtonSmoothLightingName());
        }

        private void ButtonNet_Click(object sender, EventArgs e)
        {
            if (where == EnumScreenKey.InGameMenu)
            {
                buttonNet.Enabled = false;
                buttonNet.SetText(Language.T("gui.net.on"));
                ClientMain.OpenNet();
            }
        }

        private void ButtonDone_Click(object sender, EventArgs e)
        {
            // Сохранение настроек
            if (where == EnumScreenKey.InGameMenu)
            {
                if (Setting.OverviewChunk != sliderChunk.Value)
                {
                    ClientMain.World.ChunkPrClient.SetOverviewChunk();
                    ClientMain.Player.SetOverviewChunk(sliderChunk.Value);
                }
                if (Setting.SmoothLighting != cacheSmoothLighting)
                {
                    ClientMain.World.RerenderAllChunks();
                }
            }

            Setting.OverviewChunk = sliderChunk.Value;
            Setting.MusicVolume = sliderMusicVolume.Value;
            Setting.SoundVolume = sliderSoundVolume.Value;
            Setting.Fps = sliderFps.Value;
            Setting.Nickname = textBoxNickname.Text;
            Setting.Language = cacheLanguage;
            Setting.SmoothLighting = cacheSmoothLighting;
            Setting.SetSizeInterface(sliderSizeInterface.Value);
            Setting.SeedBegin = textBoxSeed.Text == "" ? 0 : int.Parse(textBoxSeed.Text);
            Setting.Save();
            Language.SetLanguage((AssetsLanguage)cacheLanguage);

            OnFinished(where);
        }
    }
}
