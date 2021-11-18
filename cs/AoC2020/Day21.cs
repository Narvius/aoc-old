using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AoC2020
{
    public class Day21 : ISolution
    {
        // Find the number of safe ingredients used across all recipes.
        public string PartOne(string[] lines)
            => new AllergenMapper(lines).SafeIngredientCount().ToString();

        // Find the ingredients with allergens in them.
        public string PartTwo(string[] lines)
            => new AllergenMapper(lines).CanonicalDangerousIngredientsList().ToString();
    }

    /// <summary>
    /// Determines which ingredients have allergens in them.
    /// </summary>
    public class AllergenMapper
    {
        private readonly List<string[]> recipes;
        private readonly Dictionary<string, string> allergenMapping;

        public AllergenMapper(string[] lines)
        {
            var allergenCandidates = new Dictionary<string, HashSet<string>>();
            recipes = new List<string[]>();
            allergenMapping = new Dictionary<string, string>();

            // Find candidates for each allergen.
            // An ingredient is a candidate for an allergen if it shows up in all recipes that have that allegen.
            foreach (var line in lines)
            {
                var data = line.Split(" (contains");
                recipes.Add(data[0].Trim(' ').Split(' ').ToArray());
                foreach (var allergen in data[1].Trim(')').Replace(" ", "").Split(","))
                    if (allergenCandidates.ContainsKey(allergen))
                        allergenCandidates[allergen].IntersectWith(recipes.Last());
                    else
                        allergenCandidates[allergen] = recipes.Last().ToHashSet();
            }

            // Use the candidate lists to pin down which ingredient has which allergen.
            while (allergenCandidates.Count > 0)
            {
                var (allergen, candidates) = allergenCandidates.First(kvp => kvp.Value.Count(a => !allergenMapping.Values.Contains(a)) == 1);
                allergenMapping[allergen] = candidates.First(c => !allergenMapping.Values.Contains(c));
                allergenCandidates.Remove(allergen);
            }
        }

        /// <summary>
        /// Counts the number of safe ingredients across all recipes.
        /// </summary>
        /// <returns>Total number of safe ingredients used.</returns>
        public int SafeIngredientCount()
            => recipes.Sum(recipe => recipe.Count(ingredient => !allergenMapping.Values.Contains(ingredient)));

        /// <summary>
        /// Constructs the canonical dangerous ingredients list, which lists ingredients with allergens in them in a specific orer.
        /// </summary>
        /// <returns>The canonical dangerous ingredients list.</returns>
        public string CanonicalDangerousIngredientsList()
            => string.Join(",", from entry in allergenMapping
                                orderby entry.Key ascending
                                select entry.Value);
    }
}
