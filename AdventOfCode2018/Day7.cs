using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdventOfCode2018
{
    public class Day7 : ISolution
    {
        // Work out the dependency graph/order for the steps.
        public string PartOne(string[] lines)
            => new WorkerPool(1, 0).Assemble(BuildPrerequisites(lines)).result;

        // Work out the total duration it takes to work off all steps given a duration for each, and an amount of workers.
        public string PartTwo(string[] lines)
            => new WorkerPool(5, 60).Assemble(BuildPrerequisites(lines)).duration.ToString();

        // Builds a map of (step -> required steps). 
        private Dictionary<char, List<char>> BuildPrerequisites(string[] lines)
        {
            var prereqs = from line in lines
                          let data = line.Split(' ')
                          let item = (prereq: data[1][0], step: data[7][0])
                          group item by item.step into groupedPrereqs
                          select new KeyValuePair<char, List<char>>(groupedPrereqs.Key, groupedPrereqs.Select(x => x.prereq).ToList());

            var result = new Dictionary<char, List<char>>(prereqs);

            foreach (var letter in "ABCDEFGHIJKLMNOPQRSTUVWXYZ")
                if (!result.ContainsKey(letter))
                    result.Add(letter, new List<char>());

            return result;
        }
    }

    // Manages the flow of assigning and marking-as-done of steps.
    public class WorkerPool
    {
        public int WorkerCount { get; }
        public int TimeConstant { get; }

        public WorkerPool(int workerCount, int timeConstant)
        {
            WorkerCount = workerCount;
            TimeConstant = timeConstant;
        }

        // Returns how long it took to complete all tasks, and the order in which they were finished.
        public (int duration, string result) Assemble(Dictionary<char, List<char>> prereqs)
        {
            var workers = new List<Worker>();
            for (int i = 0; i < WorkerCount; i++)
                workers.Add(new Worker());

            AssignWork(workers, prereqs);

            string result = "";
            int time = 0;
            while (workers.Any(w => w.Working))
            {
                result += new string(Step(workers, prereqs).ToArray());
                time++;
            }

            return (time, result);
        }

        // Performs a single step of work.
        private IEnumerable<char> Step(List<Worker> workers, Dictionary<char, List<char>> prereqs)
        {
            // Note that worker.Work() returns the step itself if the work is finished.
            foreach (var worker in workers)
            {
                var result = worker.Work();
                if (result != null)
                {
                    MarkComplete(prereqs, result.Value);
                    yield return result.Value;
                }
            }

            AssignWork(workers, prereqs);
        }

        // Assigns work to all workers without jobs, provided there are available steps.
        private void AssignWork(List<Worker> workers, Dictionary<char, List<char>> prereqs)
        {
            if (workers.All(w => w.Working))
                return;

            if (!TryGetAvailable(prereqs, workers, out var steps))
                return;

            int i = 0;
            foreach (var worker in workers)
                if (!worker.Working && steps.Length > i)
                {
                    worker.Assign(steps[i], Duration(steps[i]));
                    i++;
                }
        }

        // Returns a list of available steps.
        // A step is available if there are no remaining prerequisites, and no worker is working on it.
        private bool TryGetAvailable(Dictionary<char, List<char>> prereqs, List<Worker> workers, out string steps)
        {
            steps = null;

            if (prereqs.Count == 0)
                return false;

            steps = new string((from pair in prereqs
                                where pair.Value.Count == 0 && !workers.Any(w => w.WorksOn(pair.Key))
                                orderby pair.Key ascending
                                select pair.Key).ToArray());

            return steps.Length > 0;
        }

        // Marks a step as complete, by removing it completely from the prerequisites map.
        private void MarkComplete(Dictionary<char, List<char>> prereqs, char step)
        {
            foreach (var prereq in prereqs)
                prereq.Value.Remove(step);
            prereqs.Remove(step);
        }

        // The duration of a given task.
        private int Duration(char c) => TimeConstant + 1 + (c - 'A');
    }

    // Encapsulates behaviour for a single worker.
    public class Worker
    {
        private char currentStep;
        private int remaining;

        // Returns whether the worker is working at all.
        public bool Working => remaining > 0;

        // Reduces remaining time by one step, and returns the step itself when it's finished being worked on.
        public char? Work()
        {
            if (remaining == 0)
                return null;

            if (--remaining == 0)
                return currentStep;

            return null;
        }

        // Checks whether this worker is working on the given step.
        public bool WorksOn(char step) => Working && currentStep == step;

        // Assigns some work to the worker.
        public void Assign(char step, int duration)
        {
            currentStep = step;
            remaining = duration;
        }
    }
}
