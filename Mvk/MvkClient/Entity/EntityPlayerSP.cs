using MvkClient.Renderer;
using MvkClient.Renderer.Chunk;
using MvkClient.Util;
using MvkClient.World;
using MvkServer;
using MvkServer.Entity;
using MvkServer.Glm;
using MvkServer.Item;
using MvkServer.Management;
using MvkServer.Network.Packets.Client;
using MvkServer.Network.Packets.Server;
using MvkServer.Util;
using MvkServer.World.Block;
using MvkServer.World.Chunk;
using SharpGL;
using System;
using System.Collections.Generic;

namespace MvkClient.Entity
{
    /// <summary>
    /// Сущность основного игрока тикущего клиента
    /// </summary>
    public class EntityPlayerSP : EntityPlayerClient
    {
        /// <summary>
        /// Плавное перемещение угла обзора
        /// </summary>
        public SmoothFrame Fov { get; private set; }
        /// <summary>
        /// Плавное перемещение глаз, сел/встал
        /// </summary>
        public SmoothFrame Eye { get; private set; }
        /// <summary>
        /// массив матрицы перспективу камеры 3D
        /// </summary>
        public float[] Projection { get; private set; }
        /// <summary>
        /// массив матрицы расположения камеры в пространстве
        /// </summary>
        public float[] LookAt { get; private set; }
        /// <summary>
        /// Матрица просмотра Projection * LookAt
        /// </summary>
        public float[] View { get; private set; }
        /// <summary>
        /// Вектор луча
        /// </summary>
        public vec3 RayLook { get; private set; }

        /// <summary>
        /// Принудительно обработать FrustumCulling
        /// </summary>
        public bool IsFrustumCulling { get; private set; } = false;
        /// <summary>
        /// Объект расчёта FrustumCulling
        /// </summary>
        public Frustum FrustumCulling { get; private set; } = new Frustum();
        /// <summary>
        /// Массив чанков которые попадают под FrustumCulling для рендера
        /// </summary>
        public FrustumStruct[] ChunkFC { get; private set; } = new FrustumStruct[0];
        /// <summary>
        /// Список сущностей которые попали в луч
        /// </summary>
        public EntityPlayerMP[] EntitiesLook { get; private set; } = new EntityPlayerMP[0];
        /// <summary>
        /// Вид камеры
        /// </summary>
        public EnumViewCamera ViewCamera { get; private set; } = EnumViewCamera.Eye;
        /// <summary>
        /// Последнее значение поворота вокруг своей оси
        /// </summary>
        public float RotationYawLast { get; private set; }
        /// <summary>
        /// Последнее значение поворота вверх вниз
        /// </summary>
        public float RotationPitchLast { get; private set; }
        /// <summary>
        /// Выбранный объект
        /// </summary>
        public MovingObjectPosition MovingObject { get; private set; }
        /// <summary>
        /// Позиция камеры в блоке для альфа, в зависимости от вида (с глаз, с зади, спереди)
        /// </summary>
        public vec3i PositionAlphaBlock { get; private set; }
        /// <summary>
        /// Позиция камеры
        /// </summary>
        public vec3 PositionCamera { get; private set; }
        /// <summary>
        /// Для эффекта где находяться глаза
        /// </summary>
        public WhereEyes WhereEyesEff { get; private set; } = WhereEyes.Air;
        /// <summary>
        /// Причина смерти, строка
        /// </summary>
        public string TextDeath { get; private set; } = "";

        /// <summary>
        /// Массив по длинам используя квадратный корень для всей видимости в объёме для обновление чанков в объёме
        /// </summary>
        private vec3i[] distSqrtAlpha;
        /// <summary>
        /// Позиция камеры в чанке для альфа, в зависимости от вида (с глаз, с зади, спереди)
        /// </summary>
        private vec3i positionAlphaChunk;
        /// <summary>
        /// Позиция когда был запрос рендера для альфа блоков для малого смещения, в чанке
        /// </summary>
        private vec3i positionAlphaBlockPrev;
        /// <summary>
        /// Позиция когда был запрос рендера для альфа блоков для большого смещения, за пределами чанка
        /// </summary>
        private vec3i positionAlphaChunkPrev;
        /// <summary>
        /// Позиция с учётом итерполяции кадра
        /// </summary>
        private vec3 positionFrame;
        /// <summary>
        /// Поворот Pitch с учётом итерполяции кадра
        /// </summary>
        private float pitchFrame;
        /// <summary>
        /// Поворот Yaw головы с учётом итерполяции кадра
        /// </summary>
        private float yawHeadFrame;
        /// <summary>
        /// Поворот Yaq тела с учётом итерполяции кадра
        /// </summary>
        private float yawBodyFrame;
        /// <summary>
        /// Высота глаз с учётом итерполяции кадра
        /// </summary>
        private float eyeFrame;

        /// <summary>
        /// Лист DisplayList матрицы 
        /// </summary>
        //private uint dListLookAt;
        /// <summary>
        /// массив векторов расположения камеры в пространстве для DisplayList
        /// </summary>
        private vec3[] lookAtDL;

        /// <summary>
        /// Счётчик паузы анимации левого удара, такты
        /// </summary>
        private int leftClickPauseCounter = 0;
        /// <summary>
        /// Холостой удар
        /// </summary>
        private bool blankShot = false;
        /// <summary>
        /// Активна ли рука в действии
        /// 0 - не активна, 1 - активна левая, 2 - активна правая
        /// </summary>
        private ActionHand handAction = ActionHand.None;
        /// <summary>
        /// Объект работы над игроком разрушения блока, итомы и прочее
        /// </summary>
        private ItemInWorldManager itemInWorldManager;
        /// <summary>
        /// Сущность по которой ударил игрок, если null то нет атаки
        /// </summary>
        private EntityBase entityAtack;
        /// <summary>
        /// Выбранная ячейка
        /// </summary>
        private int currentPlayerItem = 0;

