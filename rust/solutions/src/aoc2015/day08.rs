/// See [`superfluous_characters_when_reducing`].
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    Ok(input.iter().map(|s| superfluous_characters_when_reducing(*s)).sum::<u32>().to_string())
}

/// See [`superfluous_characters_when_expanding`].
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    Ok(input.iter().map(|s| superfluous_characters_when_expanding(*s)).sum::<u32>().to_string())
}

/// Counts how many more characters there are in the string literal respresentation of a string
/// than there's actual logical characters in the string.
fn superfluous_characters_when_reducing(s: &str) -> u32 {
    2 + {
        let mut result = 0;
        let mut escaping = false;

        for c in s.chars() {
            if escaping {
                result += if c == 'x' { 3 } else { 1 };
                escaping = false;
            } else if c == '\\' {
                escaping = true;
            }
        }

        result
    }
}

/// Counts how many more characters there are in the string literal representation that would
/// produce the original string, than said original string.
fn superfluous_characters_when_expanding(s: &str) -> u32 {
    2 + s.chars().filter(|&c| c == '"' || c == '\\').count() as u32
}
