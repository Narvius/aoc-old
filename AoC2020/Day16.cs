using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AoC2020
{
    public class Day16 : ISolution
    {
        // Searching through all tickets, find the fields that are guaranteed to be invalid. Sum them all.
        public string PartOne(string[] lines)
        {
            var chunks = lines.ChunkBy(string.IsNullOrEmpty, true).Select(chunk => chunk.ToArray()).ToArray();
            var fields = chunks[0].Select(line => new TicketField(line)).ToArray();

            return (from ticket in chunks[2].Skip(1)
                    let numbers = ticket.Split(',').Select(int.Parse)
                    from number in numbers
                    where fields.All(f => !f.Matches(number))
                    select number).Sum().ToString();
        }

        // Find which fields are which, then compute a checksum based on specific fields of your own ticket.
        public string PartTwo(string[] lines)
        {
            // Parse all relevant input data.
            var chunks = lines.ChunkBy(string.IsNullOrEmpty, true).Select(chunk => chunk.ToArray()).ToArray();
            
            var fields = chunks[0].Select(line => new TicketField(line)).ToList();
            var ownTicket = chunks[1][1].Split(',').Select(long.Parse).ToArray();
            var validTickets = (from ticket in chunks[2].Skip(1)
                                let numbers = ticket.Split(',').Select(int.Parse).ToArray()
                                where numbers.All(n => fields.Any(f => f.Matches(n)))
                                select numbers).ToArray();

            // For each position, find all possible fields it might be.
            var candidates = Enumerable.Range(0, fields.Count)
                .Select(i => fields.Where(f => validTickets.All(t => f.Matches(t[i]))).ToList())
                .ToArray();

            // Find candidate lists that only have 1 candidate--that means that position definitely has
            // to be that TicketField.  Remove the TicketField that was unique from all other candidate lists,
            // since it now assigned. Keep repeating this until all fields are taken care of.
            var orderedFields = new TicketField[fields.Count];
            for (int i = 0; i < fields.Count; i++)
                if (candidates[i].Count == 1)
                {
                    orderedFields[i] = candidates[i][0];
                    for (int n = 0; n < fields.Count; n++)
                        candidates[n].Remove(orderedFields[i]);
                    i = -1;
                }

            // Multiply together all "departure" fields from my own ticket together.
            return orderedFields
                .Select((f, i) => f.Name.StartsWith("departure") ? ownTicket[i] : 1)
                .Aggregate((a, b) => a * b)
                .ToString();
        }
    }

    /// <summary>
    /// Wraps logic related to parsing and matching a ticket field.
    /// </summary>
    public class TicketField
    {
        public readonly string Name;
        private readonly (int low, int high) firstRange;
        private readonly (int low, int high) secondRange;

        public TicketField(string line)
        {
            var match = Regex.Match(line, @"([^:]+): (\d+)-(\d+) or (\d+)-(\d+)");
            Name = match.Groups[1].Value;
            firstRange = (int.Parse(match.Groups[2].Value), int.Parse(match.Groups[3].Value));
            secondRange = (int.Parse(match.Groups[4].Value), int.Parse(match.Groups[5].Value));
        }

        /// <summary>
        /// Checks if the provided value could be this field.
        /// </summary>
        /// <returns>True if the provided value could be this field, false otherwise.</returns>
        public bool Matches(int value)
            => (firstRange.low <= value && value <= firstRange.high)
            || (secondRange.low <= value && value <= secondRange.high);
    }
}
