use anyhow::{anyhow, Result};
use keys::{Day, Event, Key, Part};
use std::convert::TryFrom;
use std::fs::File;
use std::io::BufRead;
use std::time::{Duration, Instant};

pub struct Solution {
    pub key: Key,
    pub solution: fn(&[&str]) -> Result<String>,
    data_path: &'static str,
}

impl Solution {
    pub fn new(key: Key) -> Option<Solution> {
        solutions::get_solution(key).map(|(solution, data_path)| Solution {
            key,
            solution,
            data_path,
        })
    }

    pub fn from_keyspec(spec: &str) -> Option<Solution> {
        Self::new(parse_key(spec).ok()?)
    }

    pub fn get_result(&self) -> (Result<String>, Duration) {
        let raw_input: Vec<_> = {
            let file = File::open(self.data_path)
                .expect(&format!("failed to open data file '{}'", self.data_path));

            std::io::BufReader::new(file)
                .lines()
                .map(|l| l.unwrap())
                .collect()
        };

        let ref_input: Vec<_> = raw_input.iter().map(|s| &**s).collect();

        let start = Instant::now();
        let returned = (self.solution)(&ref_input);
        let end = Instant::now();

        (returned, end.duration_since(start))
    }
}

fn parse_key(keyspec: &str) -> Result<Key> {
    if keyspec.len() != 5 || keyspec.chars().count() != 5 {
        Err(anyhow!("parse_key: keyspec must be 5 ASCII characters"))
    } else {
        let year = 2000 + keyspec[0..2].parse::<u16>()?;
        let day = keyspec[2..4].parse::<u8>()?;
        let part = (if keyspec.chars().nth(4).unwrap() == 'a' {
            Ok(Part::One)
        } else if keyspec.chars().nth(4).unwrap() == 'b' {
            Ok(Part::Two)
        } else {
            Err(anyhow!("final character of keyspec must be 'a' or 'b'"))
        })?;

        Ok(Key {
            event: Event::try_from(year)?,
            day: Day::try_from(day)?,
            part,
        })
    }
}
