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
                "[{}-{:02}{}]   FAILED: {}",
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

/// Reads command line arguments and returns the relevant solutions. The program accepts between
/// 0 and 3 (inclusive) arguments, and the meaning changes depending on the amount.
///
/// - "" (a special mode that runs only the latest day solution from the latest event)
/// - "day"
/// - "event day"
/// - "event day part"
///
/// `event` defaults to "L" (the latest event known to the program). `day` has no default. `part`
/// defaults to "." (all parts). You can supply a comma-separated list of numbers, "L" or "." to
/// each argument.
///
/// `event` expects numbers in the range from 15 (AoC2015) and up; `day` expects numbers in the
/// range from 1 to 25 (inclusive); `part` expects a 1 or 2.
///
/// When a list or "." are provided in multiple arguments, all possible combinations of those lists
/// will be run.
fn solutions_from_args() -> Option<Vec<Solution>> {
    let args: Vec<_> = std::env::args().collect();

    let (events, days, parts) = match args.len() {
        1 => {
            // No argument. Run the highest day solution available from the default event (the
            // as if you supplied "L" for the year).
            let event = keys::Event::parse("L")?;
            let mut days = keys::Day::parse(".")?;
            days.retain(|&d| {
                Solution::new(keys::Key {
                    event: event[0],
                    day: d,
                    part: Part::One,
                })
                .is_some()
            });
            days.reverse();
            days.truncate(1);

            (event, days, keys::Part::parse(".")?)
        }
        2 => (
            // One argument: day, assuming some convenient year and all parts.
            keys::Event::parse("L")?,
            keys::Day::parse(&args[1])?,
            keys::Part::parse(".")?,
        ),
        3 => (
            // Two arguments: year and day, assuming all parts.
            keys::Event::parse(&args[1])?,
            keys::Day::parse(&args[2])?,
            keys::Part::parse(".")?,
        ),
        4 => (
            // Three arguments: year, day and part.
            keys::Event::parse(&args[1])?,
            keys::Day::parse(&args[2])?,
            keys::Part::parse(&args[3])?,
        ),
        _ => return None,
    };

    Some(
        events
            .iter()
            .cartesian_product(&days)
            .cartesian_product(&parts)
            .map(|((&event, &day), &part)| keys::Key { event, day, part })
            .filter_map(Solution::new)
            .collect(),
    )
}
