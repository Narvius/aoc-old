use std::collections::HashSet;

/// Traverse the path from the instructions, return taxicab distance from origin afterwards.
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    let (mut p, mut v) = ((0i32, 0i32), (0i32, -1i32));

    for instruction in input[0].split(", ") {
        v = match &instruction[0..1] {
            "L" => rotated_left(v),
            "R" => rotated_right(v),
            _ => v,
        };
        let d = instruction[1..].parse::<i32>()?;
        p.0 += v.0 * d;
        p.1 += v.1 * d;
    }

    Ok((p.0.abs() + p.1.abs()).to_string())
}

/// Traverse the path from the instructions, remembering each point visited. Return taxicab distance
/// from origin of the first point visited twice.
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    let mut set = HashSet::new();
    let (mut p, mut v) = ((0i32, 0i32), (0i32, -1i32));

    for instruction in input[0].split(", ") {
        v = match &instruction[0..1] {
            "L" => rotated_left(v),
            "R" => rotated_right(v),
            _ => v,
        };
        let d = instruction[1..].parse::<i32>()?;

        for _ in 0..d {
            p.0 += v.0;
            p.1 += v.1;

            if set.contains(&p) {
                return Ok((p.0.abs() + p.1.abs()).to_string());
            } else {
                set.insert(p);
            }
        }
    }

    unreachable!()
}

/// Returns the same point rotated left by 90 degrees.
fn rotated_left((x, y): (i32, i32)) -> (i32, i32) { (y, -x) }

/// Returns the same point rotated right by 90 degrees.
fn rotated_right((x, y): (i32, i32)) -> (i32, i32) { (-y, x) }