        public EntityPlayerSP(WorldClient world) : base(world)
        {
            Fov = new SmoothFrame(1.43f);
            Eye = new SmoothFrame(GetEyeHeight());
            itemInWorldManager = new ItemInWorldManager(world, this);
            MovingObject = new MovingObjectPosition();
            Movement = new MovementInputFromOptions(world.ClientMain.KeyBind);
        }

        /// <summary>
        /// Тип сущности
        /// </summary>
        public override EnumEntities GetEntityType() => IsInvisible() 
            ? EnumEntities.PlayerInvisible 
            : (GetHealth() > 0 && ViewCamera == EnumViewCamera.Eye) ? EnumEntities.PlayerHand : EnumEntities.Player;

        /// <summary>
        /// Возвращает true, если эта вещь названа
        /// </summary>
        public override bool HasCustomName() => false;

        /// <summary>
        /// Пометить игрока как мёртвым и указать ему причину
        /// </summary>
        public void OnDeathClient(string text)
        {
            TextDeath = text;
            SetHealth(0);
        }

        #region DisplayList

        /// <summary>
        /// Обновить матрицу
        /// </summary>
        public void MatrixProjection(float distantion)
        {
            GLWindow.gl.MatrixMode(OpenGL.GL_PROJECTION);
            GLWindow.gl.LoadIdentity();
            GLWindow.gl.Perspective(glm.degrees(Fov.ValueFrame),
                (float)GLWindow.WindowWidth / (float)GLWindow.WindowHeight,
                0.01f, distantion == 0 ? CamersDistance() : distantion);
            GLWindow.gl.MatrixMode(OpenGL.GL_MODELVIEW);
            GLWindow.gl.LoadIdentity();
            if (lookAtDL != null && lookAtDL.Length == 3)
            {
                GLWindow.gl.LookAt(lookAtDL[0].x, lookAtDL[0].y, lookAtDL[0].z,
                lookAtDL[1].x, lookAtDL[1].y, lookAtDL[1].z,
                lookAtDL[2].x, lookAtDL[2].y, lookAtDL[2].z);
            }
        }

        /// <summary>
        /// Обновить матрицу
        /// </summary>
        //private void UpMatrixProjection()
        //{
        //    if (lookAtDL != null && lookAtDL.Length == 3)
        //    {
        //        GLRender.ListDelete(dListLookAt);
        //        dListLookAt = GLRender.ListBegin();
        //        GLWindow.gl.Viewport(0, 0, GLWindow.WindowWidth, GLWindow.WindowHeight);

        //        GLWindow.gl.MatrixMode(OpenGL.GL_PROJECTION);
        //        GLWindow.gl.LoadIdentity();
        //        GLWindow.gl.Perspective(glm.degrees(Fov.ValueFrame), (float)GLWindow.WindowWidth / (float)GLWindow.WindowHeight, 0.001f, CamersDistance());
        //        GLWindow.gl.MatrixMode(OpenGL.GL_MODELVIEW);
        //        GLWindow.gl.LoadIdentity();

        //        GLWindow.gl.LookAt(lookAtDL[0].x, lookAtDL[0].y, lookAtDL[0].z,
        //            lookAtDL[1].x, lookAtDL[1].y, lookAtDL[1].z,
        //            lookAtDL[2].x, lookAtDL[2].y, lookAtDL[2].z);

        //        // Код с фиксированной функцией может использовать альфа-тестирование
        //        // Чтоб корректно прорисовывался кактус
        //        GLWindow.gl.AlphaFunc(OpenGL.GL_GREATER, 0.1f);
        //        GLWindow.gl.Enable(OpenGL.GL_ALPHA_TEST);
        //        //GLWindow.gl.Enable(OpenGL.GL_TEXTURE_2D);
        //        //GLWindow.gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_FILL);
        //        GLRender.ListEnd();
        //    }
        //}

        /// <summary>
        /// Получить максимальную дистанцию для прорисовки, для клиента
        /// </summary>
        private float CamersDistance()
        {
            //float dis = OverviewChunk * 22.624f * 2f;
            float dis = OverviewChunk * 22.6275f; //16f * 1.4143f;
            if (dis < 128f) return 128f;
            return dis;
        }

        #endregion

        /// <summary>
        /// Надо ли обрабатывать LivingUpdate, для мобов на сервере, и игроки у себя локально
        /// </summary>
        protected override bool IsLivingUpdate() => true;

