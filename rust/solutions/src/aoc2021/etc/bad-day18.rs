/// Sum up all the snailfish numbers in the input, find the magnitude of the result.
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    let expressions = parse(input);
    let final_expression = expressions.into_iter().reduce(add_expressions);
    
    Ok(magnitude(&final_expression.unwrap()).to_string())
}

/// Find the highest magnitude obtainable from adding any two different snailfish numbers from the
/// input.
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    let expressions = parse(input);

    let mut best = 0;
    for i in 0..expressions.len() {
        for j in 0..expressions.len() {
            if i == j {
                continue;
            }
            best = best.max(magnitude(&add_expressions(expressions[i].clone(), expressions[j].clone())));
        }
    }
    Ok(best.to_string())
}

/// A single token of a snailfish number. A snailfish number is simply a sequence of these, with
/// the property that a "pair" has the shape of `[OpenPair, child, child, ClosePair]`, where `child`
/// is either a `Literal(n)` or another pair with the same shape.
#[derive(Clone, Copy)]
enum Atom {
    OpenPair,
    Literal(u8),
    ClosePair,
}

impl Atom {
    /// If this is a literal atom, returns the value; otherwise, panics.
    fn unwrap_literal(self) -> u8 {
        match self {
            Atom::OpenPair => panic!("attempted to unwrap an OpenPair"),
            Atom::Literal(n) => n,
            Atom::ClosePair => panic!("attempted to unwrap a ClosePair"),
        }
    }
}

/// Calculates the magnitude of a snailfish number.
fn magnitude(atoms: &[Atom]) -> u64 {
    fn closing_index(atoms: &[Atom], start: usize) -> usize {
        let mut depth = 0;
        for i in start + 1.. {
            match atoms[i] {
                Atom::OpenPair => depth += 1,
                Atom::ClosePair if depth == 0 => return i,
                Atom::ClosePair => depth -= 1,
                _ => {},
            }
        }
        panic!("malformed expression")
    }

    // We're assuming the first element is an OpenPair, so content starts on atoms[1].
    let mut left_end = 1;
    let left_val = match atoms[1] {
        Atom::OpenPair => {
            left_end = closing_index(atoms, 1);
            magnitude(&atoms[1..=left_end])
        },
        Atom::Literal(n) => n as u64,
        _ => panic!("malformed expression"),
    };

    let right_val = match atoms[left_end + 1] {
        Atom::OpenPair => magnitude(&atoms[left_end + 1..]),
        Atom::Literal(n) => n as u64,
        _ => panic!("malformed expression"),
    };

    3 * left_val + 2 * right_val
}

/// Adds two snailfish numbers together.
fn add_expressions(mut lhs: Vec<Atom>, rhs: Vec<Atom>) -> Vec<Atom> {
    lhs.insert(0, Atom::OpenPair);
    lhs.extend(rhs);
    lhs.push(Atom::ClosePair);
    loop {
        if explode_once(&mut lhs) {
            continue;
        }

        if split_once(&mut lhs) {
            continue;
        }

        return lhs;
    }
}

/// Finds the first explodable pair and explodes it. For details, see the puzzle description.
fn explode_once(atoms: &mut Vec<Atom>) -> bool {
    let mut depth = 0;
    for i in 0..atoms.len() {
        // Read through the snailfish expression until we reach explode depth (if at all).
        match atoms[i] {
            Atom::OpenPair => depth += 1,
            Atom::ClosePair => depth -= 1,
            _ => {},
        }

        // We've reached explode depth. We're assuming that the slice of 4 entries starting here
        // is a pair of two literals, because neither the puzzle input nor the operations during
        // the puzzle produce a deeper depth.
        if depth == 5 {
            let removed = atoms.drain(i + 1..i + 4).collect::<Vec<_>>();

            // add left number to the first number to the left
            for j in 0..i {
                if let Some(Atom::Literal(t)) = atoms.get_mut(i - j) {
                    *t += removed[0].unwrap_literal();
                    break;
                }
            }
            // add right number to the first number to the right
            for j in i + 1..atoms.len() {
                if let Some(Atom::Literal(t)) = atoms.get_mut(j) {
                    *t += removed[1].unwrap_literal();
                    break;
                }
            }

            atoms[i] = Atom::Literal(0);

            return true;
        }
    }

    false
}

/// Finds the first splittable atom (a literal with a value of 10 or more) and replaces it with
/// a split pair.
fn split_once(atoms: &mut Vec<Atom>) -> bool {
    for i in 0..atoms.len() {
        if let Atom::Literal(n) = atoms[i] {
            // We've found an atom to explode.
            if n >= 10 {
                atoms[i] = Atom::OpenPair;
                atoms.insert(i + 1, Atom::Literal(n / 2));
                atoms.insert(i + 2, Atom::Literal((n + 1) / 2));
                atoms.insert(i + 3, Atom::ClosePair);
                return true;
            }
        }
    }
    false
}

/// Parses the puzzle input into a series of snailfish numbers.
fn parse(input: &[&str]) -> Vec<Vec<Atom>> {
    let mut result = vec![];
    for line in input {
        let mut expr = vec![];
        for c in line.as_bytes() {
            match c {
                b'[' => expr.push(Atom::OpenPair),
                b',' => {},
                b']' => expr.push(Atom::ClosePair),
                _ => expr.push(Atom::Literal(c - b'0'))
            }
        }
        result.push(expr);
    }
    result
}
