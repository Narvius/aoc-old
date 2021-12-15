use std::cmp::Ordering;
use std::collections::BinaryHeap;

/// Find the lowest risk achievable when crossing the grid.
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    let grid = parse(input).ok_or(anyhow::anyhow!("failed to parse input"))?;
    Ok(find_risk(grid).ok_or(anyhow::anyhow!("failed to calculate risk"))?.to_string())
}

/// Find the lowest risk achievable when crossing the fivefold expanded grid.
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    let grid = parse_large(input).ok_or(anyhow::anyhow!("failed to parse input"))?;
    Ok(find_risk(grid).ok_or(anyhow::anyhow!("failed to calculate risk"))?.to_string())
}

/// Calculates the lowest possible risk as per the puzzle rules.
fn find_risk(mut grid: Vec<Vec<u32>>) -> Option<u32> {
    let mut seekers = BinaryHeap::new();
    seekers.push(Seeker { cost: 0, position: (0, 0) });

    let endpoint = (grid[0].len() - 1, grid.len() - 1);
    while let Some(seeker) = seekers.pop() {
        // Take the lowest-cost seeker so far.
        if seeker.position == endpoint {
            return Some(seeker.cost);
        }

        // Spawn new seekers on all adjacent unconsumed squares, consuming them.
        // This is valid, because that way a seeker on a square always represents the lowest-cost
        // possible seeker that it *could* have. By maintaining this property across the entire
        // grid, we end up with the lowest possible cost on the `endpoint`.
        for (x, y) in neighbours(seeker.position) {
            if let Some(v) = grid.get_mut(y).and_then(|xs| xs.get_mut(x)) {
                if *v > 0 {
                    seekers.push(Seeker { position: (x, y), cost: seeker.cost + *v });
                    *v = 0;
                }
            }
        }
    }

    None
}

/// Gets orthogonally-adjacent coordinates that fit into `usize`.
fn neighbours((x, y): (usize, usize)) -> impl Iterator<Item = (usize, usize)> {
    [(-1i32, 0i32), (0, -1), (1, 0), (0, 1)]
        .into_iter()
        .filter_map(move |(dx, dy)| Some((
            usize::try_from(x as i32 + dx).ok()?,
            usize::try_from(y as i32 + dy).ok()?
        )))
}

/// A path search head that stores the accumulated cost so far and its position.
#[derive(Copy, Clone, Eq, PartialEq)]
struct Seeker {
    cost: u32,
    position: (usize, usize),
}

// Seekers get sorted by cost ascending, so that the lowest cost gets popped off a binary heap
// priority queue.
impl Ord for Seeker {
    fn cmp(&self, other: &Self) -> Ordering {
        other.cost.cmp(&self.cost)
            .then_with(|| self.position.0.cmp(&other.position.0))
            .then_with(|| self.position.1.cmp(&other.position.1))
    }
}

impl PartialOrd for Seeker {
    fn partial_cmp(&self, other: &Self) -> Option<Ordering> {
        Some(self.cmp(other))
    }
}

/// Parses the puzzle input into a grid.
fn parse(input: &[&str]) -> Option<Vec<Vec<u32>>> {
    let mut ys = vec![];
    for &line in input {
        ys.push(line.as_bytes().into_iter().map(|&b| (b - b'0') as u32).collect());
    }
    Some(ys)
}

/// Parses the puzzle input into a fivefold expanded grid with incremented sectors.
fn parse_large(input: &[&str]) -> Option<Vec<Vec<u32>>> {
    let mut ys = vec![];
    for y_offset in 0..5 {
        for &line in input {
            let mut xs = vec![];
            for x_offset in 0..05 {
                xs.extend(line.as_bytes().into_iter()
                    .map(|&b| (1 + (b - b'0' - 1 + x_offset + y_offset) % 9) as u32));
            }
            ys.push(xs);
        }
    }
    Some(ys)
}