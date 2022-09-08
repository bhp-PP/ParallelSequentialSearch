using System.Collections.Concurrent;
using System.Diagnostics;

namespace ParallelSequentialSearch
{
    internal class Program
    {
        const int N = 1_000_000_000;
        static void Main(string[] args)
        {
            int[] numbers = GetRandomArray(N);

            Console.Write("Enter search value: ");
            int value = -1;
            while (!int.TryParse(Console.ReadLine(), out value))
            {
                Console.WriteLine("ERROR - please re-enter");
            }

            Stopwatch sw = Stopwatch.StartNew();
            int result = SequentialSearch(numbers, value);
            sw.Stop();
            Console.WriteLine(result == -1 ? "Not found" : $"Found at index {result}");
            Console.WriteLine($"Time = {sw.Elapsed.TotalSeconds} seconds");

            sw.Restart();
            result = ParallelSearch(numbers, value);
            sw.Stop();
            Console.WriteLine(result == -1 ? "Not found" : $"Found at index {result}");
            Console.WriteLine($"Time = {sw.Elapsed.TotalSeconds} seconds");
        }

        private static int[] GetRandomArray(int size)
        {
            int[] numbers = new int[size];
            Parallel.ForEach(
                Partitioner.Create(0, size),
                new ParallelOptions(),
                () => { return new Random(); },
                (range, loopState, rnd) =>
                {
                    for (int i = range.Item1; i < range.Item2; i++)
                        numbers[i] = rnd.Next(size / 2);
                    return rnd;
                },
                (_) => { }
                );
            return numbers;
        }

        private static int ParallelSearch(int[] numbers, int value)
        {
            object indexLock = new object();
            int index = -1;
            Parallel.ForEach(Partitioner.Create(0, N), (range, loopState) =>
             {
                 for (int i = range.Item1; i < range.Item2; i++)
                     if (numbers[i] == value)
                     {
                         lock (indexLock)
                         {
                             if (index == -1 || i < index)
                                 index = i;
                         }
                         loopState.Break();
                     }
             });
            return index;
        }

        private static int SequentialSearch(int[] numbers, int value)
        {
            for (int i = 0; i < numbers.Length; i++)
                if (numbers[i] == value)
                    return i;
            return -1;
        }
    }
}