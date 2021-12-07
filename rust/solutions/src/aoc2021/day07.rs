/// Find the best horizontal alignment if fuel costs are linear.
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    let crabs = input[0].split(',').map(|s| s.parse::<i32>()).collect::<Result<Vec<_>, _>>()?;
    Ok(find_best_fuel_cost(&crabs, |n| n).to_string())
}

/// Find the best horizontal alignment if fuel costs are triangular.
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    let crabs = input[0].split(',').map(|s| s.parse::<i32>()).collect::<Result<Vec<_>, _>>()?;
    Ok(find_best_fuel_cost(&crabs, |n| n * (n + 1) / 2).to_string())
}

/// Check all reasonable horizontal position alignments, and pick the cheapest one. `crabs` contains
/// the starting horizontal alignments, and `fuel_formula` maps a horizontal change to the cost of
/// achieving it.
fn find_best_fuel_cost(crabs: &[i32], fuel_formula: fn(i32) -> i32) -> i32 {
    let min = *crabs.iter().min().unwrap();
    let max = *crabs.iter().max().unwrap();

    (min..=max)
        .map(|c| crabs.iter().map(|i| fuel_formula((c - i).abs())).sum())
        .min().unwrap()
}