        /// <summary>
        /// Вызывается для обновления позиции / логики объекта
        /// </summary>
        public override void Update()
        {
            // Такты изминения глаз при присидании, и угол обзора при ускорении. Должны быть до base.Update()
            Eye.Update();
            Fov.Update();
            LastTickPos = PositionPrev = Position;
            RotationPitchPrev = RotationPitch;
            RotationYawBodyPrev = RotationYawBody;
            RotationYawHeadPrev = RotationYawHead;

            // счётчик паузы для анимации удара
            leftClickPauseCounter++;
            // тик мгновенного удаления
            itemInWorldManager.UpdateDestroy();
            // обновляем счётчик удара
            ItemInWorldManager.StatusAnimation statusUpdate = itemInWorldManager.UpdateBlock();
            if (statusUpdate == ItemInWorldManager.StatusAnimation.Animation)
            {
                leftClickPauseCounter = 100;
            }
            else if (statusUpdate == ItemInWorldManager.StatusAnimation.NotAnimation)
            {
                leftClickPauseCounter = 0;
            }

            UpdateItemInUse();

            if (!AtackUpdate())
            {
                // Если нет атаки то проверяем установку или разрушение блока
                if (handAction != ActionHand.None)
                {
                    HandActionUpdate();
                    if (leftClickPauseCounter > GetArmSwingAnimationEnd() / 2  // для анимации разбития блока, пока не разобъёшь пауза между ударами
                        || blankShot) // мгновенный удар, первый удар, установить, атака
                    {
                        SwingItem();
                        blankShot = false;
                        leftClickPauseCounter = 0;
                    }
                }
            }

            base.Update();

            // Обновление курсоро, не зависимо от действия игрока, так как рядом может быть изминение
            UpCursor();

            // Проверка на обновление чанков альфа блоков, в такте после перемещения
            UpdateChunkRenderAlphe();

            if (RotationEquals())
            {
                IsFrustumCulling = true;
            }
            if (ActionChanged != EnumActionChanged.None)
            {
                IsFrustumCulling = true;
                UpdateActionPacket();
            }

            // синхронизация выброного слота
            SyncCurrentPlayItem();
        }

        /// <summary>
        /// Удар рукой по сущности в такте игры
        /// </summary>
        private bool AtackUpdate()
        {
            if (entityAtack != null)
            {
                if (!entityAtack.IsDead)
                {
                    SwingItem();
                    vec3 pos = entityAtack.Position + new vec3(.5f);
                    World.SpawnParticle(EnumParticle.Test, 5, pos, new vec3(1), 0);
                    ClientWorld.ClientMain.TrancivePacket(new PacketC03UseEntity(entityAtack.Id, (entityAtack.Position - Position).normalize()));
                    entityAtack = null;
                    return true;
                }
                entityAtack = null;
            }
            return false;
        }

        /// <summary>
        /// Размахивает предметом, который держит игрок
        /// </summary>
        public override void SwingItem()
        {
            base.SwingItem();
            ClientWorld.ClientMain.TrancivePacket(new PacketC0AAnimation());
        }

        /// <summary>
        /// Обновление в каждом тике, если были требования по изминению позицыи, вращения, бег, сидеть и тп.
        /// </summary>
        private void UpdateActionPacket()
        {
            bool isSS = false;
            if (ActionChanged.HasFlag(EnumActionChanged.IsSneaking))
            {
                Eye.Set(GetEyeHeight(), 4);
                isSS = true;
            }
            if (ActionChanged.HasFlag(EnumActionChanged.IsSprinting))
            {
                Fov.Set(IsSprinting() ? 1.62f : 1.43f, 4);
                isSS = true;
            }

            bool isPos = ActionChanged.HasFlag(EnumActionChanged.Position);
            bool isLook = ActionChanged.HasFlag(EnumActionChanged.Look);

            if (isPos && isLook)
            {
                ClientWorld.ClientMain.TrancivePacket(new PacketC06PlayerPosLook(
                    Position, RotationYawHead, RotationPitch, IsSneaking(), IsSprinting(), OnGround
                ));
            }
            else if (isLook)
            {
                ClientWorld.ClientMain.TrancivePacket(new PacketC05PlayerLook(RotationYawHead, RotationPitch, IsSneaking()));
            }
            else if (isPos || isSS)
            {
                ClientWorld.ClientMain.TrancivePacket(new PacketC04PlayerPosition(Position, IsSneaking(), IsSprinting(), OnGround));
            }
            ActionNone();
        }

        /// <summary>
        /// Проверка изменения вращения
        /// </summary>
        /// <returns>True - разные значения, надо InitFrustumCulling</returns>
        public bool RotationEquals()
        {
            if (RotationPitchLast != RotationPitch || RotationYawLast != RotationYawHead || RotationYawBody != RotationYawBodyPrev)
            {
                SetRotationHead(RotationYawLast, RotationPitchLast);
                return true;
            }
            return false;
        }

        public void MouseMove(float deltaX, float deltaY)
        {
            // Чувствительность мыши
            float speedMouse = 3f;// 2f;// 1.5f;

            if (deltaX == 0 && deltaY == 0) return;
            float pitch = RotationPitchLast - deltaY / (float)GLWindow.WindowHeight * speedMouse;
            float yaw = RotationYawLast + deltaX / (float)GLWindow.WindowWidth * speedMouse;

            if (pitch < -glm.radians(89.0f)) pitch = -glm.radians(89.0f);
            if (pitch > glm.radians(89.0f)) pitch = glm.radians(89.0f);
            if (yaw > glm.pi) yaw -= glm.pi360;
            if (yaw < -glm.pi) yaw += glm.pi360;

            RotationYawLast = yaw;
            RotationPitchLast = pitch;

            UpCursor();
        }

