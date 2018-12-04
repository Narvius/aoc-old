using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AdventOfCode2018
{
    public class Day4 : ISolution
    {
        public string PartOne(string[] lines)
        {
            var mostSleepingGuard = (from timeline in GetTimelines(lines)
                                     group timeline by timeline.GuardId into guardData
                                     orderby guardData.Sum(t => t.SleepingMinutes()) descending
                                     select guardData).First();

            return (mostSleepingGuard.Key * GetMostSleptMinute(mostSleepingGuard).minute).ToString();
        }

        public string PartTwo(string[] lines)
        {
            var mostOverlappingGuard = (from timeline in GetTimelines(lines)
                                        group timeline by timeline.GuardId into guardData
                                        let info = GetMostSleptMinute(guardData)
                                        orderby info.amount descending
                                        select (id: guardData.Key, minute: info.minute)).First();

            return (mostOverlappingGuard.id * mostOverlappingGuard.minute).ToString();
        }

        private (int minute, int amount) GetMostSleptMinute(IEnumerable<GuardTimeline> timelines)
        {
            int[] hour = new int[60];
            foreach (var timeline in timelines)
            {
                var changes = timeline.StatusChanges.ToHashSet();
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

        private IEnumerable<GuardTimeline> GetTimelines(string[] rawEntries)
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
                        yield return new GuardTimeline(ExtractGuardId(currentHeader), statusChanges);
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

    public class GuardTimeline
    {
        public int GuardId { get; }
        public IEnumerable<int> StatusChanges { get; }

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

        public GuardTimeline(int guardId, IEnumerable<int> statusChanges)
        {
            GuardId = guardId;
            StatusChanges = new List<int>(statusChanges);
        }
    }
}
