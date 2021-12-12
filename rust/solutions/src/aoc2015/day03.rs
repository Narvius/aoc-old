use std::collections::HashMap;

// Follow the instructions, count visited positions.
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    let mut visited = HashMap::new();
    walk(&mut visited, input[0].chars());
    Ok(visited.keys().count().to_string())
}

/// Split up the instructions into two sequences, by alternating elements.
/// Run these separately, and count the number of positions they visit together.
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    let mut visited = HashMap::new();
    let (a, b): (Vec<_>, Vec<_>) = input[0].chars().enumerate().partition(|(i, _)| i % 2 == 0);
    walk(&mut visited, a.iter().map(|&(_, c)| c));
    walk(&mut visited, b.iter().map(|&(_, c)| c));
    Ok(visited.keys().count().to_string())
}

/// Executes a sequence of instructions, logging all positions that were visited.
fn walk(visited: &mut HashMap<(i32, i32), usize>, instructions: impl Iterator<Item = char>) {
    let mut position = (0, 0);
    *visited.entry(position).or_insert(0) += 1;
    for c in instructions {
        step(&mut position, c);
        *visited.entry(position).or_insert(0) += 1;
    }
}

/// Executes a single step of the instructions.
fn step(p: &mut (i32, i32), c: char) {
    match c {
        '<' => p.0 -= 1,
        '^' => p.1 -= 1,
        '>' => p.0 += 1,
        'v' => p.1 += 1,
        _ => { }
    }
}