        /// <summary>
        /// Обновить курсор
        /// </summary>
        private void UpCursor()
        {
            if (IsInvisible())
            {
                // Если невидимка мы не можем выбирать объекты
                if (MovingObject.IsCollision())
                {
                    // Если был объект убираем его
                    MovingObject = new MovingObjectPosition();
                }
                return;
            }
            MovingObject = RayCast();
            if (MovingObject.IsBlock())
            {
                ChunkBase chunk = World.GetChunk(MovingObject.BlockPosition.GetPositionChunk());
                vec3i pos = MovingObject.BlockPosition.GetPosition0();
                string s1 = ToBlockDebug(chunk, pos);
                string strUp = "";
                if (MovingObject.BlockPosition.Y < ChunkBase.COUNT_HEIGHT_BLOCK)
                {
                    BlockPos blockPosUp = MovingObject.BlockPosition.OffsetUp();
                    strUp = string.Format(
                        "BlkUp:{0} {1} L:{2}",
                        blockPosUp,
                        World.GetBlockState(blockPosUp),
                        ToBlockDebug(chunk, blockPosUp.GetPosition0())
                    );
                }
                Debug.BlockFocus = string.Format(
                    "Block:{0} {1}{6} L:{2}\r\n{3}\r\n{4}, {5}\r\n",
                    MovingObject.BlockPosition,
                    MovingObject.Block,
                    s1,
                    strUp,
                    chunk.Light.GetHeight(pos.x, pos.z),
                    chunk.GetDebugAllSegment(),
                    MovingObject.IsLiquid ? string.Format(" {0} {1}", MovingObject.EBlockLiquid, MovingObject.BlockLiquidPosition) : ""
                );
            }
            else if (MovingObject.IsLiquid)
            {
                Debug.BlockFocus = string.Format("Liquid:{0} {1}\r\n", MovingObject.EBlockLiquid, MovingObject.BlockLiquidPosition);
            }
            else if (MovingObject.IsEntity())
            {
                Debug.BlockFocus = MovingObject.Entity.GetName() + "\r\n";
            }
            else
            {
                Debug.BlockFocus = "";
            }
        }


        private string ToBlockDebug(ChunkBase chunk, vec3i pos)
        {
            if (pos.y > ChunkBase.COUNT_HEIGHT_BLOCK) return "";
            ChunkStorage storage = chunk.StorageArrays[pos.y >> 4];
           // if (!chunkStorage.IsEmptyData())
            {
                int index = (pos.y & 15) << 8 | pos.z << 4 | pos.x;
                return string.Format("[{2}] b{0} s{1} {3}", storage.lightBlock[index], storage.lightSky[index], pos,
                     chunk.biome[pos.x << 4 | pos.z]); //, chunkStorage.ToString());
            }
            //return "@-@";
        }
        /// <summary>
        /// Обновить матрицу камеры
        /// </summary>
        public bool UpView(float timeIndex)
        {
            int ort = Debug.IsDrawOrto * 2;
            mat4 projection = Debug.IsDrawOrto > 0
                ? glm.ortho(-GLWindow.WindowWidth / ort, GLWindow.WindowWidth / ort, -GLWindow.WindowHeight / ort, GLWindow.WindowHeight / ort, -250, 500)
                : glm.perspective(Fov.ValueFrame, (float)GLWindow.WindowWidth / (float)GLWindow.WindowHeight, 0.01f, CamersDistance());
            Projection = projection.to_array();

            vec3 pos = new vec3(0, GetEyeHeightFrame(), 0);
            vec3 front = GetLookFrame(timeIndex).normalize();
            vec3 up = new vec3(0, 1, 0);

            if (ViewCamera == EnumViewCamera.Back)
            {
                // вид сзади
                pos = GetPositionCamera(pos, front * -1f, MvkGlobal.CAMERA_DIST);
            } else if (ViewCamera == EnumViewCamera.Front)
            {
                // вид спереди
                pos = GetPositionCamera(pos, front, MvkGlobal.CAMERA_DIST);
                front *= -1f;
            } else
            {
                //vec3 right = glm.cross(up, front).normalize();
                //vec3 f2 = (front * -1f - right * .7f).normalize();
                //pos = GetPositionCamera(pos, f2, 2f);
            }

            if (!IsFlying && MvkGlobal.WIGGLE_EFFECT)
            {
                // Эффект болтания когда игрок движется 
                vec3 right = glm.cross(up, front).normalize();
                float limbS = GetLimbSwingAmountFrame(timeIndex) * .12f;
                // эффект лево-право
                float limb = glm.cos(LimbSwing * 0.3331f) * limbS;
                pos += right * limb;
                up = glm.cross(front, right);
                // эффект вверх-вниз
                limb = glm.cos(LimbSwing * 0.6662f) * limbS;
                pos += up * limb;
            }
            mat4 look = glm.lookAt(pos, pos + front, up);
            View = (projection * look).to_array();
            float[] lookAt = look.to_array();
            if (!Mth.EqualsArrayFloat(lookAt, LookAt, 0.00001f))
            {
                PositionCamera = pos;
                LookAt = lookAt;
                RayLook = front;
                lookAtDL = new vec3[] { pos, pos + front, up };
                return true;
            }
            return false;
        }

