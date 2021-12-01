/// In the input file, count the number of lines that have a larger value than the preceding line.
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    let depths = input.iter().map(|&s| s.parse::<u32>().unwrap()).collect::<Vec<_>>();
    Ok(depths.iter()
        .zip(depths.iter().skip(1))
        .filter(|&(a, b)| b > a)
        .count().to_string())
}

/// Consider all possible width-3 windows in the input file. Count the number of windows that have
/// a larger sum than the window starting one line prior.
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    let depths = input.iter().map(|&s| s.parse::<u32>().unwrap()).collect::<Vec<_>>();
    let window_sums = depths.windows(3).map(|w| w.iter().sum()).collect::<Vec<u32>>();
    Ok(window_sums.iter()
        .zip(window_sums.iter().skip(1))
        .filter(|&(a, b)| b > a)
        .count().to_string())
}
