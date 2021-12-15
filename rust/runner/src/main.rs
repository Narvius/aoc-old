//! Entry point for running solutions.

mod solution;

use crate::solution::Solution;
use itertools::Itertools;
use keys::Part;
use std::time::{Duration, Instant};

fn main() {
    let solutions = solutions_from_args().expect("could not parse arguments");

    let start = Instant::now();

    for solution in solutions {
        match solution.get_result() {
            (Ok(a), runtime) => println!(
                "[{}-{:02}{}]   {} (runtime: {}s)",
                solution.key.event as u16,
                solution.key.day as u8,
                match solution.key.part {
                    Part::One => 'a',
                    Part::Two => 'b',
                },
                a,
                duration_as_string(runtime)
            ),
            (Err(e), _) => println!(
                "(FAILED) [{}-{:02}{}]   {}",
                solution.key.event as u16,
                solution.key.day as u8,
                match solution.key.part {
                    Part::One => 'a',
                    Part::Two => 'b',
                },
                e
            ),
        }
    }

    let end = Instant::now();
    println!(
        "Total runtime: {}s",
        duration_as_string(end.duration_since(start))
    );
}

/// The number seconds in the [`std::time::Duration`], with three significant digits of fractional
/// precision.
fn duration_as_string(duration: Duration) -> String {
    let value = (duration.as_millis() as f32) / 1000.0;
    if value == 0.0 {
        String::from("less than 0.001")
    } else {
        format!("{}", value)
    }
}

fn solutions_from_args() -> Option<Vec<Solution>> {
    let args: Vec<_> = std::env::args().collect();

    let (events, days, parts) = match args.len() {
        2 => {
            // One argument: day, assuming some convenient year and all parts.
            (
                keys::Event::parse("L")?,
                keys::Day::parse(&args[1])?,
                keys::Part::parse(".")?,
            )
        },
        3 => {
            // Two arguments: year and day, assuming all parts.
            (
                keys::Event::parse(&args[1])?,
                keys::Day::parse(&args[2])?,
                keys::Part::parse(".")?,
            )
        },
        4 => {
            // Three arguments: year, day and part.
            (
                keys::Event::parse(&args[1])?,
                keys::Day::parse(&args[2])?,
                keys::Part::parse(&args[3])?,
            )
        },
        _ => return None,
    };

    Some(events.iter()
        .cartesian_product(&days)
        .cartesian_product(&parts)
        .map(|((&event, &day), &part)| keys::Key { event, day, part })
        .filter_map(|key| Solution::new(key))
        .collect())
}