        /// <summary>
        /// Обновление в кадре
        /// </summary>
        public void UpdateFrame(float timeIndex)
        {
            // Меняем положения глаз
            Eye.UpdateFrame(timeIndex);
            eyeFrame = Eye.ValueFrame;

            positionFrame = base.GetPositionFrame(timeIndex);
            yawBodyFrame = GetRotationYawBodyFrame(timeIndex);
            yawHeadFrame = GetRotationYawFrame(timeIndex);
            pitchFrame = GetRotationPitchFrame(timeIndex);

            // Меняем угол обзора (как правило при изменении скорости)
            if (Fov.UpdateFrame(timeIndex)) { }
            UpProjection();

            vec3 offset = new vec3(positionFrame.x, positionFrame.y + eyeFrame, positionFrame.z);
            ClientWorld.RenderEntityManager.SetCamera(positionFrame, yawHeadFrame, pitchFrame);

            // Изменяем матрицу глаз игрока
            if (UpView(timeIndex) || IsFrustumCulling)
            {
                // Если имеется вращение камеры или было перемещение, то запускаем расчёт FrustumCulling
                InitFrustumCulling();

                // Определяем где глаза
                vec3 posCam = Position + PositionCamera;
                BlockPos blockPos = new BlockPos(posCam);
                BlockState blockState = World.GetBlockState(blockPos);
                BlockBase block = blockState.GetBlock();
                switch (block.Material.EMaterial)
                {
                    case EnumMaterial.Lava: WhereEyesEff = WhereEyes.Lava; break;
                    case EnumMaterial.Oil: WhereEyesEff = WhereEyes.Oil; break;
                    case EnumMaterial.Water: WhereEyesEff = WhereEyes.Water; break;
                    default: WhereEyesEff = WhereEyes.Air; break;
                }

                if (WhereEyesEff != WhereEyes.Air)
                {
                    if (block.EBlock == EnumBlock.WaterFlowing || block.EBlock == EnumBlock.OilFlowing || block.EBlock == EnumBlock.LavaFlowing)
                    {
                        float h = blockState.met / 15f;
                        float h2 = blockPos.Y + h;
                        if (posCam.y >= h2) WhereEyesEff = WhereEyes.Air;
                    }
                    else if (World.GetBlockState(blockPos.OffsetUp()).IsAir())
                    {
                        if (posCam.y >= blockPos.Y + .9f) WhereEyesEff = WhereEyes.Air;
                    }
                }
            }
        }

        /// <summary>
        /// Требуется перерасчёт FrustumCulling
        /// </summary>
        public void UpFrustumCulling() => IsFrustumCulling = true;

        /// <summary>
        /// Перерасчёт FrustumCulling
        /// </summary>
        public void InitFrustumCulling()
        {
            if (LookAt == null || Projection == null) return;
            FrustumCulling.Init(LookAt, Projection);
            
            Debug.DrawFrustumCulling.Clear();
            int countFC = 0;
            vec3i chunkPos = new vec3i(
                Mth.Floor(positionFrame.x) >> 4,
                Mth.Floor(positionFrame.y) >> 4,
                Mth.Floor(positionFrame.z) >> 4);
            
            List<FrustumStruct> listC = new List<FrustumStruct>();

            if (DistSqrt != null)
            {
                int i, xc, zc, xb, zb, x1, y1, z1, x2, y2, z2;
                FrustumStruct frustum;
                ChunkRender chunk;
                vec2i coord;
                vec2i vec;
                int countOC = DistSqrt.Length;
                for (i = 0; i < countOC; i++)
                {
                    vec = DistSqrt[i];
                    xc = vec.x;
                    zc = vec.y;
                    xb = xc << 4;
                    zb = zc << 4;

                    x1 = xb - 15;
                    y1 = -255;
                    z1 = zb - 15;
                    x2 = xb + 15;
                    y2 = 255;
                    z2 = zb + 15;
                    if (FrustumCulling.IsBoxInFrustum(x1, y1, z1, x2, y2, z2))
                    {
                        coord = new vec2i(xc + chunkPos.x, zc + chunkPos.z);
                        chunk = ClientWorld.ChunkPrClient.GetChunkRender(coord);
                        if (chunk == null) frustum = new FrustumStruct(coord);
                        else frustum = new FrustumStruct(chunk);

                        int count = frustum.FrustumShow(FrustumCulling, x1, z1, x2, z2, Mth.Floor(positionFrame.y + eyeFrame));// positionFrame.y);
                        if (count > 0)
                        {
                            if (Debug.IsDrawFrustumCulling)
                            {
                                coord = new vec2i(xc, zc);
                                if (!Debug.DrawFrustumCulling.Contains(coord)) Debug.DrawFrustumCulling.Add(coord);
                            }
                            listC.Add(frustum);
                        }
                        // Для подсчёта чанков меша
                        countFC++;
                    }
                }
            }
            Debug.CountMeshAll = countFC;
            Debug.RenderFrustumCulling = true;
            ChunkFC = listC.ToArray();
            IsFrustumCulling = false;
        }

        /// <summary>
        /// Проверить не догруженые чанки и догрузить если надо
        /// </summary>
        public void CheckChunkFrustumCulling()
        {
            int i;
            FrustumStruct fs;
            ChunkRender chunk;
            for (i = 0; i < ChunkFC.Length; i++)
            {
                fs = ChunkFC[i];
                if (!fs.IsChunk())
                {
                    chunk = ClientWorld.ChunkPrClient.GetChunkRender(fs.GetCoord());
                    if (chunk != null)
                    {
                        ChunkFC[i] = new FrustumStruct(chunk, fs.GetSortList());
                    }
                }
            }
        }

        /// <summary>
        /// Следующий вид камеры
        /// </summary>
        public void ViewCameraNext()
        {
            int count = Enum.GetValues(typeof(EnumViewCamera)).Length - 1;
            int value = (int)ViewCamera;
            value++;
            if (value > count) value = 0;
            ViewCamera = (EnumViewCamera)value;
        }

        /// <summary>
        /// Занести массив сущностей попадающих в луч
        /// </summary>
        public void SetEntitiesLook(List<EntityPlayerMP> entities) => EntitiesLook = entities.ToArray();

        /// <summary>
        /// Определить положение камеры, при виде сзади и спереди, проверка RayCast
        /// </summary>
        /// <param name="pos">позиция глаз</param>
        /// <param name="vec">направляющий вектор к расположению камеры</param>
        private vec3 GetPositionCamera(vec3 pos, vec3 vec, float dis)
        {
            vec3 offset = ClientWorld.RenderEntityManager.CameraOffset;
            if (IsInvisible())
            {
                return pos + vec * dis;
            }
            MovingObjectPosition moving = World.RayCastBlock(pos + offset, vec, dis, true);
            return pos + vec * (moving.IsBlock() ? glm.distance(pos, moving.RayHit + new vec3(moving.Norm) * .5f - offset) : dis);
        }

