/// Find the sum of all syntax error scores. That is, for every line with mismatched closing
/// characters, count that mismatched closing character as an arbitrary score given by the puzzle.
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    Ok(input.into_iter().fold(0, |a, b| {
        a + match incomplete_tail(b) {
            Err(b'(') => 3,
            Err(b'[') => 57,
            Err(b'{') => 1197,
            Err(b'<') => 25137,
            _ => 0,
        }
    }).to_string())
}

/// Find the middle autocompletion score. That is, for every line that is missing additional
/// closing characters, find those missing closing characters, then derive a score from them, then
/// choose the median of the set (which is guaranteed to have an uneven number of entries).
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    let mut scores = input.into_iter().filter_map(|s| {
        match incomplete_tail(s) {
            Ok(v) => Some(v.into_iter().rev().fold(0u64, |a, b| a * 5 + match b {
                b'(' => 1,
                b'[' => 2,
                b'{' => 3,
                b'<' => 4,
                _ => 0,
            })),
            _ => None,
        }
    }).collect::<Vec<_>>();

    scores.sort();
    Ok(scores[scores.len() / 2].to_string())
}

/// Syntax checks the line, and returns either `Ok(stack of remaining unclosed delimiters)`, or
/// `Err(mismatched opening delimiter)`, depending on whether it is an incomplete or corrupted line.
fn incomplete_tail(s: &str) -> Result<Vec<u8>, u8> {
    let mut stack = vec![];

    for &byte in s.as_bytes() {
        match byte {
            b'(' | b'[' | b'{' | b'<' => stack.push(byte),
            b')' => if stack.pop() != Some(b'(') { return Err(b'('); },
            b']' => if stack.pop() != Some(b'[') { return Err(b'['); },
            b'}' => if stack.pop() != Some(b'{') { return Err(b'{'); },
            b'>' => if stack.pop() != Some(b'<') { return Err(b'<'); },
            _ => {},
        }
    }

    Ok(stack)
}