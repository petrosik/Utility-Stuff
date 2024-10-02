namespace Petrosik
{
    namespace Utility
    {
        using System;
        /// <summary>
        /// (deterministic friendly) Random with exposed Seed and number of actions(Pulls) taken.
        /// </summary>
        public class DetRandom
        {
            private readonly Random r;
            public int Pulls { get; private set; }
            public int Seed { get; private set; }
            /// <summary>
            /// Creates new random with the <paramref name="Seed"/>
            /// </summary>
            /// <param name="Seed"></param>
            internal DetRandom(int Seed)
            {
                r = new(Seed);
                Pulls = 0;
                this.Seed = Seed;
            }
            /// <summary>
            /// Creates new random with the <paramref name="Seed"/> and runs the number of <paramref name="Pulls"/>
            /// </summary>
            /// <param name="Seed"></param>
            /// <param name="Pulls"></param>
            internal DetRandom(int Seed, int Pulls)
            {
                r = new(Seed);
                for (int i = 0; i < Pulls; i++)
                {
                    r.Next();
                }
                this.Pulls = Pulls;
                this.Seed = Seed;
            }
            /// <summary>
            /// Creates new random from string with exported information
            /// </summary>
            /// <param name="ExportedString"></param>
            /// <exception cref="Exception"></exception>
            internal DetRandom(string ExportedString)
            {
                try
                {
                    var it = ExportedString.Split(',');
                    Seed = int.Parse(it[0]);
                    Pulls = int.Parse(it[1]);
                }
                catch (Exception ex)
                {
                    throw new Exception($"String \"{ExportedString}\" is not in the correct format for inporting", ex);
                }
                r = new(Seed);
                for (int i = 0; i < Pulls; i++)
                {
                    r.Next();
                }
            }
            /// <summary>
            /// Creates new random
            /// </summary>
            internal DetRandom()
            {
                r = new();
                Seed = r.Next(int.MinValue, int.MaxValue);
                r = new(Seed);
                Pulls = 0;
            }
            public int Next(int minValue, int maxValue)
            {
                Pulls++;
                return r.Next(minValue, maxValue);
            }
            public int Next(int maxValue)
            {
                Pulls++;
                return r.Next(maxValue);
            }
            public int Next()
            {
                Pulls++;
                return r.Next();
            }
            public double NextDouble()
            {
                Pulls++;
                return r.NextDouble();
            }
            public float NextSingle()
            {
                Pulls++;
                return r.NextSingle();
            }
            public long NextInt64()
            {
                Pulls++;
                return r.NextInt64();
            }
            public long NextInt64(int minValue, int maxValue)
            {
                Pulls++;
                return r.NextInt64(minValue, maxValue);
            }
            public long NextInt64(int maxValue)
            {
                Pulls++;
                return r.NextInt64(maxValue);
            }
            public void NextBytes(byte[] buffer)
            {
                Pulls += buffer.Length;
                r.NextBytes(buffer);
            }
            /// <summary>
            /// Exports string that can be inported
            /// </summary>
            /// <returns></returns>
            public string Export()
            {
                return $"{Seed},{Pulls}";
            }
            public override string ToString()
            {
                return $"Seed: {Seed}, Pulls: {Pulls}x";
            }
        }
    }
}
