mod solutions;

use std::time::Duration;
use solutions::{Answer, Part};

fn main() {
    let args: Vec<String> = std::env::args().collect();
    
    // Select solutions to run based on command line input.
    if args.len() != 2 {
        println!("1 argument expected, got {}", args.len() - 1);
        return;
    }

    let target = { if let Ok(target) = Target::new(&args[1]) {
        target
    } else {
        println!("invalid argument; either a number between 1-25, or an asterisk (*) for 'all'");
        return;
    }};

    // Run solutions, and print results.
    let (answers, _) = target.execute();
    let mut runtime = Duration::new(0, 0);

    for answer in answers {
        match answer.answer {
            Ok(a) => println!("Day {}{}: {} (runtime: {}s)",
                answer.day,
                match answer.part { Part::One => 'a', Part::Two => 'b' },
                a,
                as_seconds(answer.runtime)),
            Err(e) => println!("(FAILED) Day {}{}: {}",
                answer.day,
                match answer.part { Part::One => 'a', Part::Two => 'b' },
                e),
        }

        runtime += answer.runtime;
    }

    println!("Total runtime: {}", as_seconds(runtime));
}

/// Describes which solution(s) to run.
enum Target {
    Single(usize),
    All,
}

impl Target {
    /// Parse a [`Target`] from a string slice.
    /// Legal inputs are an integer between 1 and 25 (inclusive), or a single asterisk.
    /// All other inputs will result in an Err return value.
    fn new(arg: &str) -> anyhow::Result<Target> {
        match arg.parse::<usize>() {
            Ok(n) if 1 <= n && n <= 25 => Ok(Target::Single(n)),
            Err(_) if arg == "*" => Ok(Target::All),
            _ => Err(anyhow::anyhow!("invalid argument: expected number between 1-25, or an asterisk (*) for 'all'"))
        }
    }

    /// Executes all solutions included by this [`Target`], and collects all calculated answers as
    /// well as errors, then returns them.
    fn execute(&self) -> (Vec<Answer>, Vec<String>) {
        let range = match *self {
            Target::Single(n) => n..=n,
            Target::All => 1..=25,
        };

        let mut answers = vec![];
        let mut fails = vec![];

        for n in range {
            match solutions::run_solution(n) {
                Ok((a1, a2)) => {
                    answers.push(a1);
                    answers.push(a2);
                },
                Err(e) => fails.push(format!("{:?}", e)),
            }
        }

        (answers, fails)
    }
}

/// The number seconds in the [`std::time::Duration`], with three significant digits of fractional
/// precision.
fn as_seconds(duration: Duration) -> f32 {
    (duration.as_millis() as f32) / 1000.0
}