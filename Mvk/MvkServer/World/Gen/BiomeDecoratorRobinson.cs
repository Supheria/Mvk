using MvkServer.Util;
using MvkServer.World.Block;
using MvkServer.World.Gen.Feature;

namespace MvkServer.World.Gen
{
    /// <summary>
    /// Докорация в биомах серии робинзон
    /// </summary>
    public class BiomeDecoratorRobinson : BiomeDecorator
    {
        public BiomeDecoratorRobinson(WorldServer world) : base(world) { }

        public override void Init()
        {
            genPlants = new WorldGenPlants();
            genCactus = new WorldGenCactus();
            genOak = new WorldGenTreeOak();
            genBirch = new WorldGenTreeBirch();
            genSpruce = new WorldGenTreeSpruce();
            genFruit = new WorldGenTreeFruit();
            genPalm = new WorldGenTreePalm();

            genBrol = new WorldGenBrol();
            genCoal = new WorldGenMinable(new BlockState(EnumBlock.OreCoal), 17);
            genIron = new WorldGenMinable(new BlockState(EnumBlock.OreIron), 9);
            genGold = new WorldGenMinable(new BlockState(EnumBlock.OreGold), 7); // 9
            genDirt = new WorldGenMinable(new BlockState(EnumBlock.Dirt), 33);
            gemGravel = new WorldGenMinable(new BlockState(EnumBlock.Gravel), 33);
            genGranite = new WorldGenMinable(new BlockState(EnumBlock.Granite), 33);
            genLimestone = new WorldGenMinable(new BlockState(EnumBlock.Limestone), 33);
            genOil = new WorldGenMinable(new BlockState(EnumBlock.Oil), 24);

            genPancakeSand = new WorldGenPancake(EnumBlock.Sand, 9, 8);
            gemPancakeGravel = new WorldGenPancake(EnumBlock.Gravel, 7, 6);
            genPancakeClay = new WorldGenPancake(EnumBlock.Clay, 5, 4);
            genPancakeDirt = new WorldGenPancake(EnumBlock.Dirt, 9, 7);

            oilPerChunk = 0;
            dirtPerChunk = 0;
            gravelPerChunk = 0;
            granitePerChunk = 0;
            limestonePerChunk = 0;
            coalPerChunk = 0;
            ironPerChunk = 0;
            goldPerChunk = 0;
        }

        protected override void GenStandardOres()
        {
            if (oilPerChunk > 0 && currentRand.Next(oilPerChunk) != 0)
            {
                GenStandardOre(1, genOil, 16, 40);
            }
            //GenStandardOre(dirtPerChunk, genDirt, 1, 112);
            //GenStandardOre(gravelPerChunk, gemGravel, 42, 80);
            GenStandardOre(granitePerChunk, genGranite, 8, 112); // 80
            GenStandardOre(limestonePerChunk, genLimestone, 8, 112); // 80
            GenStandardOre(coalPerChunk, genCoal, 8, 80); // 128
            GenStandardOre(ironPerChunk, genIron, 8, 64); // 64
            GenStandardOre(goldPerChunk, genGold, 80, 128);
        }

        protected override void GenBrol(Rand rand, int xbc, int zbc)
        {
            for (int i = 0; i < brolPerChunk; i++)
            {
                genBrol.Generate(world, rand, new BlockPos(xbc + rand.Next(16), 11, zbc + rand.Next(16)));
            }
        }


    }
}
