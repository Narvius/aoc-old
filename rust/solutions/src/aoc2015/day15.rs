use regex::Regex;

/// Find the best cookie recipe with exactly 100 units of ingredients.
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    // Uses a greedy algorithm that just at every step takes 1 unit of the ingredient that would
    // locally result in the highest score. We're assuming we're starting with at least 1 unit of
    // each ingredient, so that all subscores start out positive--otherwise this algorithm
    // literally cannot make any decisions on what's good or not.
    let ingredients = input.iter().map(|s| parse_line(s).unwrap()).collect::<Vec<_>>();
    let mut choices = vec![1; 4];

    for _ in 4..100 {
        let mut best = 0;
        let mut best_index = 0;

        for i in 0..ingredients.len() {
            choices[i] += 1;
            let score = score(&ingredients.as_slice(), &choices);
            if score > best {
                best = score;
                best_index = i;
            }
            choices[i] -= 1;
        }

        choices[best_index] += 1;
    }

    Ok(score(&ingredients.as_slice(), &choices).to_string())
}

/// Find the best cookie recipe with exactly 100 units of ingredients, worth exactly 500 calories.
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    // Brute force lmao. The calorie restriction actually vastly limits the search space, thus
    // making brute force more viable than in part 1!

    // Recursively loops through all possible recipes (given the calorie restriction) and assigns
    // the highest found score to `best`.
    fn find_best_recipe_score(index: usize, ingredients: &[Vec<i32>], choices: &mut [i32], best: &mut i32, slots: i32, calories: i32) {
        if index == ingredients.len() - 1 {
            // Last ingredient doesn't need to be looped: Instead we just give all remaining slots
            // to it, since we HAVE to use exactly 100. Then, only use the recipe if that actually
            // matches the calorie count we want.
            choices[index] = slots;
            let calories: i32 = (0..ingredients.len()).map(|i| ingredients[i][4] * choices[i]).sum();
            if calories == 500 {
                *best = std::cmp::max(*best, score(&ingredients, &choices));
            }
        } else {
            // Loop through this level of possible choices, then recurse with the next index to
            // loop through that. Ultimately, this will result in all ingredients (except the last)
            // getting their own loop.
            for choice in 0..=std::cmp::min(slots, calories / ingredients[index][4]) {
                choices[index] = choice;
                find_best_recipe_score(index + 1, ingredients, choices, best, slots - choice, calories - choice * ingredients[index][4]);
            }
        }
    }

    let ingredients =  {
        // sort by calories descending, to minimize the number of iterations in outer loops
        // we're doing later
        let mut temp = input.iter().map(|s| parse_line(s).unwrap()).collect::<Vec<_>>();
        temp.as_mut_slice().sort_by_key(|i| 100 - i[4]);
        temp
    };

    let mut best = 0;
    let mut choices = vec![0; ingredients.len()];

    find_best_recipe_score(0, &ingredients.as_slice(), &mut choices.as_mut_slice(), &mut best, 100, 500);

    Ok(best.to_string())
}

/// Scores a cookie recipe according to the rules in the problem.
fn score(ingredients: &[Vec<i32>], counts: &[i32]) -> i32 {
    let mut subscores = vec![0i32; 4];
    for i in 0..counts.len() {
        for j in 0..subscores.len() {
            subscores[j] += ingredients[i][j] * counts[i];
        }
    }
    subscores.iter().take(4).copied().map(|x| std::cmp::max(x, 0)).reduce(|a, b| a * b).unwrap()
}

/// Extracts the properties of an ingredient from a line of puzzle input.
fn parse_line(s: &str) -> Option<Vec<i32>> {
    lazy_static::lazy_static! {
        static ref RE: Regex = Regex::new(r"-?\d+").unwrap();
    }

    let mut matches = RE.find_iter(s);
    let capacity = matches.next().and_then(|m| m.as_str().parse().ok())?;
    let durability = matches.next().and_then(|m| m.as_str().parse().ok())?;
    let flavor = matches.next().and_then(|m| m.as_str().parse().ok())?;
    let texture = matches.next().and_then(|m| m.as_str().parse().ok())?;
    let calories = matches.next().and_then(|m| m.as_str().parse().ok())?;

    Some(vec![capacity, durability, flavor, texture, calories])
}

// Old solution. It assumed there will always be exactly 4 ingredients.
fn _part2_first_solution(input: &[&str]) -> anyhow::Result<String> {
    // Given a list of amount choices made so far, returns an upper bounds for how many you could
    // conceivably pick of the next ingredient.
    fn maximum_possible_units(ingredients: &Vec<Vec<i32>>, choices: &[i32]) -> i32 {
        let remaining_slots = 100 - choices.iter().copied().sum::<i32>();
        let remaining_calories = 500 - (0..choices.len()).map(|i| ingredients[i][4] * choices[i]).sum::<i32>();
        std::cmp::min(remaining_slots, remaining_calories / ingredients[choices.len()][4])
    }

    let ingredients =  {
        // sort by calories descending, to minimize the number of loops we're doing later
        let mut temp = input.iter().map(|s| parse_line(s).unwrap()).collect::<Vec<_>>();
        temp.as_mut_slice().sort_by_key(|i| 100 - i[4]);
        temp
    };

    let mut best = vec![1; 4];
    let mut best_score = 0;
    for i in 0..maximum_possible_units(&ingredients, &[]) {
        for j in 0..maximum_possible_units(&ingredients, &[i]) {
            for k in 0..maximum_possible_units(&ingredients, &[i, j]) {
                let m = 100 - (i + j + k);
                let counts = &[i, j, k, m];
                let calories: i32 = (0..ingredients.len()).map(|i| ingredients[i][4] * counts[i]).sum();
                if calories != 500 {
                    continue;
                } else {
                    let score = score(&ingredients.as_slice(), counts);
                    if score > best_score {
                        best.clear();
                        best.extend_from_slice(counts);
                        best_score = score;
                    }
                }
            }
        }
    }

    Ok(best_score.to_string())
}
