use std::collections::HashMap;

/// Count the number of "nice" strings in the input.
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    Ok(input.iter().filter(|s| is_nice(s)).count().to_string())
}

/// Count the number of "nice" strings in the input, using new, more complicated rules for "nice"ness.
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    Ok(input.iter().filter(|s| is_actually_nice(s)).count().to_string())
}

/// Checks whether a string is "nice".
fn is_nice(s: &str) -> bool {
    let chars: Vec<_> = s.chars().collect();

    let has_3_vowels = s.chars().filter(|&c| "aeiou".contains(c)).count() >= 3;
    let has_double_letter = chars.windows(2).any(|w| w[0] == w[1]);
    let excludes_naughty_pairs = chars.windows(2).all(|w| !matches!(w, ['a', 'b'] | ['c', 'd'] | ['p', 'q'] | ['x', 'y']));

    has_3_vowels && has_double_letter && excludes_naughty_pairs
}

/// Checks whether a string is "nice" according to the updated rules from part 2.
fn is_actually_nice(s: &str) -> bool {
    let chars: Vec<_> = s.chars().collect();
    let mut pairs = HashMap::new();
    let mut last_pair: Option<(char, char)> = None;

    for pair in chars.windows(2).map(|w| (w[0], w[1])) {
        if last_pair != Some(pair) {
            *pairs.entry(pair).or_insert(0) += 1;
            last_pair = Some(pair);
        } else {
            last_pair = None;
        }
    }
    
    let has_repeated_pair = pairs.values().any(|&v| v >= 2);
    let has_aba_sequence = chars.windows(3).any(|w| w[0] == w[2]);
    
    has_repeated_pair && has_aba_sequence
}