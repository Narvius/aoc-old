﻿using System;
using System.IO;

namespace AoC2019
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(Solve<Day1>());
            Console.ReadKey();
        }

        static (string, string) Solve<T>()
            where T : ISolution, new()
        {
            var instance = new T();
            string filename = Path.ChangeExtension(typeof(T).Name, ".txt");

            var contents = File.ReadAllLines(filename);

            string a = "", b = "";
            try { a = instance.PartOne(contents); } catch (NotImplementedException) { } catch (Exception ex) { Console.WriteLine(ex); }
            try { b = instance.PartTwo(contents); } catch (NotImplementedException) { } catch (Exception ex) { Console.WriteLine(ex); }

            return (a, b);
        }
    }
}
