using MvkServer.Util;
using MvkServer.World.Block;
using MvkServer.World.Chunk;
using MvkServer.World.Gen.Feature;

namespace MvkServer.World.Gen
{
    /// <summary>
    /// Докорация в биомах
    /// </summary>
    public class BiomeDecorator
    {
        protected WorldGenPiece genPiece;
        protected WorldGenPlants genPlants;
        protected WorldGenTina genTina;
        protected WorldGenCactus genCactus;
        protected WorldGenTree genPalm;
        protected WorldGenTree genOak;
        protected WorldGenTree genBirch;
        protected WorldGenTree genSpruce;
        protected WorldGenTree genFruit;

        protected WorldGenBrol genBrol;

        protected WorldGenMinable genIron;
        protected WorldGenMinable genCoal;
        protected WorldGenMinable genGold;
        protected WorldGenMinable genDirt;
        protected WorldGenMinable gemGravel;
        protected WorldGenMinable genGranite;
        protected WorldGenMinable genLimestone;
        protected WorldGenMinable genOil;

        protected WorldGenPancake genPancakeSand;
        protected WorldGenPancake gemPancakeGravel;
        protected WorldGenPancake genPancakeClay;
        protected WorldGenPancake genPancakeDirt;
        protected WorldGenThicketsGrass genThicketsGrass;

        /// <summary>
        /// Количество брола, создаваемой на чанке
        /// </summary>
        public int brolPerChunk = 0;

        /// <summary>
        /// Количество цветов, создаваемой на чанке
        /// </summary>
        public int flowersPerChunk = 0;
        /// <summary>
        /// Количество высокой травы, создаваемой на чанке
        /// </summary>
        public int grassPerChunk = 0;
        /// <summary>
        /// Количество тины, создаваемой на чанке
        /// </summary>
        public int tinaPerChunk = 0;
        /// <summary>
        /// Количество маленьких блоков, создаваемой на чанке
        /// </summary>
        public int piecePerChunk = 0;
        /// <summary>
        /// Количество какутосов, создаваемой на чанке
        /// </summary>
        public int cactiPerChunk = 0;
        /// <summary>
        /// Количество дубов, создаваемой на чанке
        /// </summary>
        public int oakPerChunk = 0;
        /// <summary>
        /// Количество берёз, создаваемой на чанке
        /// </summary>
        public int birchPerChunk = 0;
        /// <summary>
        /// Количество елей, создаваемой на чанке
        /// </summary>
        public int sprucePerChunk = 0;
        /// <summary>
        /// Количество фруктовых, создаваемой на чанке
        /// </summary>
        public int fruitPerChunk = 0;
        /// <summary>
        /// Количество блинчиков травы, создаваемой на чанке
        /// </summary>
        public int thicketsGrassPerChunk = 0;
        /// <summary>
        /// Вероятность генерации дерева при одной на чанк, с вероятностью 1 к randomTree
        /// </summary>
        public int randomTree = 1;
        /// <summary>
        /// Вероятность генерации маленьких блоков при одной на чанк, с вероятностью 1 к randomTree
        /// </summary>
        public int randomPiece = 1;
        /// <summary>
        /// Вероятность генерации блинчиков травы на чанк, с вероятностью 1 к randomTree
        /// </summary>
        public int randomThicketsGrass = 1;
        /// <summary>
        /// Вероятность генерации пальм на чанк, с вероятностью 1 к randomTree
        /// </summary>
        public int randomPalm = 0;

