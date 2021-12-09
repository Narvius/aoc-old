use itertools::Itertools;
use std::collections::{HashMap, HashSet};

/// Sum the values at low points (incremented by 1).
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    let map: Vec<Vec<u8>> = input.into_iter()
        .map(|s| s.as_bytes().into_iter().map(|b| b - b'0').collect()).collect();

    Ok(low_points(&map).map(|(x, y)| map[y][x] as u32 + 1).sum::<u32>().to_string())
}

/// Find the three largest basins, and get the product of their sizes.
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    let map: Vec<Vec<u8>> = input.into_iter()
        .map(|s| s.as_bytes().into_iter().map(|b| b - b'0').collect()).collect();
    let mut basins = HashMap::new();
    for p in low_points(&map) {
        basins.insert(p, basin_size(&map, p));
    }

    Ok(basins.into_values()
        .sorted_by(|a, b| b.cmp(a))
        .take(3).reduce(|a, b| a * b)
        .unwrap().to_string())
}

/// Returns the size of a basin for the given low point. May produce incorrect results for
/// arguments that are not low points.
fn basin_size(map: &Vec<Vec<u8>>, (x, y): (usize, usize)) -> u32 {
    let mut basin = HashSet::new();
    let mut stack = vec![(x, y)];
    basin.insert((x, y));

    while let Some(p) = stack.pop() {
        for n in neighbours(map, p) {
            if !basin.contains(&n) && basin.contains(&flows_towards(map, n)) {
                basin.insert(n);
                stack.push(n);
            }
        }
    }

    basin.len() as u32
}

/// Returns the coordinate this coordinate "flows towards", ie. the lowest neighbour. Extreme
/// values (that is, 0 and 9) "flow towards" themselves.
fn flows_towards(map: &Vec<Vec<u8>>, (x, y): (usize, usize)) -> (usize, usize) {
    match map[y][x] {
        0 | 9 => (x, y),
        _ => neighbours(map, (x, y)).min_by_key(|&(x, y)| map[y][x]).unwrap_or((x, y)),
    }
}


/// Returns a list of all "low points" in the map, as defined by the problem statement.
fn low_points<'a>(map: &'a Vec<Vec<u8>>) -> impl Iterator<Item = (usize, usize)> + 'a {
    (0..map[0].len()).cartesian_product(0..map.len())
        .filter(move |&(x, y)| {
            match map.get(y).and_then(|xs| xs.get(x)) {
                Some(&v) => neighbours(map, (x, y)).all(|(x, y)| v < map[y][x]),
                _ => false,
            }
        })
}

/// Returns an iterator over the neighbours of coordinate. Works for invalid coordinates, but will
/// most likely return nothing.
fn neighbours<'a>(map: &'a Vec<Vec<u8>>, (x, y): (usize, usize)) -> impl Iterator<Item = (usize, usize)> + 'a {
    [(-1i32, 0), (0, -1i32), (1, 0), (0, 1)].iter()
        .filter_map(move |(dx, dy)| -> Option<(usize, usize)> {
            let x = usize::try_from(x as i32 + dx).ok()?;
            let y = usize::try_from(y as i32 + dy).ok()?;

            map.get(y).and_then(|xs| xs.get(x))?;
            Some((x, y))
        })
}