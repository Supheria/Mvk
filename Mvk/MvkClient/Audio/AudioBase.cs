﻿using MvkAssets;
using MvkClient.Setitings;
using MvkServer.Glm;
using MvkServer.Sound;
using System;

namespace MvkClient.Audio
{
    /// <summary>
    /// Базовый класс звуков
    /// </summary>
    public class AudioBase
    {
        /// <summary>
        /// Массив всех семплов
        /// </summary>
        private AudioSample[] items;
        /// <summary>
        /// Объект источников звука
        /// </summary>
        private readonly AudioSources sources = new AudioSources();
        /// <summary>
        /// Строка для дэбага сколько источников и занятых
        /// </summary>
        public string StrDebug { get; protected set; }

        public void Initialize()
        {
            // Инициализация звука
            IntPtr pDevice = Al.alcOpenDevice(null);
            IntPtr pContext = Al.alcCreateContext(pDevice, null);
            Al.alcMakeContextCurrent(pContext);

            // Инициализация источников звука
            sources.Initialize();
        }

        /// <summary>
        /// Инициализировать длинну массива для семплов
        /// </summary>
        public void InitializeArray(int count) => items = new AudioSample[count];

        /// <summary>
        /// Загрузка сэмпла
        /// </summary>
        public void InitializeSample(AssetsSample key)
        {
            byte[] vs = Assets.GetSample(key.ToString());
            AudioSample sample = new AudioSample();
            sample.LoadOgg(vs);
            items[(int)key] = sample;
        }

        /// <summary>
        /// Такт
        /// </summary>
        public void Tick()
        {
            sources.AudioTick();
            StrDebug = string.Format("{0}/{1}", sources.CountProcessing, sources.CountAll);
        }

        /// <summary>
        /// Проиграть звук
        /// </summary>
        public void PlaySound(AssetsSample key, vec3 pos, float volume, float pitch)
        {
            if (Setting.SoundVolume > 0)
            {
                AudioSample sample = items[(int)key];
                if (sample != null && sample.Size > 0)
                {
                    AudioSource source = sources.GetAudio();
                    if (source != null)
                    {
                        source.Sample(sample);
                        source.Play(pos, volume * Setting.ToFloatSoundVolume(), pitch);
                    }
                }
            }
        }
        /// <summary>
        /// Проиграть звук
        /// </summary>
        public void PlaySound(AssetsSample key) => PlaySound(key, new vec3(0), 1f, 1f);
        /// <summary>
        /// Проиграть звук
        /// </summary>
        public void PlaySound(AssetsSample key, float volume) => PlaySound(key, new vec3(0), volume, 1f);
    }
}
