/// Run the submarine course, reading the commands in a simple way.
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    let (h, d) = input.iter()
        .map(|s| s.parse::<Command>().unwrap())
        .fold((0i32, 0i32), |(h, d), cmd| match cmd.direction {
            Direction::Up => (h, d - cmd.delta),
            Direction::Down => (h, d + cmd.delta),
            Direction::Forward => (h + cmd.delta, d),
        });

    Ok((h * d).to_string())
}

/// Run the submarine course, reading the commands in a slightly convoluted way described in the
/// problem statement.
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    let (h, d, _) = input.iter()
        .map(|s| s.parse::<Command>().unwrap())
        .fold((0i32, 0i32, 0i32), |(h, d, a), cmd| match cmd.direction {
            Direction::Up => (h, d, a - cmd.delta),
            Direction::Down => (h, d, a + cmd.delta),
            Direction::Forward => (h + cmd.delta, d + a * cmd.delta, a),
        });
        
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
