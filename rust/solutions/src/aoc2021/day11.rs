use itertools::Itertools;
use std::collections::HashSet;

/// Simulate the octopi for 100 steps and count the total number of flashes.
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    let mut map: Vec<Vec<u8>> = input.into_iter()
        .map(|s| s.as_bytes().into_iter().map(|b| b - b'0').collect()).collect();
    
    let mut flashes = 0;
    for _ in 0..100 {
        flashes += step(&mut map);
    }
    Ok(flashes.to_string())
}

/// Find the number of steps needed until all octopi flash simultaneously.
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    let mut map: Vec<Vec<u8>> = input.into_iter()
        .map(|s| s.as_bytes().into_iter().map(|b| b - b'0').collect()).collect();
    
    for steps in 1.. {
        if step(&mut map) == map.len() * map[0].len() {
            return Ok(steps.to_string());
        }
    }

    unreachable!()
}

/// Steps the octopus simulation once and returns the number of flashes that occurred.
fn step(map: &mut Vec<Vec<u8>>) -> usize {
    let mut found = HashSet::new();
    let mut queue = vec![];

    for (y, x) in (0..map.len()).cartesian_product(0..map[0].len()) {
        map[y][x] += 1;
        if map[y][x] > 9 {
            found.insert((x, y));
            queue.push((x, y));
        }
    }

    while let Some(p) = queue.pop() {
        for (x, y) in neighbours(map[0].len(), map.len(), p) {
            map[y][x] += 1;
            if map[y][x] > 9 && !found.contains(&(x, y)) {
                found.insert((x, y));
                queue.push((x, y));
            }
        }
    }

    let flashes = found.len();

    for (x, y) in found {
        map[y][x] = 0;
    }

    flashes
}

/// Returns an iterator over the neighbours of coordinate. Works for invalid coordinates, but will
/// most likely return nothing.
fn neighbours(width: usize, height: usize, (x, y): (usize, usize)) -> impl Iterator<Item = (usize, usize)> {
    [(-1i32, 0), (0, -1i32), (1, 0), (0, 1), (-1, -1), (1, -1), (-1, 1), (1, 1)].iter()
        .filter_map(move |(dx, dy)| -> Option<(usize, usize)> {
            let x = usize::try_from(x as i32 + dx).ok()?;
            let y = usize::try_from(y as i32 + dy).ok()?;

            (x < width && y < height).then(|| (x, y))
        })
}