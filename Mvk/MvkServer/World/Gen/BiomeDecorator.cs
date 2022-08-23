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
        public WorldGenPlants genPlants = new WorldGenPlants();
        public WorldGenCactus genCactus;
        public WorldGenPalm genPalm;
        public WorldGenerator ironGen;
        public WorldGenerator coalGen;
        public WorldGenerator goldGen;
        public WorldGenerator brolGen;

        /// <summary>
        /// Количество цветов, создаваемой на чанке
        /// </summary>
        public int flowersPerChunk = 0;
        /// <summary>
        /// Количество высокой травы, создаваемой на чанке
        /// </summary>
        public int grassPerChunk = 1;
        /// <summary>
        /// Количество какутосов, создаваемой на чанке
        /// </summary>
        public int cactiPerChunk = 0;
        /// <summary>
        /// Количество пальм, создаваемой на чанке
        /// </summary>
        public int palmPerChunk = 0;

        private readonly WorldServer world;
        private ChunkBase currentChunk;
        private Rand currentRand;
        private BlockPos currentBlockPos;

        public BiomeDecorator(WorldServer world)
        {
            this.world = world;
            ironGen = new WorldGenMinable(new BlockState(EnumBlock.OreIron), 9);
            coalGen = new WorldGenMinable(new BlockState(EnumBlock.OreCoal), 12);
            goldGen = new WorldGenMinable(new BlockState(EnumBlock.OreGold), 4);
            brolGen = new WorldGenMinable(new BlockState(EnumBlock.Brol), 3);
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
                y = 97 + rand.Next(16);
                genPalm.GenerateArea(world, chunk, rand, new BlockPos(xbc + x, y, zbc + z));
            }
            currentChunk = chunk;
            currentRand = rand;
            currentBlockPos = new BlockPos(xbc, 0, zbc);

            GenStandardOre(50, brolGen, 100, 160);
            GenStandardOre(20, ironGen, 3, 160);
            //GenStandardOre(10, ironGen, 3, 160);
            GenStandardOre(3, goldGen, 3, 160);
            //GenStandardOre(10, ironGen, 3, 160);
            GenStandardOre(10, ironGen, 3, 160);
            GenStandardOre(10, ironGen, 3, 160);
            GenStandardOre(20, coalGen, 3, 160);
            
            //GenStandardOre(10, coalGen, 3, 160);
        }

        private void GenStandardOre(int count, WorldGenerator worldGenerator, int yMin, int yMax)
        {
            int i;

            if (yMax < yMin)
            {
                i = yMin;
                yMin = yMax;
                yMax = i;
            }
            else if (yMax == yMin)
            {
                if (yMin < ChunkBase.COUNT_HEIGHT_BLOCK) yMax++;
                else yMin--;
            }
            for (i = 0; i < count; i++)
            {
                worldGenerator.GenerateArea(world, currentChunk, currentRand,
                    currentBlockPos.Offset(currentRand.Next(16), currentRand.Next(yMax - yMin) + yMin, currentRand.Next(16)));
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
            int i, x, y, z;
            BlockPos blockPos;
            // Цветы
            for (i = 0; i < flowersPerChunk; i++)
            {
                x = rand.Next(16);
                x += rand.Next(8) - rand.Next(8);
                z = rand.Next(16);
                z += rand.Next(8) - rand.Next(8);
                blockPos = new BlockPos(xbc + x, 0, zbc + z);
                blockPos.Y = world.GetChunk(blockPos).Light.GetHeight(x & 15, z & 15);
                genPlants.SetId((ushort)(46 + rand.Next(2)));
                genPlants.Generate(world, rand, blockPos);
            }
            // Трава
            for (i = 0; i < grassPerChunk; i++)
            {
                x = rand.Next(16);
                z = rand.Next(16);
                y = chunk.Light.GetHeight(x, z);
                provider.biomes[0].GetRandomWorldGenForGrass().Generate(world, rand, new BlockPos(xbc + x, y, zbc + z));
            }
            // Кактус
            for (i = 0; i < cactiPerChunk; i++)
            {
                x = rand.Next(16);
                z = rand.Next(16);
                y = chunk.Light.GetHeight(x, z);
                genCactus.Generate(world, rand, new BlockPos(xbc + x, y, zbc + z));
            }
        }
    }
}
