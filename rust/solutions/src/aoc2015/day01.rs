/// Following the elevator instructions, get the final floor.
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    Ok(input[0].chars().fold(0i32, |floor, c| floor + if c == '(' { 1 } else { -1 }).to_string())
}

/// Get the index of the elevator instruction that first results in entering the basement.
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    let mut floor = 0i32;
    Ok((input[0].chars().take_while(move |&c| { floor += if c == '(' { 1 } else { -1 }; floor >= 0}).count() + 1).to_string())
}
