use itertools::Itertools;

/// Find the best possible quantum entanglement value for 3 equally-weighted groups.
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    Ok(find_quantum_entanglement(
        &input
            .iter()
            .map(|&s| s.parse().unwrap())
            .collect::<Vec<_>>(),
        3,
    )
    .unwrap()
    .to_string())
}

/// Find the best possible quantum entanglement value for 4 equally-weighted groups.
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    Ok(find_quantum_entanglement(
        &input
            .iter()
            .map(|&s| s.parse().unwrap())
            .collect::<Vec<_>>(),
        4,
    )
    .unwrap()
    .to_string())
}

/// Calculates the smallest possible quantum entanglement value (see puzzle description) for the
/// given set of `weights` if they were split up into `groups` groups of equal total weights.
fn find_quantum_entanglement(weights: &[usize], groups: usize) -> Option<usize> {
    let group_weight = weights.iter().copied().sum::<usize>() / groups;

    // smallest theoretically possible size is the number of largest elements we need to take
    // before the sum is larger than the target. So if we find any group of that size, that means
    // we only need to consider those.
    let best_size = {
        let (mut remaining, mut i) = (group_weight as i32, weights.len() - 1);
        while remaining > 0 {
            remaining -= weights[i] as i32;
            i -= 1;
        }
        weights.len() - i
    };

    groups_of_size(group_weight, best_size, &weights)
        .map(|g| g.iter().copied().reduce(|a, b| a * b).unwrap())
        .min()
}

/// Returns all possible combinations from `weights` of the given `size` that sum up to `weight`.
fn groups_of_size<'a>(
    weight: usize,
    size: usize,
    weights: &'a [usize],
) -> impl Iterator<Item = Vec<usize>> + 'a {
    (0..weights.len())
        .combinations(size)
        .filter_map(move |mut c| {
            for i in 0..c.len() {
                c[i] = weights[c[i]];
            }
            (c.iter().sum::<usize>() == weight).then(|| c)
        })
}
