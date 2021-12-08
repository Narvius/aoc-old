use itertools::Itertools;

/// Count the number of occurences of the digits 1, 4, 7, 8 in outputs.
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    let mut result = 0;
    for line in input {
        let (_input, output) = line.split_once(" | ").unwrap();
        // length 2 = digit 1; length 3 = digit 7, length 4 = digit 4, length 7 = digit 8
        // since those are unique and we only need to count them, no content analysis necessary.
        result += output.split_whitespace().filter(|s| [2, 3, 4, 7].contains(&s.len())).count();
    }
    Ok(result.to_string())
}

/// Identify the output numbers, and sum them.
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    let mut result = 0;
    for line in input {
        let (input, output) = line.split_once(" | ").unwrap();
        let input: Vec<_> = input.split_whitespace().map(|s| s.as_bytes()).collect();
        let mapping = deduce_mapping(&input);
        result += output.split_whitespace()
            .fold(0, |a, c| 10 * a + digit_from_mapping(c.as_bytes(), &mapping).unwrap());
    }
    Ok(result.to_string())
}

/// Given the "input" portion of a line of puzzle input, returns a mapping from the scrambled wire
/// letter, to the canonical wire letter (as used in [`PATTERNS`]).
fn deduce_mapping(digits: &[&[u8]]) -> Vec<u8> {
    let mut candidates = vec![b"abcdefg".iter().copied().collect::<Vec<_>>(); 7];

    // First pass.
    // The digits 1, 4, 7 and 8 are always uniquely identifiable by string length. 8 doesn't
    // constrain our candidate list though, so we only use 1, 4 and 7 here.
    for &digit in digits {
        let unique_digit = match digit.len() {
            2 => Some(1),
            3 => Some(7),
            4 => Some(4),
            _ => None,
        };

        if let Some(n) = unique_digit {
            // For each candidate set `C` for wire `b`: if `digit` contains `b`,
            // then `C = C & PATTERN[n]`. Otherwise, `C = C \ PATTERN[n]` (where A & B is set
            // intersection, and A \ B is set difference).
            for b in b'a'..=b'g' {
                let keep = digit.contains(&b);
                candidates[(b - b'a') as usize].retain(|c| !(keep ^ PATTERNS[n].contains(c)));
            }
        }
    }

    // Second pass.
    // By now we have so massively constrained the search space that we can just brute force the
    // rest of the way.
    for mapping in candidates.into_iter().multi_cartesian_product() {
        if digits.into_iter().all(|s| digit_from_mapping(s, &mapping).is_some()) {
            return mapping;
        }
    }

    unreachable!()
}

/// Given a 7 segment string and a [`mapping`](deduce_mapping), returns the digit represented by
/// the encoded 7 segment string.
fn digit_from_mapping(digit: &[u8], mapping: &[u8]) -> Option<usize> {
    let mut wires: Vec<_> = digit.iter().map(|c| mapping[(c - b'a') as usize]).collect();
    wires.sort();
    PATTERNS.iter().position(|s| s.eq(&wires))
}

/// `PATTERNS[n]` is the canonical wires for digit `n`.
static PATTERNS: &[&[u8]] = &[b"abcefg", b"cf", b"acdeg", b"acdfg", b"bcdf", b"abdfg", b"abdefg", b"acf", b"abcdefg", b"abcdfg"];

/// Old solution. Just brute force over all possible mappings and picking the one that doesn't
/// result in nonsense. Ran in about 6-7 seconds on the machine I was testing on, which is fiiiine,
/// but also unacceptable.
#[allow(dead_code)]
fn old_part2(input: &[&str]) -> anyhow::Result<String> {
    let mut result = 0;

    let valid_mapping = |input: &str, mapping: &[u8]|
        input.split_whitespace().all(|s| digit_from_mapping(s.as_bytes(), mapping).is_some());

    for line in input {
        let (input, output) = line.split_once(" | ").unwrap();
        for mapping in (b'a'..=b'g').permutations(7) {
            if input.split_whitespace().all(|s| valid_mapping(s, &mapping)) {
                result += output.split_whitespace()
                    .fold(0, |a, c| 10 * a + digit_from_mapping(c.as_bytes(), &mapping).unwrap());
            }
        }
    }
    Ok(result.to_string())
}