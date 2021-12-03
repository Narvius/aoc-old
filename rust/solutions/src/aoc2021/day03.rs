use std::cmp::Ordering;

/// Find the gamma rate (most common bit in each position in the input) and the epsilon rate (the
/// inverse of the gamma rate), and multiply them together.
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    let mut gamma = 0u32;
    for i in 0..input[0].len() {
        gamma *= 2;
        let ones = input.iter().filter(|s| s.as_bytes()[i] == b'1').count();
        if ones * 2 >= input.len() {
            gamma += 1;
        }
    }
    // This is basically equivalent to `!gamma`, but constrained to `input[0].len` binary digits.
    let epsilon = (1 << input[0].len() as u32) - 1 - gamma;

    Ok((gamma * epsilon).to_string())
}

/// Find the two ratings according to some convoluted bit-based filtering mechanism, and multiply
/// them together.
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    let oxygen = find_rating(input, Ordering::Greater, b'1');
    let scrubber = find_rating(input, Ordering::Less, b'0');

    Ok((oxygen * scrubber).to_string())
}

/// Filters the input data until only one value remains, then returns that value. `partition`
/// determines which bit to keep (most or least common), and `tiebreaker` is the bit that gets used
/// when both bits are equally common.
fn find_rating(data: &[&str], partition: Ordering, tiebreaker: u8) -> u32 {
    let mut candidates: Vec<_> = data.iter().map(|&s| s).collect();

    for i in 0..data[0].len() {
        if candidates.len() == 1 {
            break;
        }

        // decide which bit to keep
        let ones = candidates.iter().filter(|s| s.as_bytes()[i] == b'1').count();
        let to_keep = match (2 * ones).cmp(&candidates.len()) {
            Ordering::Equal => tiebreaker,
            ones_partition if ones_partition == partition => b'1',
            _ => b'0',
        };

        // remove all candidates without the matching bit
        let mut n = 0;
        while n < candidates.len() {
            if candidates[n].as_bytes()[i] != to_keep {
                candidates.remove(n);
            } else {
                n += 1;
            }
        }
    }

    u32::from_str_radix(candidates[0], 2).unwrap()
}