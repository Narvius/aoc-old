using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AoC2020
{
    public class Day04 : ISolution
    {
        // Count the number of passports that have all required fields.
        public string PartOne(string[] lines)
            => Passport.FromBlobsFile(lines).Count(passport => passport.HasRequiredFields).ToString();

        // Count the number of valid passports (have all required feilds and those fields match some validationc criteria).
        public string PartTwo(string[] lines)
            => Passport.FromBlobsFile(lines).Count(passport => passport.HasRequiredFields && passport.HasValidFields).ToString();
    }

    /// <summary>
    /// Provides means of parsing the fields of a passport and validating them.
    /// </summary>
    public class Passport
    {
        /// <summary>
        /// Produces a passport from a blob. A blob is a number of lines of strings that contain passport data.
        /// </summary>
        /// <param name="blob">The passport data.</param>
        public Passport(IEnumerable<string> blob)
        {
            var pattern = @"(\w{3}):(\S+)";

            foreach (var line in blob)
                foreach (Match match in Regex.Matches(line, pattern))
                {
                    var (key, val) = (match.Groups[1].Value, match.Groups[2].Value);
                    switch (key)
                    {
                        case "byr": BirthYear = int.Parse(val); break;
                        case "iyr": IssueYear = int.Parse(val); break;
                        case "eyr": ExpirationYear = int.Parse(val); break;
                        case "hgt": Height = (int.Parse(new string(val.Where(char.IsDigit).ToArray())), new string(val.Where(char.IsLetter).ToArray())); break;
                        case "hcl": HairColour = val; break;
                        case "ecl": EyeColour = val; break;
                        case "pid": PassportId = val; break;
                        case "cid": CountryId = val; break;
                        default: throw new Exception($"invalid key '{key}' in blob");
                    }
                }
        }

        /// <summary>
        /// Given a file with passport blobs separated by empty lines, constructs all passports and returns them.
        /// </summary>
        /// <param name="blobs">One or more passport blobs separated by empty lines.</param>
        /// <returns>An enumeration of all passports that can be constructed from the blobs.</returns>
        public static IEnumerable<Passport> FromBlobsFile(string[] blobs)
            => blobs.ChunkBy(string.IsNullOrEmpty).Select(blob => new Passport(blob));

        public int BirthYear { get; set; }
        public int IssueYear { get; set; }
        public int ExpirationYear { get; set; }
        public string EyeColour { get; set; }
        public string HairColour { get; set; }
        public string PassportId { get; set; }
        public string CountryId { get; set; }
        public (int value, string unit) Height { get; set; }

        /// <summary>
        /// True if all required fields are present. The only field that is not required is <see cref="CountryId"/>.
        /// </summary>
        public bool HasRequiredFields
            => BirthYear > 0 && IssueYear > 0 && ExpirationYear > 0 && Height.value > 0
            && EyeColour != null && HairColour != null && PassportId != null;

        /// <summary>
        /// True if all required fields match their validation logic. This logic is unique per property.
        /// </summary>
        public bool HasValidFields
            => 1920 <= BirthYear && BirthYear <= 2002
            && 2010 <= IssueYear && IssueYear <= 2020
            && 2020 <= ExpirationYear && ExpirationYear <= 2030
            && ((Height.unit == "cm" && 150 <= Height.value && Height.value <= 193) || (Height.unit == "in" && 59 <= Height.value && Height.value <= 76))
            && HairColour[0] == '#' && HairColour.Length == 7 && HairColour.Skip(1).All("0123456789abcdef".Contains)
            && new[] { "amb", "blu", "brn", "gry", "grn", "hzl", "oth" }.Contains(EyeColour)
            && PassportId.Length == 9;
    }
}
