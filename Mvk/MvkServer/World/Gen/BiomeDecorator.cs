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
        public WorldGenPlants genPlants;
        public WorldGenCactus genCactus;
        public WorldGenTree genPalm;
        public WorldGenTree genOak;
        public WorldGenTree genBirch;
        public WorldGenTree genSpruce;
        public WorldGenTree genFruit;

        public WorldGenBrol genBrol;

        public WorldGenMinable genIron;
        public WorldGenMinable genCoal;
        public WorldGenMinable genGold;
        public WorldGenMinable genDirt;
        public WorldGenMinable gemGravel;
        public WorldGenMinable genGranite;
        public WorldGenMinable genLimestone;
        public WorldGenMinable genOil;

        public WorldGenPancake genPancakeSand;
        public WorldGenPancake gemPancakeGravel;
        public WorldGenPancake genPancakeClay;
        public WorldGenPancake genPancakeDirt;

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
        /// Количество какутосов, создаваемой на чанке
        /// </summary>
        public int cactiPerChunk = 0;
        /// <summary>
        /// Количество пальм, создаваемой на чанке
        /// </summary>
        public int palmPerChunk = 0;
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
        /// Вероятность генерации дерева при одной на чанк, с вероятностью 1 к randomTree
        /// </summary>
        public int randomTree = 1;

        /// <summary>
        /// Количество известняка, создаваемой на чанке
        /// </summary>
        public int limestonePerChunk = 0;

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

        private readonly WorldServer world;
        private ChunkBase currentChunk;
        private ChunkBase currentChunkSpawn;
        private Rand currentRand;
        private BlockPos currentBlockPos;

        public BiomeDecorator(WorldServer world)
        {
            this.world = world;
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
            genOil = new WorldGenMinable(new BlockState(EnumBlock.Oil), 17);

            genPancakeSand = new WorldGenPancake(EnumBlock.Sand, 9, 8);
            gemPancakeGravel = new WorldGenPancake(EnumBlock.Gravel, 7, 6);
            genPancakeClay = new WorldGenPancake(EnumBlock.Clay, 5, 4);
            genPancakeDirt = new WorldGenPancake(EnumBlock.Dirt, 9, 7);
        }

        private void UpSeed(Rand rand, int xbc, int zbc, int seed)
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
        public void GenDecorationsArea(WorldServer world, ChunkProviderGenerate provider, ChunkBase chunk, ChunkBase chunkSpawn)
        {
            int xbc = chunkSpawn.Position.x << 4;
            int zbc = chunkSpawn.Position.y << 4;
            Rand rand = provider.Random;
            UpSeed(rand, xbc, zbc, provider.Seed);
            int i, x, y, z;

            for (i = 0; i < palmPerChunk; i++)
            {
                x = rand.Next(16);
                z = rand.Next(16);
                y = chunkSpawn.GetHeightGen(x, z);
                genPalm.GenerateArea(world, chunk, rand, new BlockPos(xbc + x, y, zbc + z));
            }

            currentChunk = chunk;
            currentChunkSpawn = chunkSpawn;
            currentRand = rand;
            currentBlockPos = new BlockPos(xbc, 0, zbc);

            GenPancake(dirtPancakePerChunk, genPancakeDirt);
            GenPancake(sandPancakePerChunk, genPancakeSand);
            GenPancake(gravelPancakePerChunk, gemPancakeGravel);
            GenPancake(clayPancakePerChunk, genPancakeClay);

            GenTree(oakPerChunk, genOak);
            GenTree(birchPerChunk, genBirch);
            GenTree(sprucePerChunk, genSpruce);
            GenTree(fruitPerChunk, genFruit);

            GenStandardOre(3, genOil, 24, 64); // Нефть под вопросом!!!
            GenStandardOre(10, genDirt, 1, 256);
            GenStandardOre(8, gemGravel, 1, 256);
            GenStandardOre(10, genGranite, 1, 112); // 80
            GenStandardOre(limestonePerChunk, genLimestone, 1, 112); // 80
            GenStandardOre(20, genCoal, 3, 160); // 128
            GenStandardOre(20, genIron, 3, 96); // 64
            GenStandardOre(1, genGold, 3, 16);
        }

        private void GenStandardOre(int count, WorldGenMinable worldGenMinable, int yMin, int yMax)
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

        private void GenPancake(int count, WorldGenPancake worldGenPancake)
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

        private void GenTree(int count, WorldGenTree worldGenTree)
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

        /// <summary>
        /// Декорация в одном столбце или 1 блок
        /// </summary>
        /// <param name="world"></param>
        /// <param name="provider"></param>
        /// <param name="chunk"></param>
        public void GenDecorations(WorldServer world, ChunkProviderGenerate provider, ChunkBase chunk)
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
            for (i = 0; i < brolPerChunk; i++)
            {
                genBrol.Generate(world, rand, new BlockPos(xbc + rand.Next(16), rand.Next(10) + 3, zbc + rand.Next(16)));
            }

        }
    }
}
