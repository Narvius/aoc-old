/// Find the sum of all syntax error scores. That is, for every line with mismatched closing
/// characters, count that mismatched closing character as an arbitrary score given by the puzzle.
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    let mut score = 0;

    for &line in input {
        let mut stack = vec![];

        for &byte in line.as_bytes() {
            match byte {
                b'(' | b'[' | b'{' | b'<' => stack.push(byte),
                b')' => if stack.pop() != Some(b'(') { score += 3; break; },
                b']' => if stack.pop() != Some(b'[') { score += 57; break; },
                b'}' => if stack.pop() != Some(b'{') { score += 1197; break; },
                b'>' => if stack.pop() != Some(b'<') { score += 25137; break; },
                _ => {}
            }
        }
    }

    Ok(score.to_string())
}

/// Find the middle autocompletion score. That is, for every line that is missing additional
/// closing characters, find those missing closing characters, then derive a score from them, then
/// choose the median of the set (which is guaranteed to have an uneven number of entries).
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    let mut scores = vec![];

    'outer: for &line in input {
        let mut stack = vec![];

        for &byte in line.as_bytes() {
            match byte {
                b'(' | b'[' | b'{' | b'<' => stack.push(byte),
                b')' => if stack.pop() != Some(b'(') { continue 'outer; },
                b']' => if stack.pop() != Some(b'[') { continue 'outer; },
                b'}' => if stack.pop() != Some(b'{') { continue 'outer; },
                b'>' => if stack.pop() != Some(b'<') { continue 'outer; },
                _ => {}
            }
        }

        scores.push(stack.into_iter().rev().fold(0u64, |a, b| a * 5 + match b {
            b'(' => 1,
            b'[' => 2,
            b'{' => 3,
            b'<' => 4,
            _ => 0,
        }));
    }

    scores.sort();
    Ok(scores[scores.len() / 2].to_string())
}
