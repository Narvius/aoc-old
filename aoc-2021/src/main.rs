mod solutions;

use std::time::Duration;
use solutions::{Answer, Part};

fn main() {
    let args: Vec<String> = std::env::args().collect();

    // Handle 
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

    let (answers, _) = target.execute();

    let mut runtime = Duration::new(0, 0);

    for answer in answers {
        match answer.answer {
            Ok(a) => println!("Day {}{}: {} (runtime: {}s)",
                answer.day,
                match answer.part { Part::One => 'a', Part::Two => 'b' },
                a,
                formatted_duration(answer.runtime)),
            Err(e) => println!("(FAILED) Day {}{}: {}",
                answer.day,
                match answer.part { Part::One => 'a', Part::Two => 'b' },
                e),
        }

        runtime += answer.runtime;
    }

    println!("Total runtime: {}", formatted_duration(runtime));
}

enum Target {
    Single(usize),
    All,
}

impl Target {
    fn new(arg: &str) -> anyhow::Result<Target> {
        match arg.parse::<usize>() {
            Ok(n) if 1 <= n && n <= 25 => Ok(Target::Single(n)),
            Err(_) if arg == "*" => Ok(Target::All),
            _ => Err(anyhow::anyhow!("invalid argument: expected number between 1-25, or an asterisk (*) for 'all'"))
        }
    }

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

fn formatted_duration(duration: Duration) -> f32 {
    (duration.as_millis() as f32) / 1000.0
}