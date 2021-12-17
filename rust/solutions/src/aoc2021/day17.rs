use itertools::Itertools;
use std::collections::HashMap;

/// Find the biggest height you could conceivably achieve with a shot, and still land in the target
/// area.
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    let (_, ys) = parse(input).ok_or(anyhow::anyhow!("failed to parse input"))?;
    
    // We can completely ignore the horizontal component of the velocity, because we know that we
    // can always find a speed that gives us infinite steps (by reaching x velocity 0 within the 
    // allowed horizontal range due to drag).
    let mut result = 0;
    for y in 1..(ys.1 - ys.0) * 4 {
        let start = y * (y + 1) / 2;
        for n in 1.. {
            let sum = start - n * (n + 1) / 2;
            if ys.0 <= sum && sum <= ys.1 {
                result = y;
                break;
            }
            else if sum < ys.0 {
                break;
            }
        }
    }
    Ok((result * (result + 1) / 2).to_string())
}

/// Count the number of initial velocities that would land within the target area.
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    let (xs, ys) = parse(input).ok_or(anyhow::anyhow!("failed to parse input"))?;

    // Build a map of [(step range) => (size of set of initial x velocities with that range)]
    let mut valid_xs = HashMap::new();
    for x in 1..xs.1 + 1 {
        if let Some(range) = step_range_for_x(x, xs) {
            *valid_xs.entry(range).or_insert(0) += 1;
        }
    }

    // Build a map of [(step range) => (size of set of initial y velocities with that range)]
    let mut valid_ys = HashMap::new();
    for y in ys.0..(ys.1 - ys.0) * 4 {
        if let Some(range) = step_range_for_y(y, ys) {
            *valid_ys.entry(range).or_insert(0) += 1;
        }
    }

    // If step ranges for a set of X velocities and Y velocities overlap, that means we can use
    // any combination of those X and Y velocities to get a valid initial velocity. The size of
    // of the cartesian product of two sets is the product of the sizes of those sets.
    let mut count = 0;
    for ((&rx, &vx), (&ry, &vy)) in valid_xs.iter().cartesian_product(valid_ys.iter()) {
        if rx.0 <= ry.1 && (rx.1.is_none() || ry.0 <= rx.1.unwrap()) {
            count += vx * vy;
        }
    }
    Ok(count.to_string())
}

/// Returns the range of steps during which the X coordinate is within the target for the given
/// initial velocity. Because horizontal velocity can reach 0, it is possible that we stay in the
/// target forever; in that case the returned step range has no upper bound.
fn step_range_for_x(mut velocity: i32, target: (i32, i32)) -> Option<(i32, Option<i32>)> {
    let mut distance = 0;
    let mut steps = 0;
    let mut min = None;

    loop {
        distance += velocity;
        velocity -= velocity.signum();
        steps += 1;

        match (min.is_some(), target.0 <= distance, distance <= target.1, velocity == 0) {
            // Entered the target.
            (false, true, true, _) => min = Some(steps),
            // Passed the target.
            (_, true, false, _) => return min.map(|a| (a, Some(steps - 1))),
            // Stopped moving.
            (_, _, _, true) => return min.map(|a| (a, None)),
            // Nothing special happened, carry on.
            _ => {},
        }
    }
}

/// Returns the range of steps during which the Y coordinate is within the target for the given
/// initial velocity.
fn step_range_for_y(mut velocity: i32, target: (i32, i32)) -> Option<(i32, i32)> {
    let mut distance = 0;
    let mut steps = 0;
    let mut min = None;

    loop {
        distance += velocity;
        velocity -= 1;
        steps += 1;

        match (min.is_some(), target.0 <= distance, distance <= target.1) {
            // Entered the target.
            (false, true, true) => min = Some(steps),
            // Passed the target.
            (_, false, true) => return min.map(|a| (a, steps - 1)),
            // Nothing special happened, carry on.
            _ => {},
        }
    }
}

/// Parses the puzzle input into a pair of target ranges, for x and y coordinates, respectively.
fn parse(input: &[&str]) -> Option<((i32, i32), (i32, i32))> {
    let mut nums = input[0].split(&['=', '.', ','][..]).filter_map(|s| s.parse().ok());
    Some((
        (nums.next()?, nums.next()?),
        (nums.next()?, nums.next()?),
    ))
}
