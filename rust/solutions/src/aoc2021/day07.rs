use crate::util::mapped_sum;

/// Find the best horizontal alignment if fuel costs are linear.
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    let mut crabs = input[0].split(',').map(|s| s.parse::<i32>()).collect::<Result<Vec<_>, _>>()?;
    // The cheapest alignment is at the median of the set, more or less by definition. We know
    // there's an odd amount of, so it's just the middle of the sorted list.
    crabs.sort();
    let target = crabs[crabs.len() / 2];
    Ok(mapped_sum(&crabs, |i| (target - i).abs()).unwrap().to_string())
}

/// Find the best horizontal alignment if fuel costs are triangular.
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    let crabs = input[0].split(',').map(|s| s.parse::<i32>()).collect::<Result<Vec<_>, _>>()?;
    // The cheapest alignment in this case is the average (arithmetic mean). I'm not quite sure why,
    // though.
    let target: i32 = crabs.iter().copied().sum::<i32>() / crabs.len() as i32;
    let sum = mapped_sum(&crabs, |i| ((target - i).abs() * ((target - i).abs() + 1) / 2)).unwrap();
    Ok(sum.to_string())
}
