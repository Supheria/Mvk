using System;
using System.Diagnostics.Contracts;

namespace MvkServer.Util
{
    //public class Rand : Random
    //{
    //    public Rand() : base() { }
    //    public Rand(int seed) : base(seed) { }

    //    public float NextFloat()
    //    {
    //        return (float)NextDouble();
    //    }
    //}

    //[System.Runtime.InteropServices.ComVisible(true)]
    //[Serializable]
    public class RandSystem
    {
        private const int MBIG = Int32.MaxValue;
        private const int MSEED = 161803398;
        private const int MZ = 0;

        private int inext;
        private int inextp;
        private readonly int[] seedArray = new int[56];

        public RandSystem() : this(Environment.TickCount) { }

        public RandSystem(int seed)
        {
            SetSeed(seed);
        }

        public RandSystem SetSeed(int seed)
        {
            int ii, i;
            int mj, mk;

            //Initialize our Seed array.
            //This algorithm comes from Numerical Recipes in C (2nd Ed.)
            int subtraction = (seed == Int32.MinValue) ? Int32.MaxValue : Math.Abs(seed);
            mj = MSEED - subtraction;
            seedArray[55] = mj;
            mk = 1;
            for (i = 1; i < 55; i++)
            {  //Apparently the range [1..55] is special (Knuth) and so we're wasting the 0'th position.
                ii = (21 * i) % 55;
                seedArray[ii] = mk;
                mk = mj - mk;
                if (mk < 0) mk += MBIG;
                mj = seedArray[ii];
            }
            for (int k = 1; k < 5; k++)
            {
                for (i = 1; i < 56; i++)
                {
                    seedArray[i] -= seedArray[1 + (i + 30) % 55];
                    if (seedArray[i] < 0) seedArray[i] += MBIG;
                }
            }
            inext = 0;
            inextp = 21;
            //seed = 1;

            return this;
        }

        /// <summary>
        /// Return a new random number [0..1) and reSeed the Seed array.
        /// </summary>
        private float Sample() =>
            //Including this division at the end gives us significantly improved
            //random number distribution.
            (InternalSample() * (1.0f / MBIG));

        private int InternalSample()
        {
            int retVal;
            int locINext = inext;
            int locINextp = inextp;

            if (++locINext >= 56) locINext = 1;
            if (++locINextp >= 56) locINextp = 1;

            retVal = seedArray[locINext] - seedArray[locINextp];

            if (retVal == MBIG) retVal--;
            if (retVal < 0) retVal += MBIG;

            seedArray[locINext] = retVal;

            inext = locINext;
            inextp = locINextp;

            return retVal;
        }

        /// <summary>
        /// An int [0..Int32.MaxValue)
        /// </summary>
        public int Next() => InternalSample();

        private double GetSampleForLargeRange()
        {
            // The distribution of double value returned by Sample 
            // is not distributed well enough for a large range.
            // If we use Sample for a range [Int32.MinValue..Int32.MaxValue)
            // We will end up getting even numbers only.

            int result = InternalSample();
            // Note we can't use addition here. The distribution will be bad if we do that.
            bool negative = (InternalSample() % 2 == 0) ? true : false;  // decide the sign based on second sample
            if (negative)
            {
                result = -result;
            }
            double d = result;
            d += (Int32.MaxValue - 1); // get a number in range [0 .. 2 * Int32MaxValue - 1)
            d /= 2 * (uint)Int32.MaxValue - 1;
            return d;
        }

        /// <summary>
        /// An int [minvalue..maxvalue)
        /// </summary>
        public int Next(int minValue, int maxValue)
        {
            if (minValue > maxValue) throw new ArgumentNullException("minValue > maxValue");
            Contract.EndContractBlock();

            long range = (long)maxValue - minValue;
            if (range <= Int32.MaxValue)
            {
                return ((int)(Sample() * range) + minValue);
            }

            return (int)((long)(GetSampleForLargeRange() * range) + minValue);
        }
        /// <summary>
        /// An int [0..maxValue)
        /// </summary>
        public int Next(int maxValue)
        {
            if (maxValue < 0) throw new ArgumentNullException("maxValue < 0");
            Contract.EndContractBlock();

            return (int)(Sample() * maxValue);
        }

        /// <summary>
        /// A double [0..1)
        /// </summary>
        public float NextFloat() => Sample();

        /// <summary>
        /// Fills the byte array with random bytes [0..0x7f].  The entire array is filled.
        /// </summary>
        public void NextBytes(byte[] buffer)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
            Contract.EndContractBlock();

            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte)(InternalSample() % (Byte.MaxValue + 1));
            }
        }
    }
}
