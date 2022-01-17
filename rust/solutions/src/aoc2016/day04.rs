use std::collections::HashMap;

use itertools::Itertools;

/// Sum the Ids of all real rooms. 
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    Ok(input
        .into_iter()
        .filter_map(|s| id_if_real(s))
        .sum::<u32>()
        .to_string())
}

/// Find the Id of the North Pole storage room.
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    Ok(input
        .into_iter()
        .filter_map(|s| id_if_northpole_object_storage(s))
        .next()
        .ok_or(anyhow::anyhow!("no room matched name"))?
        .to_string())
}

/// Returns the Id of the room, if and only if it is not a decoy.
fn id_if_real(roomspec: &str) -> Option<u32> {
    let mut map: HashMap<u8, usize> = HashMap::new();
    let (data, checksum) = roomspec.split_once('[').unwrap_or(("", ""));

    for &c in data.as_bytes() {
        if c.is_ascii_alphabetic() {
            *map.entry(c).or_default() += 1;
        }
    }

    let top: Vec<_> = map
        .into_iter()
        .sorted_by(|a, b| b.1.cmp(&a.1).then(a.0.cmp(&b.0)))
        .map(|c| c.0)
        .take(5)
        .collect();

    if top.len() == 5 && (0..5).all(|i| top[i] == checksum.as_bytes()[i]) {
        Some(data.split('-').last()?.parse().ok()?)
    } else {
        None
    }
}

/// Returns the Id of the room, if and only if it's where North Pole objects are stored.
fn id_if_northpole_object_storage(roomspec: &str) -> Option<u32> {
    // Returns the given alphabetic character, rotated forward by `n` characters; ignores all other
    // characters except dashes, which become spaces.
    fn rotn(b: u8, n: u8) -> u8 {
        match b {
            b'-' => b' ',
            b if b.is_ascii_alphabetic() => (b - b'a' + n) % 26 + b'a',
            _ => b,
        }
    }

    let name = "northpole object storage".as_bytes();

    (name.len() < roomspec.as_bytes().len())
        .then(|| {
            id_if_real(roomspec).and_then(|id| {
                let shift = (id % 26) as u8;
                name.into_iter()
                    .zip(roomspec.as_bytes())
                    .all(|(&a, &b)| a == rotn(b, shift))
                    .then(|| id)
            })
        })
        .flatten()
}