        /// <summary>
        /// Получить объект по тикущему лучу
        /// </summary>
        private MovingObjectPosition RayCast()
        {
            // максимальная дистанция луча
            float maxDis = MvkGlobal.RAY_CAST_DISTANCE;
            vec3 pos = GetPositionFrame();
            pos.y += GetEyeHeightFrame();
            // нормализованный вектор луча
            vec3 dir = RayLook;
            // вектор луча с дистанцией
            vec3 vecRay = dir * maxDis;
            // Луч на поиск блока
            MovingObjectPosition moving = World.RayCastBlock(pos, dir, maxDis, false);

            // Рамка луча для поиска сущностей, т.е. от позиции до дистанции куда смотрим
            AxisAlignedBB axis = BoundingBox.AddCoordBias(vecRay);
            // Собираем все сущности которые могут соприкосатся с рамкой обзора
            List<EntityBase> list = World.GetEntitiesWithinAABB(ChunkBase.EnumEntityClassAABB.EntityLiving, axis, Id);
            MovingObjectPosition moving2 = null;
            EntityBase entityHit = null;
            vec3 rayHit = new vec3(0);
            EntityBase entity;
            float distance = moving.IsBlock() ? glm.distance(pos, moving.RayHit) : 0;
            for (int i = 0; i < list.Count; i++)
            {
                entity = list[i];
                // Проверка может ли сущность быть в проверке
                if (entity.CanBeCollidedWith())
                {
                    // Дополнительная обработка рамки, с целью проверить точное косание сущности
                    float size = entity.GetCollisionBorderSize();
                    moving2 = entity.BoundingBox.Expand(new vec3(size)).CalculateIntercept(pos, pos + vecRay);
                    if (moving2 != null)
                    {
                        // Если косание сущности было фиксируем, и фиксируем на иближайшее к позиции прошлого такта
                        float f = glm.distance(pos, moving2.RayHit);
                        if (f < distance || distance == 0)
                        {
                            entityHit = entity;
                            rayHit = moving2.RayHit;
                            distance = f;
                        }
                    }
                }
            }
            if (entityHit != null)
            {
                // Помечаем что было поподание по сущности
                moving = new MovingObjectPosition(entityHit, rayHit);
            }
            return moving;
        }

        /// <summary>
        /// Действие правой рукой
        /// </summary>
        private void HandActionUpdate()
        {
            if (!IsInvisible())
            {
                MovingObjectPosition moving = RayCast();

                if (handAction == ActionHand.Left)
                {
                    // Разрушаем блок
                    if (itemInWorldManager.IsDestroyingBlock && ((moving.IsBlock() && !itemInWorldManager.BlockPosDestroy.ToVec3i().Equals(moving.BlockPosition.ToVec3i())) || !moving.IsBlock()))
                    {
                        // Отмена разбитие блока, сменился блок
                        ClientMain.TrancivePacket(new PacketC07PlayerDigging(itemInWorldManager.BlockPosDestroy, PacketC07PlayerDigging.EnumDigging.About));
                        ItemInWorldManagerDestroyAbout();
                    }
                    else if (itemInWorldManager.IsDestroyingBlock)
                    {
                        // Этап разбития блока
                        if (itemInWorldManager.IsDestroy())
                        {
                            // Стоп, окончено разбитие
                            ClientMain.TrancivePacket(new PacketC07PlayerDigging(itemInWorldManager.BlockPosDestroy, PacketC07PlayerDigging.EnumDigging.Stop));
                            itemInWorldManager.DestroyStop();
                        }
                    }
                    else if (itemInWorldManager.NotPauseUpdate)
                    {
                        DestroyingBlockStart(moving, false);
                    }
                }
                else if (handAction == ActionHand.Right && itemInWorldManager.NotPauseUpdate)
                {
                    // устанавливаем блок
                    HandActionRightStart(moving, false);
                }
            }
        }

        /// <summary>
        /// Действие правой рукой
        /// </summary>
        public void HandAction()
        {
            if (!IsInvisible())
            {
                MovingObjectPosition moving = RayCast();
                if (moving.IsEntity())
                {
                    // Курсор попал на сущность
                    entityAtack = moving.Entity;
                    handAction = ActionHand.None;
                }
                else
                {
                    DestroyingBlockStart(moving, true);
                    handAction = ActionHand.Left;
                }
            }
        }

        /// <summary>
        /// Остановить юз предмета
        /// </summary>
        public void OnStoppedUsingItem()
        {
            StopUsingItem();
            ClientMain.TrancivePacket(new PacketC0CPlayerAction(PacketC0CPlayerAction.EnumAction.StopUsingItem));
        }

        /// <summary>
        /// Действия правой клавишей мыши, ставим блок
        /// </summary>
        public void HandActionRight()
        {
            if (!IsInvisible())
            {
                MovingObjectPosition moving = RayCast();
                HandActionRightStart(moving, true);
            }
        }

