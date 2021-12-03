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
        // split the candidates list into those for which the relevant bit is '0', and for which
        // the relevant bit is '1'.
        let zero_count = crate::util::separate_by(&mut candidates, &b'0', |s| &s.as_bytes()[i]);
        let one_partition = candidates.split_off(zero_count);

        // at this point, 'candidates' is the 'zero' partition, and 'one_partition' is the, well,
        // 'one' partition. Now, decide which of these to keep, and store it in 'candidates'. The
        // other partition is dropped and deallocated.
        match zero_count.cmp(&one_partition.len()) {
            Ordering::Equal => if tiebreaker == b'1' { candidates = one_partition; },
            ordering => if ordering != partition { candidates = one_partition; },
        }

        if candidates.len() == 1 {
            break;
        }
    }

    u32::from_str_radix(candidates[0], 2).unwrap()
}
