using System;
using System.Collections.Generic;
using System.Linq;
using Module.Shared;

namespace Module.Shared {

    public static class RandomExtensions {

        public static double NextDouble(this Random random, double lowerBound, double upperBound) {
            double resolutionFactor = 2; // To avoid problems with max and min double values
            upperBound = upperBound / resolutionFactor;
            lowerBound = lowerBound / resolutionFactor;
            double f = (upperBound - lowerBound) * random.NextDouble() + lowerBound;
            return f * resolutionFactor;
        }

        public static float NextFloat(this Random random, float lowerBound = 0, float upperBound = 1) {
            return (float)(NextDouble(random, lowerBound, upperBound));
        }

        public static bool NextBool(this Random random) {
            // Source: https://stackoverflow.com/a/19191165/165106
            return random.NextDouble() > 0.5;
        }

        /// <summary> Will efficiently return a random entry of a source enumerable with a 
        /// uniform probability, see also https://stackoverflow.com/a/648240/165106 </summary>
        public static T NextRndChild<T>(this Random self, IEnumerable<T> source) {
            T current = default(T);
            int count = 0;
            foreach (T element in source) {
                count++;
                if (self.Next(count) == 0) { current = element; }
            }
            if (count == 0) { throw new InvalidOperationException("Sequence was empty"); }
            return current;
        }

        public static void ShuffleList<T>(this Random self, IList<T> listToShuffle) {
            int n = listToShuffle.Count;
            while (n > 1) {
                n--;
                int k = self.Next(n + 1);
                T value = listToShuffle[k];
                listToShuffle[k] = listToShuffle[n];
                listToShuffle[n] = value;
            }
        }

        public static IEnumerable<T> SampleElemsToGetRandomSubset<T>(this Random self, IEnumerable<T> elements, int subsetSize) {
            return self.ShuffleEntries(elements).Take(subsetSize);
        }

        public static IEnumerable<T> ShuffleEntries<T>(this Random self, IEnumerable<T> elementsToShuffle) {
            return ShuffleEntriesEnumerator(self, elementsToShuffle).Cached();
        }

        private static IEnumerable<T> ShuffleEntriesEnumerator<T>(Random self, IEnumerable<T> elementsToShuffle) {
            // From https://stackoverflow.com/a/1653204/165106
            var buffer = elementsToShuffle.ToList();
            for (int i = 0; i < buffer.Count; i++) {
                int j = self.Next(i, buffer.Count);
                yield return buffer[j];
                buffer[j] = buffer[i];
            }
        }

        public static double NextGaussian(this Random random, double mean = 0, double standardDeviation = 1) {
            // generate random numbers with a uniform distribution
            double u1 = 1.0 - random.NextDouble();
            double u2 = 1.0 - random.NextDouble();
            // use the Box-Muller transform to convert the uniform distribution to a Gaussian distribution
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            // scale and shift the distribution to the desired mean and standard deviation
            return mean + standardDeviation * randStdNormal;
        }
        
    }

}

namespace Module.Shared {

    /// <summary> Helps with generating random data for testing </summary>
    public static class RandomNameGenerator {

        // v for vowels 1 for the first set of consonants and 2 for the second set of consonants:
        public static List<string> generatorInstructions = new List<string>() { "v2", "v2v", "1v2", "v2v2", "1v2v2" };
        public static List<string> vowels = new List<string>() { "a", "e", "i", "o", "u", "ei", "ai", "ou", "j", "ji", "y", "oi", "au", "oo" };
        public static List<string> consonants1 = new List<string>() { "b", "c", "d", "f", "g", "h", "k", "l", "m", "n", "p", "q", "r", "s", "t", "v", "w", "x", "z", "ch", "bl", "br", "fl", "gl", "gr", "kl", "pr", "st", "sh", "th" };
        public static List<string> consonants2 = new List<string>() { "b", "d", "f", "g", "h", "k", "l", "m", "n", "p", "r", "s", "t", "v", "w", "z", "ch", "gh", "nn", "st", "sh", "th", "tt", "ss", "pf", "nt" };

        public static string NextRandomName(this Random self) {
            string instructionSet = self.NextRandomListEntry(generatorInstructions);
            return NextRandomName(self, instructionSet, vowels, consonants1, consonants2);
        }

        public static string NextRandomName(this Random self, string genertorInstruction, List<string> vowels, List<string> consonants1, List<string> consonants2) {
            string generatedName = "";
            int length = genertorInstruction.Length;
            for (int i = 0; i < length; i++) {
                char c = genertorInstruction[0];
                switch (c) {
                    case 'v': generatedName += self.NextRandomListEntry(vowels); break;
                    case '1': generatedName += self.NextRandomListEntry(consonants1); break;
                    case '2': generatedName += self.NextRandomListEntry(consonants2); break;
                }
                genertorInstruction = genertorInstruction.Substring(1);
            }
            return generatedName.ToFirstCharUpperCase();
        }

        private static T NextRandomListEntry<T>(this Random self, List<T> list) { return list[self.Next(0, list.Count - 1)]; }

    }

}