        /// <summary>
        /// Начало действия правой клавишей мыши
        /// </summary>
        /// <param name="moving">объект луча</param>
        /// <param name="start">true - только нажали, false - повторное нажатие если не отпускали клавишу</param>
        private void HandActionRightStart(MovingObjectPosition moving, bool start)
        {
            bool click = false;
            ItemStack itemStack;
            if (start && moving.IsBlock() && !IsSneaking())
            {
                // клик по блоку
                BlockBase block = moving.Block.GetBlock();
                click = block.OnBlockActivated(World, this, moving.BlockPosition, moving.Block, moving.Side, moving.Facing);
                // Отправляем на сервер клик по блоку
                //TODO::2023-08-07 если убрать коммент click то не открывается первый крафт, а ранее добавли, из-за кремния, вроде
                //     if (click) 
                {
                    ClientMain.TrancivePacket(new PacketC08PlayerBlockPlacement(moving.BlockPosition));
                }
                if (!click)
                {
                    itemStack = Inventory.GetCurrentItem();
                    if ((itemStack == null || (itemStack != null && itemStack.Item.IsTool())) && block.Material.SimpleCraft
                        && ClientMain.World.GetBlockState(moving.BlockPosition.OffsetUp()).IsAir())
                    {
                        // Проверка, на инструмент или кулак и проверка твёрдого блока и сверху небо
                        //TODO::2023-07-09 Открываем верстак выживания
                        if (ClientMain.Player.IsCreativeMode)
                        {
                            ClientMain.Screen.InGameConteinerCreative();
                        }
                        else
                        {
                            ClientMain.Screen.InGameWindow(EnumWindowType.CraftFirst);
                        }
                        return;
                    }
                }
            }
            if (!click)
            {
                itemStack = Inventory.GetCurrentItem();
                if (itemStack != null)
                {
                    // В стаке блок, и по лучу можем устанавливать блок
                    if (itemStack.ItemUse(this, World, moving.BlockPosition, moving.Side, moving.Facing))
                    {
                        ClientMain.TrancivePacket(new PacketC08PlayerBlockPlacement(moving.BlockPosition, moving.Side, moving.Facing));
                        itemInWorldManager.Put(moving.BlockPosition, moving.Side, moving.Facing, Inventory.CurrentItem);
                        itemInWorldManager.PutPause(start);
                        blankShot = true;
                    }
                    else
                    {
                        itemInWorldManager.PutAbout();
                        // Использовать элемент правой кнопкой мыши
                        ItemStack itemStackNew = itemStack.UseItemRightClick(World, this);
                        EnumItemAction itemAction = itemStack.GetItemUseAction();
                        if (!itemStack.Equals(itemStackNew) || itemAction == EnumItemAction.Drink || itemAction == EnumItemAction.Eat || itemAction == EnumItemAction.Throw)
                        {
                            itemInWorldManager.PutPause(true);
                            ClientMain.TrancivePacket(new PacketC08PlayerBlockPlacement(2));
                        }
                    }
                    handAction = ActionHand.Right;
                }
            }
        }

        /// <summary>
        /// Начало разрушения блока
        /// </summary>
        private void DestroyingBlockStart(MovingObjectPosition moving, bool start)
        {
            if (!itemInWorldManager.IsDestroyingBlock && moving.IsBlock())
            {
                BlockState blockState = World.GetBlockState(moving.BlockPosition);
                BlockBase block = blockState.GetBlock();

                if (itemInWorldManager.CanDestroy(block))
                {
                    int hardness = block.GetPlayerRelativeBlockHardness(this, blockState);
                    if (hardness == 0)
                    {
                        // Ломаем мгновенно, без захода в тик
                        ClientMain.TrancivePacket(new PacketC07PlayerDigging(moving.BlockPosition, PacketC07PlayerDigging.EnumDigging.Destroy));
                        itemInWorldManager.InstantDestroy(moving.BlockPosition);
                        if (start) blankShot = true;
                    }
                    else
                    {
                        // Ломаем через тик
                        if (itemInWorldManager.CheckInitialDamage(hardness))
                        {
                            itemInWorldManager.HitOnBlock(moving.BlockPosition, block, true);
                            ClientMain.TrancivePacket(new PacketC07PlayerDigging(moving.BlockPosition, PacketC07PlayerDigging.EnumDigging.Hit));
                        }
                        // Начало разбитие блока
                        ClientMain.TrancivePacket(new PacketC07PlayerDigging(moving.BlockPosition, PacketC07PlayerDigging.EnumDigging.Start));
                        itemInWorldManager.DestroyStart(moving.BlockPosition, hardness);
                    }
                }
                else if (start)
                {
                    itemInWorldManager.HitOnBlock(moving.BlockPosition, block, true);
                    ClientMain.TrancivePacket(new PacketC07PlayerDigging(moving.BlockPosition, PacketC07PlayerDigging.EnumDigging.Hit));
                    blankShot = true;
                }
            }
            else if (start) blankShot = true;
        }

        /// <summary>
        /// Отмена действия правой рукой
        /// </summary>
        public void UndoHandAction()
        {
            if (itemInWorldManager.IsDestroyingBlock)
            {
                ClientMain.TrancivePacket(new PacketC07PlayerDigging(itemInWorldManager.BlockPosDestroy, PacketC07PlayerDigging.EnumDigging.About));
                ItemInWorldManagerDestroyAbout();
            }
            handAction = ActionHand.None;
        }

        /// <summary>
        /// Нет управления
        /// </summary>
        public override void MovementNone()
        {
            base.MovementNone();
            ((WorldClient)World).ClientMain.KeyBind.InputNone();
            UndoHandAction();
        }

        public override void Respawn()
        {
            base.Respawn();
            ClientWorld.ClientMain.GameMode();
        }

        /// <summary>
        /// Падение
        /// </summary>
        protected override void Fall()
        {
            if (fallDistanceResult > .0001f)
            {
                ClientWorld.ClientMain.TrancivePacket(new PacketC0CPlayerAction(PacketC0CPlayerAction.EnumAction.Fall, fallDistanceResult));
                ParticleFall(fallDistanceResult);
                fallDistanceResult = 0;
            }
        }

