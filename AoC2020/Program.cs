using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AoC2020
{
    class Program
    {
        static void Main(string[] _)
        {
            var solutions = SelectProblem().ToList();
            if (solutions.Count == 0)
                Console.WriteLine("no solutions found matching selection");
            else
            {
                var start = DateTime.UtcNow;
                foreach (var (s, d, p) in solutions)
                    Console.WriteLine($"{p}{Solve(s, d)}");
                Console.WriteLine($"...computed in {(DateTime.UtcNow - start).TotalSeconds:0.00s}.");
            }
            Console.ReadKey();
        }

        static (string, string) Solve(ISolution solution, string dataFile)
        {
            if (solution == null)
                return (null, null);

            var contents = File.ReadAllLines(Path.Combine("Data", dataFile));

            string a = "", b = "";
            try { a = solution.PartOne(contents); } catch (NotImplementedException) { } catch (Exception ex) { Console.WriteLine(ex); }
            try { b = solution.PartTwo(contents); } catch (NotImplementedException) { } catch (Exception ex) { Console.WriteLine(ex); }

            return (a, b);
        }

        static IEnumerable<(ISolution solution, string dataFile, string printPrefix)> SelectProblem()
        {
            static string Show(int i) => $"({(char)('a' + i)}) Day {i + 1,-2}";

            var solutions = GatherSolutions();
            Console.WriteLine(">> Select a solution to run:");

            for (int i = 0; i < 25; i++)
            {
                Console.ForegroundColor = solutions[i] == null ? ConsoleColor.DarkGray : ConsoleColor.White;
                Console.Write(Show(i));
                if ((i + 1) % 10 == 0)
                    Console.WriteLine();
                else
                    Console.Write("  ");
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("(*) all     (any other key) latest");
            Console.ForegroundColor = ConsoleColor.Gray;

            char c = Console.ReadKey(true).KeyChar;

            if (c == '*')
            {
                Console.WriteLine($">> Selected all");
                Console.WriteLine();

                for (int i = 0; i < 25; i++)
                    if (solutions[i] != null)
                        yield return (solutions[i], $"Day{i + 1:00}.txt", $"Day {i + 1,-2} => ");

            }
            else if (!char.IsLower(c))
            {
                var solution = solutions.Last(s => s != null);
                var index = Array.IndexOf(solutions, solution);

                Console.WriteLine($">> Selected latest (day {index + 1})");
                Console.WriteLine();

                yield return (solution, $"Day{index + 1:00}.txt", "");
            }
            else
            {
                Console.WriteLine($">> Selected Day {c - 'a' + 1}");
                Console.WriteLine();

                var solution = solutions[c - 'a'];
                if (solution != null)
                    yield return (solution, $"Day{c - 'a' + 1:00}.txt", "");
            }
        }

        static ISolution[] GatherSolutions()
        {
            var types = typeof(Program).Assembly.GetTypes();
            var result = new ISolution[25];
            for (int i = 0; i < 25; i++)
                result[i] = types.FirstOrDefault(s => s.Name.EndsWith($"Day{i + 1:00}"))?.GetConstructor(Type.EmptyTypes).Invoke(null) as ISolution;
            return result;
        }
    }
}
