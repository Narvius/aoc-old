use std::convert::TryInto;

/// Run a cellular automaton for 100 steps, count the live cells.
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    let mut grid1 = parse_input(input);
    let mut grid2 = vec![false; grid1.len()];
    let rules = |_, b, n| (b && n == 2) || n == 3;

    for _ in 0..50 {
        step_with_rules(&grid1, &mut grid2, rules);
        step_with_rules(&grid2, &mut grid1, rules);
    }

    Ok(grid1.iter().copied().filter(|&b| b).count().to_string())
}

/// Run a cellular automaton for 100 steps, count the live cells. The rules for the automaton
/// stipulate that the corner cells are *always* alive.
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    let mut grid1 = parse_input(input);
    let mut grid2 = vec![false; grid1.len()];
    let rules = |i, b, n| [0, 99, 9900, 9999].contains(&i) || (b && n == 2) || n == 3;
    grid1[0] = true;
    grid1[99] = true;
    grid1[9900] = true;
    grid1[9999] = true;

    for _ in 0..50 {
        step_with_rules(&grid1, &mut grid2, rules);
        step_with_rules(&grid2, &mut grid1, rules);
    }

    Ok(grid1.iter().copied().filter(|&b| b).count().to_string())
}

/// Steps the automaton using the given ruleset.
fn step_with_rules(source: &[bool], target: &mut [bool], rules: fn(usize, bool, u8) -> bool) {
    for i in 0..target.len() {
        target[i] = rules(i, source[i], true_neighbours(&source, i));
    }
}

/// Counts the number of neighbour that are true.
fn true_neighbours(grid: &[bool], i: usize) -> u8 {    
    // Offsets that move straight up or down are always ok (they'll just be out of bounds later)
    let indices = [-100, 100].iter()
        // only include offsets that end up further left if we're not at the left edge
        .chain(if i % 100 != 0 { [-101, -1, 99].iter() } else { [].iter() })
        // only include offsets that end up further right if we're not at the right edge
        .chain(if i % 100 != 99 { [-99, 1, 101].iter() } else { [].iter() });

    indices
        .filter(|&d| *(i as i32 + d).try_into().ok().and_then(|index: usize| grid.get(index)).unwrap_or(&false))
        .count() as u8
}

/// Parses the input into a grid of bools.
fn parse_input(input: &[&str]) -> Vec<bool> {
    let mut result = vec![false; 100 * 100];
    for (i, &v) in input.iter().enumerate() {
        for (j, c) in v.chars().enumerate() {
            if c == '#' {
                result[i * 100 + j] = true;
            }
        }
    }
    result
}