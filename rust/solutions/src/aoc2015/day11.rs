// Get the next valid password.
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    let mut password = input[0].chars().collect::<Vec<_>>();

    while !is_acceptable_password(&password) {
        advance_password(&mut password);
    }

    Ok(password.into_iter().collect())
}

// Get the next valid password after that.
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    let mut password = input[0].chars().collect::<Vec<_>>();

    while !is_acceptable_password(&password) {
        advance_password(&mut password);
    }

    advance_password(&mut password);

    while !is_acceptable_password(&password) {
        advance_password(&mut password);
    }

    Ok(password.into_iter().collect())
}

/// Checks if a password is acceptable according to the problem specification.
fn is_acceptable_password(s: &[char]) -> bool {

    let has_ascending_letters = s.windows(3).any(|w| w[0] as u8 + 1 == w[1] as u8 && w[1] as u8 + 1 == w[2] as u8);
    let has_repeated_nonoverlapping_pair = {
        let mut pairs = s.windows(2).filter(|w| w[0] == w[1]).map(|w| w[0]);
        if let Some(c) = pairs.next() {
            pairs.any(|w| c != w)
        } else {
            false
        }
    };
    let excludes_forbidden_chars = s.iter().all(|&c| !"iol".contains(c));

    has_ascending_letters && has_repeated_nonoverlapping_pair && excludes_forbidden_chars
}

/// Gets the alexicographical successor for the current password. The space is limited to lowercase
/// ASCII characters.
fn advance_password(s: &mut [char]) {
    for i in (0..s.len()).rev() {
        match s[i] {
            'z' => s[i] = 'a',
            c => { s[i] = (c as u8 + 1) as char; return; },
        }
    }
}