        /// <summary>
        /// Количество нефти, создаваемой на чанке
        /// </summary>
        public int oilPerChunk = 3;
        /// <summary>
        /// Количество земли, создаваемой на чанке
        /// </summary>
        public int dirtPerChunk = 10;
        /// <summary>
        /// Количество гравия, создаваемой на чанке
        /// </summary>
        public int gravelPerChunk = 8;
        /// <summary>
        /// Количество гранита, создаваемой на чанке
        /// </summary>
        public int granitePerChunk = 10;
        /// <summary>
        /// Количество известняка, создаваемой на чанке
        /// </summary>
        public int limestonePerChunk = 0;
        /// <summary>
        /// Количество угля, создаваемой на чанке
        /// </summary>
        public int coalPerChunk = 20;
        /// <summary>
        /// Количество железа, создаваемой на чанке
        /// </summary>
        public int ironPerChunk = 20;
        /// <summary>
        /// Количество золота, создаваемой на чанке
        /// </summary>
        public int goldPerChunk = 1;

        #region PancakePerChunk

        /// <summary>
        /// Количество блинчика песка
        /// </summary>
        public int sandPancakePerChunk = 0;
        /// <summary>
        /// Количество блинчика гравия
        /// </summary>
        public int gravelPancakePerChunk = 0;
        /// <summary>
        /// Количество блинчика глинв
        /// </summary>
        public int clayPancakePerChunk = 0;
        /// <summary>
        /// Количество блинчика земли
        /// </summary>
        public int dirtPancakePerChunk = 0;

        #endregion

        protected readonly WorldServer world;
        protected ChunkBase currentChunk;
        protected ChunkBase currentChunkSpawn;
        protected Rand currentRand;
        protected BlockPos currentBlockPos;

        public BiomeDecorator() { }

        public BiomeDecorator(WorldServer world) => this.world = world;

        protected void InitBase()
        {
            genPiece = new WorldGenPiece();
            genTina = new WorldGenTina();
            genPlants = new WorldGenPlants();
            genCactus = new WorldGenCactus();
            genOak = new WorldGenTreeOak();
            genBirch = new WorldGenTreeBirch();
            genSpruce = new WorldGenTreeSpruce();
            genFruit = new WorldGenTreeFruit();
            genPalm = new WorldGenTreePalm();

            genPancakeSand = new WorldGenPancake(EnumBlock.Sand, 9, 8);
            gemPancakeGravel = new WorldGenPancake(EnumBlock.Gravel, 7, 6);
            genPancakeClay = new WorldGenPancake(EnumBlock.Clay, 5, 4);
            genPancakeDirt = new WorldGenPancake(EnumBlock.Dirt, 9, 7);

            genThicketsGrass = new WorldGenThicketsGrass();

            genBrol = new WorldGenBrol();
        }

        public virtual void Init()
        {
            InitBase();   
            
            genCoal = new WorldGenMinable(new BlockState(EnumBlock.OreCoal), 17);
            genIron = new WorldGenMinable(new BlockState(EnumBlock.OreIron), 9);
            genGold = new WorldGenMinable(new BlockState(EnumBlock.OreGold), 7); // 9
            genDirt = new WorldGenMinable(new BlockState(EnumBlock.Dirt), 33);
            gemGravel = new WorldGenMinable(new BlockState(EnumBlock.Gravel), 33);
            genGranite = new WorldGenMinable(new BlockState(EnumBlock.Granite), 33);
            genLimestone = new WorldGenMinable(new BlockState(EnumBlock.Limestone), 33);
            genOil = new WorldGenMinable(new BlockState(EnumBlock.Oil), 17);
        }

        protected void UpSeed(Rand rand, int xbc, int zbc, long seed)
        {
            rand.SetSeed(seed);
            int realX = xbc * rand.Next();
            int realZ = zbc * rand.Next();
            rand.SetSeed(realX ^ realZ ^ seed);
        }

