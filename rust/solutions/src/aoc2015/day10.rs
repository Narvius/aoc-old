/// Expand the input 40 times according to look-and-see sequence rules, then get the number of
/// digits.
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    let mut vec1 = input[0].chars().map(|c| match c { '1' => 1, '2' => 2, '3' => 3, _ => 0 }).collect::<Vec<_>>();
    let mut vec2 = vec![0; vec1.len()];

    for _ in 0..20 {
        step_look_and_say(&mut vec1, &mut vec2);
        step_look_and_say(&mut vec2, &mut vec1);
    }

    Ok(vec1.len().to_string())
}

/// Expand the input 50 times according to look-and-see sequence rules, then get the number of
/// digits.
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    let mut vec1 = input[0].chars().map(|c| match c { '1' => 1, '2' => 2, '3' => 3, _ => 0 }).collect::<Vec<_>>();
    let mut vec2 = vec![0; vec1.len()];

    for _ in 0..25 {
        step_look_and_say(&mut vec1, &mut vec2);
        step_look_and_say(&mut vec2, &mut vec1);
    }

    Ok(vec1.len().to_string())
}

/// Calculates the next step in the look-and-say sequence given by `source`, and writes it into
/// `target`.
/// # Panics
/// When `source` is empty.
fn step_look_and_say(source: &mut Vec<u8>, target: &mut Vec<u8>) {
    target.clear();
    let mut last = source[0];
    let mut run = 1;
    for i in 1..source.len() {
        if source[i] == last {
            run += 1;
        } else {
            target.push(run);
            target.push(last);
            last = source[i];
            run = 1;
        }
    }
    target.push(run);
    target.push(last);
}
