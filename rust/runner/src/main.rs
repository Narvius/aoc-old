//! Entry point for running solutions.

mod solution;

use keys::Part;
use std::time::{Duration, Instant};
use crate::solution::Solution;

fn main() {
    let solutions = solutions_from_args().unwrap();

    let start = Instant::now();

    for solution in solutions {
        match solution.get_result() {
            (Ok(a), runtime) => println!("Day {:02}{}: {} (runtime: {}s)",
                solution.key.day as u8,
                match solution.key.part { Part::One => 'a', Part::Two => 'b' },
                a,
                { let m = as_seconds(runtime); if m == 0.0 { format!("less than 0.001") } else { format!{"{}", m} }}),
            (Err(e), _) => println!("(FAILED) Day {:02}{}: {}",
                solution.key.day as u8,
                match solution.key.part { Part::One => 'a', Part::Two => 'b' },
                e),
        }
    }

    let end = Instant::now();
    println!("Total runtime: {}s", as_seconds(end.duration_since(start)));
}

/// The number seconds in the [`std::time::Duration`], with three significant digits of fractional
/// precision.
fn as_seconds(duration: Duration) -> f32 {
    (duration.as_millis() as f32) / 1000.0
}

fn solutions_from_args() -> Option<Vec<Solution>> {
    let args: Vec<_> = std::env::args().collect();

    Some(
        if (args.len() == 2 && args[1] == ".") || args.len() == 1 {
            let mut result = vec![];
            for day in 1..=25 {
                for c in "ab".chars() {
                    if let Some(s) = Solution::from_keyspec(&format!("15{:02}{}", day, c)) {
                        result.push(s);
                    }
                }
            }
            result
        } else if args.len() == 2 {
            let day = args[1].parse::<u8>().unwrap();

            vec![
                Solution::from_keyspec(&format!("15{:02}a", day))?,
                Solution::from_keyspec(&format!("15{:02}b", day))?]
        } else {
            vec![]
        })
}
