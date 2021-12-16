use std::collections::HashMap;

/// Get the code from a simple keypad, based on puzzle rules.
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    let buttons = [
        ((0, 0), '1'), ((1, 0), '2'), ((2, 0), '3'), 
        ((0, 1), '4'), ((1, 1), '5'), ((2, 1), '6'), 
        ((0, 2), '7'), ((1, 2), '8'), ((2, 2), '9'),
    ].into_iter().collect::<HashMap<_, _>>();

    Ok(code_from_keypad_and_sequences((2, 2), buttons, input))
}

/// Get the code from a complicated keypad, based on puzzle rules.
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    let buttons = [
        ((2, 0), '1'),
        ((1, 1), '2'), ((2, 1), '3'), ((3, 1), '4'),
        ((0, 2), '5'), ((1, 2), '6'), ((2, 2), '7'), ((3, 2), '8'), ((4, 2), '9'),
        ((1, 3), 'A'), ((2, 3), 'B'), ((3, 3), 'C'),
        ((2, 4), 'D'),
    ].into_iter().collect::<HashMap<_, _>>();

    Ok(code_from_keypad_and_sequences((0, 2), buttons, input))
}

/// Given an initial `pos`ition, a `keypad` layout and a list of `sequences` to follow, gets the
/// resulting code.
fn code_from_keypad_and_sequences(
    mut pos: (isize, isize),
    keypad: HashMap<(isize, isize), char>,
    sequences: &[&str]
) -> String {
    let mut output = String::with_capacity(sequences.len());

    for &sequence in sequences {
        for c in sequence.chars() {
            let next = match c {
                'L' => (pos.0 - 1, pos.1),
                'U' => (pos.0, pos.1 - 1),
                'R' => (pos.0 + 1, pos.1),
                'D' => (pos.0, pos.1 + 1),
                _ => pos,
            };

            if let Some(_) = keypad.get(&next) {
                pos = next;
            }
        }
        output.push(*keypad.get(&pos).unwrap());
    }

    output
}
