use std::collections::HashMap;

/// Find the number of different combinations of containers that can be used to reach an eggnog
/// total of 150.
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    let containers = input.iter().map(|s| s.parse().unwrap()).collect::<Vec<u32>>();

    // Every container can either be used or not used. That means we can use a binary number as a
    // "mask" of used containers. Using the example from the problem, if we had five containers
    // (20, 15, 10, 5 and 5), we could represent the solution 15 + 10 as 0b01100.
    // Iterating over all numbers from 0 to 2^(number of containers) now means iterating over all
    // possible combinations of containers.
    Ok((0..2u32.pow(containers.len() as u32))
        .map(|n| resolve_mask(&containers, n).0)
        .filter(|&n| n == 150)
        .count().to_string())
}

/// Find the number of different combinations of containers that can be used to reach an eggnog
/// total of 150, that ALSO use the least possible amount of containers.
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    let containers = input.iter().map(|s| s.parse().unwrap()).collect::<Vec<u32>>();
    
    // Maps 'amount of containers used' to 'ways to use that many containers'.
    let mut buckets: HashMap<u32, u32> = HashMap::new();
    for mask in 0..2u32.pow(containers.len() as u32) {
        let (sum, container_count) = resolve_mask(&containers, mask);
        if sum == 150 {
            *buckets.entry(container_count).or_insert(0) += 1;
        }
    }

    let key = buckets.keys().min().unwrap();
    Ok(buckets[key].to_string())
}

/// Given a mask where the `n`th bit (starting from least significant) describes whether the `n`th
/// container is used, returns the total volume of those containers, as well as their amount.
fn resolve_mask(containers: &[u32], mask: u32) -> (u32, u32) {
    let mut sum = 0;
    let mut container_count = 0;
    for i in 0..containers.len() {
        if mask & (1 << i) > 0 {
            sum += containers[i];
            container_count += 1;
        }
    }
    (sum, container_count)
}