use itertools::Itertools;

/// Find the seating arrangement with the best total happiness.
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    let (names, deltas) = build_happiness_matrix(input, false);

    Ok((0..names.len())
        .permutations(names.len())
        .map(|arrangement| happiness_for_arrangement(&arrangement, &deltas))
        .max().unwrap().to_string())
}

/// Find the seating arrangement with the best total happiness, but including yourself this time.
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    let (names, deltas) = build_happiness_matrix(input, true);

    Ok((0..names.len())
        .permutations(names.len())
        .map(|arrangement| happiness_for_arrangement(&arrangement, &deltas))
        .max().unwrap().to_string())
}

/// Calculates the total happiness for a given seating arrangement.
fn happiness_for_arrangement(arrangement: &[usize], deltas: &[i32]) -> i32 {
    let mut wrapped = vec![];
    wrapped.push(arrangement.last().copied().unwrap());
    wrapped.extend_from_slice(arrangement);
    wrapped.push(arrangement[0]);

    wrapped.windows(3).map(|w|
         deltas[w[1] + w[0] * arrangement.len()] + deltas[w[1] + w[2] * arrangement.len()]
    ).sum()
}

/// Builds a matrix-baesd graph from the input data; then returns the labels and matrix as separate
/// [`Vec`]s. If `include_yourself` is set, a new node not included in the input data is added,
/// with all edges for it set to 0.
fn build_happiness_matrix(data: &[&str], include_yourself: bool) -> (Vec<String>, Vec<i32>) {
    // collect names
    let mut names = vec![String::from(data[0].split(' ').next().unwrap())];
    let first_name = names[0].clone();
    for line_with_name in data.iter().take_while(move |s| s.starts_with(first_name.as_str())) {
        let name_with_period = line_with_name.split(' ').last().unwrap();
        names.push(String::from(&name_with_period[0..name_with_period.len() - 1]));
    }

    if include_yourself {
        names.push(String::from("Yourself"));
    }

    // build happiness graph
    let mut deltas = vec![0; names.len() * names.len()];
    for line in data.iter() {
        if let &[name1, _would, alter, n, _happiness, _units, _by, _sitting, _next, _to, name2] = line.split(' ').collect::<Vec<_>>().as_slice() {
            let name2 = &name2[0..name2.len() - 1];
            let p1 = names.iter().position(|n| n == name1).unwrap();
            let p2 = names.iter().position(|n| n == name2).unwrap();
            let delta = n.parse::<i32>().unwrap() * match alter {
                "gain" => 1,
                "lose" => -1,
                _ => unreachable!()
            };

            deltas[p1 + p2 * names.len()] = delta;
        }
    }

    (names, deltas)
}