        /// <summary>
        /// Декорация областей которые могу выйти за 1 чанк
        /// </summary>
        /// <param name="world"></param>
        /// <param name="provider"></param>
        /// <param name="chunk">которы сетим</param>
        /// <param name="chunkOffset">где спавн</param>
        public void GenDecorationsArea(WorldServer world, ChunkProviderGenerateBase provider, ChunkBase chunk, ChunkBase chunkSpawn)
        {
            int xbc = chunkSpawn.Position.x << 4;
            int zbc = chunkSpawn.Position.y << 4;
            Rand rand = provider.Random;
            UpSeed(rand, xbc, zbc, provider.Seed);
            int i, x, y, z;

            currentChunk = chunk;
            currentChunkSpawn = chunkSpawn;
            currentRand = rand;
            currentBlockPos = new BlockPos(xbc, 0, zbc);

            if (randomPalm > 1 && currentRand.Next(randomPalm) == 0)
            {
                x = rand.Next(16);
                z = rand.Next(16);
                y = chunkSpawn.GetHeightGen(x, z);
                genPalm.GenerateArea(world, chunk, rand, new BlockPos(xbc + x, y, zbc + z));
            }

            GenPancake(dirtPancakePerChunk, genPancakeDirt);
            GenPancake(sandPancakePerChunk, genPancakeSand);
            GenPancake(gravelPancakePerChunk, gemPancakeGravel);
            GenPancake(clayPancakePerChunk, genPancakeClay);

            GenTree(oakPerChunk, genOak);
            GenTree(birchPerChunk, genBirch);
            GenTree(sprucePerChunk, genSpruce);
            GenTree(fruitPerChunk, genFruit);

            GenStandardOres();

            if (thicketsGrassPerChunk > 0 || (randomThicketsGrass > 1 && currentRand.Next(randomThicketsGrass) == 0))
            {
                int count = thicketsGrassPerChunk > 0 ? thicketsGrassPerChunk : 1;
                for (i = 0; i < count; i++)
                {
                    x = currentRand.Next(16);
                    z = currentRand.Next(16);
                    y = currentChunkSpawn.GetHeightGen(x, z);
                    genThicketsGrass.GenerateArea(world, currentChunk, currentRand, currentBlockPos.Offset(x, y, z));
                }
            }
        }

        protected virtual void GenStandardOres()
        {
            GenStandardOre(oilPerChunk, genOil, 24, 32); // Нефть под вопросом!!!
            GenStandardOre(dirtPerChunk, genDirt, 1, 112);
            GenStandardOre(gravelPerChunk, gemGravel, 1, 112);
            GenStandardOre(granitePerChunk, genGranite, 1, 112); // 80
            GenStandardOre(limestonePerChunk, genLimestone, 1, 112); // 80
            GenStandardOre(coalPerChunk, genCoal, 3, 96); // 128
            GenStandardOre(ironPerChunk, genIron, 3, 48); // 64
            GenStandardOre(goldPerChunk, genGold, 3, 16);
        }

        protected void GenStandardOre(int count, WorldGenMinable worldGenMinable, int yMin, int yMax)
        {
            if (count > 0)
            {
                int x, y, z, i;
                for (i = 0; i < count; i++)
                {
                    x = currentRand.Next(16);
                    z = currentRand.Next(16);
                    y = currentRand.Next(yMax - yMin) + yMin;
                    if (y < currentChunkSpawn.GetHeightGen(x, z))
                    {
                        worldGenMinable.GenerateArea(world, currentChunk, currentRand,
                            currentBlockPos.Offset(x, y, z));
                    }
                }
            }
        }

        protected void GenPancake(int count, WorldGenPancake worldGenPancake)
        {
            if (count > 0)
            {
                int x, y, z, i;
                for (i = 0; i < count; i++)
                {
                    x = currentRand.Next(16);
                    z = currentRand.Next(16);
                    y = currentChunkSpawn.GetHeightGen(x, z) - 1;
                    worldGenPancake.GenerateArea(world, currentChunk, currentRand, currentBlockPos.Offset(x, y, z));
                }
            }
        }

        protected void GenTree(int count, WorldGenTree worldGenTree)
        {
            if (count > 0)
            {
                if (count == 1 && randomTree > 1 && currentRand.Next(randomTree) != 0)
                {
                    return;
                }
                int x, y, z, i;
                for (i = 0; i < count; i++)
                {
                    x = currentRand.Next(16);
                    z = currentRand.Next(16);
                    y = currentChunkSpawn.GetHeightGen(x, z);
                    worldGenTree.GenerateArea(world, currentChunk, currentRand, currentBlockPos.Offset(x, y, z));
                }
            }
        }

