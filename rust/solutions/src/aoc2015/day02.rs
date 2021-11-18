/// Calculate the amount of wrapping paper needed according to specification.
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    Ok(sum_for_each_present(input, |(l, w, h)| {
        let items = [l * w, w * h, h * l];
        items.iter().copied().sum::<u32>() + &items.iter().copied().min().unwrap()
    }).to_string())
}

/// Calculate the amount of ribbon needed according to specification.
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    Ok(sum_for_each_present(input, |(l, w, h)| {
        (l + w + h + l * w * h - &[l, w, h].iter().copied().max().unwrap()) * 2 
    }).to_string())
}

/// Calls the provided function for each present and sums the resulting numbers.
fn sum_for_each_present(input: &[&str], f: fn((u32, u32, u32)) -> u32) -> u32 {
    input.iter().map(|&s| dimensions(s)).map(f).sum()
}

/// Parses a line from the puzzle input: "(number)x(number)x(number)"
/// Probably panics for arbitrary inputs.
fn dimensions(input: &str) -> (u32, u32, u32) {
    let items: Vec<u32> = input.split('x').map(|s| s.parse::<u32>().unwrap()).collect();
    (items[0], items[1], items[2])
}