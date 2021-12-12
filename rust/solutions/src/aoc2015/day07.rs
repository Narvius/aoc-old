use std::collections::HashMap;

/// Resolve all wire connections in the input, return value of wire "a".
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    let mut vals = HashMap::new();
    let mut connections = input.iter().map(|line| parse_line(line).unwrap()).collect::<Vec<_>>();
    while !connections.is_empty() {
        connections.retain(|c| c.apply(&mut vals).is_none())
    }
    Ok(vals.get("a").unwrap().to_string())
}

/// Take the result from part 1, set it as the initial value of wire "b", run it all again,
/// and return the new value of wire "a".
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    let mut vals = HashMap::new();
    let mut connections = input.iter().map(|line| parse_line(line).unwrap()).collect::<Vec<_>>();
    while !connections.is_empty() {
        connections.retain(|c| c.apply(&mut vals).is_none())
    }

    let a = *vals.get("a").unwrap();
    vals.clear();
    vals.insert(String::from("b"), a);
    let mut connections = input.iter().map(|line| parse_line(line).unwrap()).collect::<Vec<_>>();
    while !connections.is_empty() {
        connections.retain(|c| c.apply(&mut vals).is_none())
    }

    Ok(vals.get("a").unwrap().to_string())
}

/// Produces a [`Connection`] from a line of puzzle input.
fn parse_line(line: &str) -> Option<Connection> {
    let mut parts = line.split(" -> ");
    let left_parts = parts.next()?.split(' ').collect::<Vec<_>>();
    let right_part = parts.next()?;

    let source = match left_parts.len() {
        1 => Source::Id(left_parts[0].parse().ok()?),
        2 => Source::Not(left_parts[1].parse().ok()?),
        3 => (match left_parts[1] {
            "AND" => Source::And,
            "OR" => Source::Or,
            "LSHIFT" => Source::Lshift,
            "RSHIFT" => Source::Rshift,
            _ => unreachable!(),
        })(left_parts[0].parse().ok()?, left_parts[2].parse().ok()?),
        _ => unreachable!(),
    };

    Some(Connection { source, target: String::from(right_part) })
}

/// A wire connection described by a line of puzzle input.
struct Connection {
    source: Source,
    target: String,
}

impl Connection {
    /// Computes the value on this wire if possible, adds it to the map of wire values, and
    /// returns the computed result. Does not overwrite existing entries.
    fn apply(&self, vals: &mut HashMap<String, u16>) -> Option<u16> {
        let result = self.source.resolve(vals);
        if let Some(n) = result {
            let entry = vals.entry(self.target.clone()).or_insert(0);
            if *entry > 0 {
                return Some(*entry);
            }
            vals.insert(self.target.clone(), n);
        }
        result
    }
}

/// The expression that that gets assigned to a wire in a [`Connection`].
enum Source {
    /// The value of a single [`Atom`].
    Id(Atom),
    /// The value of a single [`Atom`], but negated.
    Not(Atom),
    /// The result of ANDing together the values of two [`Atom`]s.
    And(Atom, Atom),
    /// The result of ORing together the values of two [`Atom`]s.
    Or(Atom, Atom),
    /// The result of left-shifting the value of one [`Atom`] by the other.
    Lshift(Atom, Atom),
    /// The result of right-shifting the value of one [`Atom`] by the other.
    Rshift(Atom, Atom),
}

impl Source {
    /// Calculate the value of this source expression if possible; `None` if
    /// one of the atoms in it couldn't be resolved yet.
    fn resolve(&self, vals: &HashMap<String, u16>) -> Option<u16> {
        match self {
            Self::Id(v) => v.resolve(vals),
            Self::Not(v) => v.resolve(vals).map(|x| !x),
            Self::And(v1, v2) => Self::op(vals, v1, v2, |a, b| a & b),
            Self::Or(v1, v2) => Self::op(vals, v1, v2, |a, b| a | b),
            Self::Lshift(v1, v2) => Self::op(vals, v1, v2, |a, b| a << b),
            Self::Rshift(v1, v2) => Self::op(vals, v1, v2, |a, b| a >> b),
        }
    }

    /// Performs an arbitrary calculation on two [`Atom`]s, and returns the result. `None` if
    /// either atom couldn't be resolved yet.
    fn op(vals: &HashMap<String, u16>, v1: &Atom, v2: &Atom, op: fn(u16, u16) -> u16) -> Option<u16> {
        let v1 = v1.resolve(vals)?;
        let v2 = v2.resolve(vals)?;

        Some(op(v1, v2))
    }
}

/// A single value in a connection value expression.
enum Atom {
    /// A constant, numeric value.
    Const(u16),
    /// A reference to another wire.
    Label(String),
}

impl Atom {
    /// Resolves the atom to a value. Can fail if it is a reference to a wire, but that wire does
    /// not have a value assigned yet, in which case it returns `None`.
    fn resolve(&self, vals: &HashMap<String, u16>) -> Option<u16> {
        match self {
            Self::Const(n) => Some(*n),
            Self::Label(s) => vals.get(s).copied(),
        }
    }
}

impl std::str::FromStr for Atom {
    type Err = ();

    fn from_str(s: &str) -> Result<Self, Self::Err> {
        if s.chars().all(char::is_numeric) {
            Ok(Atom::Const(s.parse().unwrap()))
        } else {
            Ok(Atom::Label(String::from(s)))
        }
    }
}
