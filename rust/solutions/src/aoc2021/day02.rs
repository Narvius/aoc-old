/// Run the submarine course, reading the commands in a simple way.
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    let (mut h, mut d) = (0i32, 0i32);
    for cmd in input.iter().map(|s| s.parse::<Command>().unwrap()) {
        match cmd.direction {
            Direction::Up => d -= cmd.delta,
            Direction::Down => d += cmd.delta,
            Direction::Forward => h += cmd.delta,
        }
    }
    Ok((h * d).to_string())
}

/// Run the submarine course, reading the commands in a slightly convoluted way described in the
/// problem statement.
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    let (mut h, mut d, mut a) = (0i32, 0i32, 0i32);
    for cmd in input.iter().map(|s| s.parse::<Command>().unwrap()) {
        match cmd.direction {
            Direction::Up => a -= cmd.delta,
            Direction::Down => a += cmd.delta,
            Direction::Forward => { h += cmd.delta; d += a * cmd.delta; },
        }
    }
    Ok((h * d).to_string())
}

/// A parsed line of puzzle input.
struct Command {
    direction: Direction,
    delta: i32,
}

impl std::str::FromStr for Command {
    type Err = &'static str;
    fn from_str(s: &str) -> Result<Self, Self::Err> {
        let tokens = s.split_once(' ').ok_or("parsed str must consist of two values")?;
        Ok(Command {
            direction: match tokens.0 {
                "up" => Direction::Up,
                "down" => Direction::Down,
                "forward" => Direction::Forward,
                _ => Err("first value is not a valid direction")?,
            },
            delta: tokens.1.parse().map_err(|_| "second value is not a valid i32")?,
        })
    }
}

enum Direction {
    Up, Down, Forward
}