use anyhow::Result;
use keys::Key;
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
