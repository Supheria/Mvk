using MvkClient.World;
using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Chunk;
using System.Diagnostics;

namespace MvkClient.Actions
{
    public class Keyboard
    {
        /// <summary>
        /// Основной клиент
        /// </summary>
        public Client ClientMain { get; private set; }
        /// <summary>
        /// Клиентский объект мира
        /// </summary>
        public WorldClient World { get; private set; }
        /// <summary>
        /// Нажата ли клавиша Shift
        /// </summary>
        public bool KeyShift { get; private set; }

        /// <summary>
        /// Объект времени с момента запуска проекта
        /// </summary>
        private static Stopwatch stopwatch = new Stopwatch();
        /// <summary>
        /// Какая клавиша была нажата ранее
        /// </summary>
        private int keyPrev;
        /// <summary>
        /// Нажатая ли F3
        /// </summary>
        private bool keyF3 = false;

        public Keyboard(WorldClient world)
        {
            World = world;
            ClientMain = world.ClientMain;
            stopwatch.Start();
        }

        public void KeyShiftDown() => KeyShift = true;
        public void KeyShiftUp() => KeyShift = false;

        /// <summary>
        /// Нажата клавиша
        /// </summary>
        /// <param name="key">индекс клавиши</param>
        public void Down(int key)
        {
            if (key == 32 && !ClientMain.KeyBind.up)
            {
                long ms = stopwatch.ElapsedMilliseconds;
                stopwatch.Restart();
                if (ms < 300 && key == keyPrev)
                {
                    if (ClientMain.Player.IsCreativeMode)
                    {
                        // Только креатив может летать, покуда
                        if (ClientMain.Player.IsFlying) ClientMain.Player.ModeSurvival();
                        else if (!ClientMain.Player.IsSneaking()) ClientMain.Player.ModeFly();
                    }
                }
            }
            keyPrev = key;

            // одно нажатие
            if (key == 66 && keyF3) World.RenderEntityManager.IsHiddenHitbox = !World.RenderEntityManager.IsHiddenHitbox; // F3+B
            if (key == 67 && keyF3) Debug.IsDrawServerChunk = !Debug.IsDrawServerChunk; // F3+C
            if (key == 71 && keyF3) World.WorldRender.ChunkCursorHiddenShow(); // F3+G
            else if (key == 114) keyF3 = true; // F3
            else if (key == 27 || key == 18) World.ClientMain.Screen.InGameMenu(); // Esc или Alt
            else if (key == 116) ClientMain.Player.ViewCameraNext(); // F5
            else if (key == 117) Debug.ScreenFileBiome(World); // F6
            else if (key == 118) 
            {
                if (++Debug.IsDrawOrto > 5) Debug.IsDrawOrto = 0; // F7
            }
            else if (key == 119) Debug.IsDrawVoxelLine = !Debug.IsDrawVoxelLine; // F8
            else if (key == 69) World.ClientMain.Screen.InGameConteiner(); // E
            else if (key == 75) ClientMain.Player.Kill(); // K
            else if (key == 82)
            {
                ChunkBase chunk = World.GetChunk(ClientMain.Player.GetChunkPos());
                chunk.ModifiedToRender(); // R
            }

            else if (key == 65) ClientMain.KeyBind.left = true;
            else if (key == 68) ClientMain.KeyBind.right = true;
            else if (key == 87) ClientMain.KeyBind.forward = true;
            else if (key == 83) ClientMain.KeyBind.back = true;
            else if (key == 32) ClientMain.KeyBind.up = true;
            else if (key == 16) ClientMain.KeyBind.down = true;
            else if (key == 17) ClientMain.KeyBind.sprinting = true;

            //else if (key == 117) // F6
        }

        /// <summary>
        /// Отпущена клавиша
        /// </summary>
        /// <param name="key">индекс клавиши</param>
        public void Up(int key)
        {
            if (key == 114) // F3
            {
                keyF3 = false;
                if (keyPrev == 114) Debug.IsDraw = !Debug.IsDraw; // Нажимали только F3
            }
            else if (key == 65) ClientMain.KeyBind.left = false;
            else if (key == 68) ClientMain.KeyBind.right = false;
            else if (key == 87) ClientMain.KeyBind.forward = false;
            else if (key == 83) ClientMain.KeyBind.back = false;
            else if (key == 32) ClientMain.KeyBind.up = false;
            else if (key == 16) ClientMain.KeyBind.down = false;
            else if (key == 17) ClientMain.KeyBind.sprinting = false;
        }
    }
}