        /// <summary>
        /// Задать место положение игрока, при спавне, телепорте и тп
        /// </summary>
        public override void SetPosLook(vec3 pos, float yaw, float pitch)
        {
            base.SetPosLook(pos, yaw, pitch);
            RotationYawLast = RotationYawBody;
            RotationPitchLast = RotationPitch;
        }

        /// <summary>
        /// Задать обзор чанков у клиента
        /// </summary>
        public override void SetOverviewChunk(int overviewChunk)
        {
            OverviewChunkPrev = OverviewChunk = overviewChunk;
            DistSqrt = MvkStatic.DistSqrtTwo2d[OverviewChunk];
            distSqrtAlpha = MvkStatic.DistSqrtTwo3d[OverviewChunk < MvkGlobal.UPDATE_ALPHE_CHUNK ? OverviewChunk : MvkGlobal.UPDATE_ALPHE_CHUNK];
        }

        /// <summary>
        /// Проверить изменение слота если изменён, отправить на сервер
        /// </summary>
        private void SyncCurrentPlayItem()
        {
            int currentItem = Inventory.CurrentItem;
            if (currentItem != currentPlayerItem)
            {
                currentPlayerItem = currentItem;
                ClientMain.TrancivePacket(new PacketC09HeldItemChange(currentPlayerItem));
                // Отмена разрушения блока если было смена предмета в руке
                ClientMain.TrancivePacket(new PacketC07PlayerDigging(itemInWorldManager.BlockPosDestroy, PacketC07PlayerDigging.EnumDigging.About));
                ItemInWorldManagerDestroyAbout();
            }
        }

        /// <summary>
        /// Проверка на обновление чанков альфа блоков, в такте после перемещения
        /// </summary>
        private void UpdateChunkRenderAlphe()
        {
            PositionAlphaBlock = new vec3i(Position + PositionCamera);
            positionAlphaChunk = new vec3i(PositionAlphaBlock.x >> 4, PositionAlphaBlock.y >> 4, PositionAlphaBlock.z >> 4);

            if (!positionAlphaChunk.Equals(positionAlphaChunkPrev))
            {
                // Если смещение чанком
                positionAlphaChunkPrev = positionAlphaChunk;
                positionAlphaBlockPrev = PositionAlphaBlock;
                vec2i posCh = GetChunkPos();
                int posY = GetChunkY();
                for (int d = 0; d < distSqrtAlpha.Length; d++)
                {
                    vec2i pos = new vec2i(posCh.x + distSqrtAlpha[d].x, posCh.y + distSqrtAlpha[d].z);
                    ChunkRender chunk = ClientWorld.ChunkPrClient.GetChunkRender(pos);
                    if (chunk != null) chunk.ModifiedToRenderAlpha(posY + distSqrtAlpha[d].y);
                }
            }
            else if (!PositionAlphaBlock.Equals(positionAlphaBlockPrev))
            {
                // Если смещение блока
                positionAlphaBlockPrev = PositionAlphaBlock;
                vec2i posCh = GetChunkPos();
                ClientWorld.ChunkPrClient.ModifiedToRenderAlpha(posCh.x, GetChunkY(), posCh.y);
            }
        }

        /// <summary>
        /// Задать атрибуты игроку
        /// </summary>
        public void SetPlayerAbilities(PacketS39PlayerAbilities packet)
        {
            IsCreativeMode = packet.IsCreativeMode();
            NoClip = packet.IsNoClip();
            IsFlying = AllowFlying = packet.IsAllowFlying();
            DisableDamage = packet.IsDisableDamage();
            //SetInvisible()Invisible = packet.IsInvisible();
        }

        /// <summary>
        /// В менеджере работы с разрушениями блока, делаем отмену разрушения
        /// </summary>
        public void ItemInWorldManagerDestroyAbout() => itemInWorldManager.DestroyAbout();

        /// <summary>
        /// Выбросить предмет который в руке, только для клиента
        /// </summary>
        public void ThrowOutCurrentItem()
        {
            if (Inventory.GetCurrentItem() != null)
            {
                ClientMain.TrancivePacket(new PacketC0CPlayerAction(PacketC0CPlayerAction.EnumAction.ThrowOutCurrentItem));
            }
        }


        #region Frame

        /// <summary>
        /// Высота глаз для кадра
        /// </summary>
        public override float GetEyeHeightFrame() => eyeFrame;

        /// <summary>
        /// Получить позицию сущности для кадра
        /// </summary>
        public vec3 GetPositionFrame() => positionFrame;

        /// <summary>
        /// Получить вектор направления камеры тела для кадра
        /// </summary>
        /// <param name="timeIndex">Коэфициент между тактами</param>
        public override vec3 GetLookBodyFrame(float timeIndex) => GetRay(yawBodyFrame, pitchFrame);

        /// <summary>
        /// Получить вектор направления камеры от головы для кадра
        /// </summary>
        /// <param name="timeIndex">Коэфициент между тактами</param>
        public override vec3 GetLookFrame(float timeIndex) => GetRay(yawHeadFrame, pitchFrame);

        #endregion

        /// <summary>
        /// Действие рук
        /// </summary>
        private enum ActionHand
        {
            /// <summary>
            /// Нет действий
            /// </summary>
            None,
            /// <summary>
            /// Левой рукой
            /// </summary>
            Left,
            /// <summary>
            /// Правой рукой
            /// </summary>
            Right
        }

        /// <summary>
        /// Где глаза
        /// </summary>
        public enum WhereEyes
        {
            Air,
            Water,
            Lava,
            Oil
        }

    }
}
