using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AdventOfCode2018
{
    public class Day4 : ISolution
    {
        // Find the guard that slept the most, and which specific minute they slept on the most.
        public string PartOne(string[] lines)
        {
            var mostSleepingGuard = (from timeline in GetShifts(lines)
                                     group timeline by timeline.GuardId into guardData
                                     orderby guardData.Sum(t => t.SleepingMinutes()) descending
                                     select guardData).First();

            return (mostSleepingGuard.Key * GetMostSleptMinute(mostSleepingGuard).minute).ToString();
        }

        // Find the guard with the highest specific single minute slept on, and which minute that is.
        public string PartTwo(string[] lines)
        {
            var mostOverlappingGuard = (from shift in GetShifts(lines)
                                        group shift by shift.GuardId into guardData
                                        let info = GetMostSleptMinute(guardData)
                                        orderby info.amount descending
                                        select (id: guardData.Key, minute: info.minute)).First();

            return (mostOverlappingGuard.id * mostOverlappingGuard.minute).ToString();
        }

        // Given all shifts of one guard, returns the single minute between 12:00am and 0:59am the guard was asleep on the most often.
        private (int minute, int amount) GetMostSleptMinute(IEnumerable<GuardShift> shifts)
        {
            int[] hour = new int[60];
            foreach (var shift in shifts)
            {
                var changes = shift.StatusChanges.ToHashSet();
                var sleeping = false;
                for (int i = 0; i < 60; i++)
                {
                    if (changes.Contains(i))
                        sleeping = !sleeping;
                    hour[i] += sleeping ? 1 : 0;
                }
            }

            var max = hour.Max();
            return (Array.IndexOf(hour, max), max);
        }

        // Returns a list of all recorded shifts, unordered/ungrouped.
        private IEnumerable<GuardShift> GetShifts(string[] rawEntries)
        {
            var orderedEvents = from line in rawEntries
                                orderby line.Substring(0, 18)
                                select line;

            string currentHeader = null;
            var statusChanges = new List<int>();
            foreach (var item in orderedEvents.Concat(new[] { "#" }))
                if (item.Contains('#'))
                {
                    if (currentHeader != null)
                        yield return new GuardShift(ExtractGuardId(currentHeader), statusChanges);
                    statusChanges.Clear();
                    currentHeader = item;
                }
                else
                    statusChanges.Add(ExtractMinutes(item));
        }

        private static readonly Regex GuardParse = new Regex(@"\#(\d+)");
        private static readonly Regex MinuteParse = new Regex(@"(\d{2})\]");

        private int ExtractMinutes(string raw)
            => int.Parse(MinuteParse.Match(raw).Groups[1].Value);

        private int ExtractGuardId(string raw)
            => int.Parse(GuardParse.Match(raw).Groups[1].Value);
    }

    // Represents a single shift by a single guard.
    public class GuardShift
    {
        public int GuardId { get; }
        public IEnumerable<int> StatusChanges { get; }

        // Returns the total number of minutes spent asleep during this shift.
        public int SleepingMinutes()
        {
            int result = 0;
            int? sleepingSince = null;
            foreach (var change in StatusChanges.Concat(new[] { 60 }))
                if (sleepingSince.HasValue)
                {
                    result += (change - sleepingSince.Value);
                    sleepingSince = null;
                }
                else
                    sleepingSince = change;

            return result;
        }

        public GuardShift(int guardId, IEnumerable<int> statusChanges)
        {
            GuardId = guardId;
            StatusChanges = new List<int>(statusChanges);
        }
    }
}
