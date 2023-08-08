using MvkServer.Entity.List;
using MvkServer.Glm;
using MvkServer.Network.Packets.Server;
using MvkServer.Sound;
using MvkServer.TileEntity;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блока ящика 1*1
    /// </summary>
    public class BlockBox : BlockBase
    {
        public BlockBox() : base()
        {
            Combustibility = true;
            IgniteOddsSunbathing = 10;
            BurnOdds = 20;
            Material = Materials.GetMaterialCache(EnumMaterial.WoodTable);
            samplesPut = samplesBreak = new AssetsSample[] { AssetsSample.DigWood1, AssetsSample.DigWood2, AssetsSample.DigWood3, AssetsSample.DigWood4 };
            samplesStep = new AssetsSample[] { AssetsSample.StepWood1, AssetsSample.StepWood2, AssetsSample.StepWood3, AssetsSample.StepWood4 };
            Particle = 521;
            Resistance = 5.0f;

            InitQuads(521);
        }


        /// <summary>
        /// Сколько ударов требуется, чтобы сломать блок в тактах (20 тактов = 1 секунда)
        /// </summary>
        public override int Hardness(BlockState state) => 80;

        /// <summary>
        /// Возвращает количество предметов, которые выпадают при разрушении блока.
        /// </summary>
       // protected override int QuantityDropped(Rand random) => 2;

        /// <summary>
        /// Получите предмет, который должен выпасть из этого блока при сборе.
        /// </summary>
        //protected override ItemBase GetItemDropped(BlockState state, Rand rand, ItemAbTool itemTool)
        //{
        //    int r = rand.Next(10);
        //    if (r < 4) return Items.GetItemCache(EnumBlock.PlanksOak);
        //    if (r < 8) return Items.GetItemCache(EnumItem.WoodChips);
        //    return null;
        //}

        /// <summary>
        /// Активация блока, клик правой клавишей мыши по блоку, true - был клик, false - нет такой возможности
        /// </summary>
        public override bool OnBlockActivated(WorldBase worldIn, EntityPlayer entityPlayer, BlockPos blockPos, BlockState state, Pole side, vec3 facing)
        {
            if (!worldIn.IsRemote && entityPlayer is EntityPlayerServer entityPlayerServer)
            {
                // Открыть окно
                entityPlayerServer.SendPacket(new PacketS2DOpenWindow(EnumWindowType.Box));
            }
            return true;
        }

        /// <summary>
        /// Действие блока после его установки только на сервере
        /// </summary>
        public override void OnBlockAdded(WorldBase worldIn, BlockPos blockPos, BlockState state)
        {
            worldIn.CreateTileEntity(EnumTileEntities.Chest, blockPos, state);
        }

        /// <summary>
        /// Действие блока после его удаления
        /// </summary>
        public override void OnBreakBlock(WorldBase worldIn, BlockPos blockPos, BlockState state) 
            => worldIn.RemoveTileEntity(blockPos);

        /// <summary>
        /// Открыли окно, вызывается объектом EntityPlayerServer
        /// </summary>
        public override void OpenWindow(WorldServer worldServer, EntityPlayerServer entityPlayer, BlockPos blockPos)
        {
            OpenWindowTileEntity(worldServer, entityPlayer, blockPos);
        }

    }
}
