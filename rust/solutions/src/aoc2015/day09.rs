use itertools::Itertools;

/// Find the shortest possible route that visits all nodes.
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    let (names, distances) = build_distance_matrix(input);

    Ok((0..names.len())
        .permutations(names.len())
        .map(|order| distance_for_sequence(&order, &distances))
        .min().unwrap().to_string())
}

/// Find the longest possible route that visits all nodes.
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    let (names, distances) = build_distance_matrix(input);

    Ok((0..names.len())
        .permutations(names.len())
        .map(|order| distance_for_sequence(&order, &distances))
        .max().unwrap().to_string())
}

/// For a given order of nodes, returns the total distance of that specific route.
fn distance_for_sequence(order: &[usize], distances: &[u32]) -> u32 {
    order.windows(2).map(|w| distances[w[0] + w[1] * order.len()]).sum()
}

/// Builds a matrix-based graph from the input data; then returns the labels and matrix as separate
/// [`Vec`]s.
fn build_distance_matrix(data: &[&str]) -> (Vec<String>, Vec<u32>) {
    // collect names
    let mut names = vec![String::from(data[0].split(' ').next().unwrap())];
    let first_name = names[0].clone();
    for line_with_name in data.iter().take_while(move |s| s.starts_with(first_name.as_str())) {
        names.push(String::from(line_with_name.split(' ').nth(2).unwrap()));
    }

    // build connection graph
    let mut distances = vec![0; names.len() * names.len()];
    for line in data.iter() {
        if let &[town1, _to, town2, _equals, distance] = line.split(' ').collect::<Vec<_>>().as_slice() {
            let p1 = names.iter().position(|n| n == town1).unwrap();
            let p2 = names.iter().position(|n| n == town2).unwrap();
            let distance = distance.parse::<u32>().unwrap();

            distances[p1 + p2 * names.len()] = distance;
            distances[p2 + p1 * names.len()] = distance;
        } else {
            unreachable!()
        }
    }

    (names, distances)
}
