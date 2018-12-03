using System;
using System.IO;

namespace AdventOfCode2018
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(Solve<Day2>());
            Console.ReadKey();
        }

        static (string, string) Solve<T>()
            where T : ISolution, new()
        {
            var instance = new T();
            string filename = Path.ChangeExtension(typeof(T).Name, ".txt");

            var contents = File.ReadAllLines(filename);

            return (instance.PartOne(contents), instance.PartTwo(contents));
        }
    }
}
