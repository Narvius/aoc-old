/// Find the code to input from the infinite diagonal page.
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    let index = to_index(parse(input).ok_or(anyhow::anyhow!("failed to parse input"))?);

    let mut code = 20151125u64;
    for _ in 1..index {
        code = (code * 252533) % 33554393;
    }
    Ok(code.to_string())
}

/// Freebie!
pub fn part2(_input: &[&str]) -> anyhow::Result<String> {
    Ok(format!("done!"))
}

/// Converts the row and column from the puzzle input into a straight index.
/// # Formula
/// `S(n)` = sum of numbers in `(1..=n)`, zero for n < 1.
/// 
/// `Index(row, col) = S(col) + S(row - 2) + col * (row - 1)`
/// 
/// This formula was derived by hand on paper, an excerpt follows:
/// 
/// ```text
/// We can trivially observe that Index(row, 1) = S(row).
/// We can observe the following:
/// Index(1, col) = S(col)
/// Index(2, col) = Index(1, col) + row = S(col) + col
/// Index(3, col) = Index(2, col) + row + 1 = S(col) + 2col + 1
/// Index(4, col) = Index(3, col) + row + 2 = S(col) + 3col + 3
/// Index(5, col) = Index(4, col) + row + 3 = S(col) + 4col + 6
/// 
/// and so on.
/// 
/// At each step, the amount of "col" in the sum grows by one, and the additional number is
/// clearly just S(n) again, but shifted.
/// 
/// So that gives us: Index(row, col) = S(col) + (row - 1) * col + S(row - 2).
/// ```
/// 
fn to_index((row, col): (usize, usize)) -> usize {
    col * (col + 1) / 2 + (row - 1) * col + row.saturating_sub(2) * (row.saturating_sub(1)) / 2
}

/// Parses the puzzle input into a row and column
fn parse(s: &[&str]) -> Option<(usize, usize)> {
    let numbers: Vec<_> = s[0]
        .split(&[' ', '.', ','][..])
        .filter(|s| s.len() > 0 && s.chars().all(|c| c.is_digit(10)))
        .collect();
    Some((numbers.get(0)?.parse().ok()?, numbers.get(1)?.parse().ok()?))
}
