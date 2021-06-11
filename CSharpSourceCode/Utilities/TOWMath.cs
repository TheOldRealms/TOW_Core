using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TOW_Core.Utilities
{
    public static class TOWMath
    {
        private static Random rng = new Random();
        private static readonly object syncLock = new object();

        /// <summary>
        /// Get a random double between min and max, inclusive.
        /// </summary>
        /// <param name="min">The lower bound of the returned double</param>
        /// <param name="max">The upper bound of the returned double</param>
        /// <returns>A random double between min and max, inclusive</returns>
        public static double GetRandomDouble(double min, double max)
        {
            lock (syncLock)
            {
                return (rng.NextDouble() * max) + min;
            }
        }

        public static int GetRandomInt(int min, int max)
        {
            lock (syncLock)
            {
                return rng.Next(min, max);
            }
        }

        public static float GetDegreeInRadians(float degree)
        {
            return degree * (float)Math.PI / 180;
        }
    }
}