        protected virtual void GenBrol(Rand rand, int xbc, int zbc)
        {
            for (int i = 0; i < brolPerChunk; i++)
            {
                genBrol.Generate(world, rand, new BlockPos(xbc + rand.Next(16), rand.Next(10) + 3, zbc + rand.Next(16)));
            }
        }

        /// <summary>
        /// Декорация в одном столбце или 1 блок
        /// </summary>
        /// <param name="world"></param>
        /// <param name="provider"></param>
        /// <param name="chunk"></param>
        public void GenDecorations(WorldServer world, ChunkProviderGenerateBase provider, ChunkBase chunk)
        {
            int xbc = chunk.Position.x << 4;
            int zbc = chunk.Position.y << 4;
            Rand rand = provider.Random;
            UpSeed(rand, xbc, zbc, provider.Seed);
            int i, x, z;
            BlockPos blockPos;
            // Цветы
            for (i = 0; i < flowersPerChunk; i++)
            {
                x = rand.Next(16);
                z = rand.Next(16);
                x += rand.Next(8) - rand.Next(8);
                z += rand.Next(8) - rand.Next(8);
                blockPos = new BlockPos(xbc + x, 0, zbc + z);
                blockPos.Y = world.GetChunk(blockPos).GetHeightGen(x & 15, z & 15);
                genPlants.SetId((ushort)(46 + rand.Next(2)));
                genPlants.Generate(world, rand, blockPos);
            }
            // Трава
            for (i = 0; i < grassPerChunk; i++)
            {
                x = rand.Next(16);
                z = rand.Next(16);
                x += rand.Next(8) - rand.Next(8);
                z += rand.Next(8) - rand.Next(8);
                blockPos = new BlockPos(xbc + x, 0, zbc + z);
                blockPos.Y = world.GetChunk(blockPos).GetHeightGen(x & 15, z & 15);
                genPlants.SetId(45);
                genPlants.Generate(world, rand, blockPos);
            }
            // Кактус
            for (i = 0; i < cactiPerChunk; i++)
            {
                x = rand.Next(16);
                z = rand.Next(16);
                x += rand.Next(8) - rand.Next(8);
                z += rand.Next(8) - rand.Next(8);
                blockPos = new BlockPos(xbc + x, 0, zbc + z);
                blockPos.Y = world.GetChunk(blockPos).GetHeightGen(x & 15, z & 15);
                genCactus.Generate(world, rand, blockPos);
            }
            // Брол
            GenBrol(rand, xbc, zbc);
            // Маленькие камни
            if (piecePerChunk > 0 || (randomPiece > 1 && currentRand.Next(randomPiece) == 0))
            {
                int count = piecePerChunk > 0 ? piecePerChunk : 1; 
                for (i = 0; i < count; i++)
                {
                    x = rand.Next(16);
                    z = rand.Next(16);
                    x += rand.Next(8) - rand.Next(8);
                    z += rand.Next(8) - rand.Next(8);
                    blockPos = new BlockPos(xbc + x, 0, zbc + z);
                    blockPos.Y = world.GetChunk(blockPos).GetHeightGen(x & 15, z & 15);
                    genPiece.SetId(70); //EnumBiome.SmallStone
                    genPiece.Generate(world, rand, blockPos);
                }
            }
            // Тина
            for (i = 0; i < tinaPerChunk; i++)
            {
                x = rand.Next(16);
                z = rand.Next(16);
                x += rand.Next(8) - rand.Next(8);
                z += rand.Next(8) - rand.Next(8);
                blockPos = new BlockPos(xbc + x, 0, zbc + z);
                blockPos.Y = world.GetChunk(blockPos).GetHeightGen(x & 15, z & 15);
                genTina.Generate(world, rand, blockPos);
            }
        }
    }
}
