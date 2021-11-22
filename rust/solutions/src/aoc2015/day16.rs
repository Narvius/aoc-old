use regex::Regex;
use std::cmp::Ordering;
use std::collections::HashMap;

/// Find the Sue for which all three facts match the sue_facts table (ignoring the Ordering
/// requirement).
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    let facts = sue_facts();

    for (n, (k1, v1), (k2, v2), (k3, v3)) in input.iter().map(|s| parse_line(s)) {
        if facts[k1].0 == v1 && facts[k2].0 == v2 && facts[k3].0 == v3 {
            return Ok(n.to_string());
        }
    }

    unreachable!()
}

/// Find the Sue for which all three facts match sue_facts table (including the Ordering
/// requirement).
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    let facts = sue_facts();

    for (n, (k1, v1), (k2, v2), (k3, v3)) in input.iter().map(|s| parse_line(s)) {
        let match1 = v1.cmp(&facts[k1].0) == facts[k1].1;
        let match2 = v2.cmp(&facts[k2].0) == facts[k2].1;
        let match3 = v3.cmp(&facts[k3].0) == facts[k3].1;
        if match1 && match2 && match3 {
            return Ok(n.to_string());
        }
    }

    unreachable!()
}

/// Parses a line of puzzle input into a Sue entry.
fn parse_line(s: &str) -> (usize, (&str, u32), (&str, u32), (&str, u32)) {
    lazy_static::lazy_static! {
        static ref RE: Regex = Regex::new(r"^Sue (\d+): ([^:]+): (\d+), ([^:]+): (\d+), ([^:]+): (\d+)$").unwrap();
    }

    let cs = RE.captures(s).unwrap();

    // Today I realized that I don't really need to allocate Strings. The puzzle input lives for
    // the entire scope of the puzzle solution code, after all. I knew there were gonna be benefits
    // to coercing it into &[&str] :^)
    (
        cs[1].parse().unwrap(),
        (&s[cs.get(2).unwrap().range()], cs[3].parse().unwrap()),
        (&s[cs.get(4).unwrap().range()], cs[5].parse().unwrap()),
        (&s[cs.get(6).unwrap().range()], cs[7].parse().unwrap())
    )
}

/// Contains all the information gained from the MFCSAM.
fn sue_facts() -> HashMap<&'static str, (u32, Ordering)> {
    [
        ("children", (3, Ordering::Equal)),
        ("cats", (7, Ordering::Greater)),
        ("samoyeds", (2, Ordering::Equal)),
        ("pomeranians", (3, Ordering::Less)),
        ("akitas", (0, Ordering::Equal)),
        ("vizslas", (0, Ordering::Equal)),
        ("goldfish", (5, Ordering::Less)),
        ("trees", (3, Ordering::Greater)),
        ("cars", (2, Ordering::Equal)),
        ("perfumes", (1, Ordering::Equal))
    ].iter().copied().collect()
}