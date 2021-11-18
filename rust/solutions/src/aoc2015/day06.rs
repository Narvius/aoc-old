use regex::Regex;
use std::ops::Range;

/// Use instructions to turn on/off or toggle blocks of lights. Count the amount of lights that
/// are on.
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    let mut lights = vec![false; 1_000_000];
    for instruction in input.iter().map(|line| parse_line(line).unwrap()) {
        apply_instruction(&mut lights, instruction, |val, prev| match val {
            Some(b) => b,
            None => !prev,
        });
    }

    Ok(lights.iter().filter(|&&x| x).count().to_string())
}

/// Use instructions to manipulate the brightness of lights.
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    let mut lights = vec![0u64; 1_000_000];
    for instruction in input.iter().map(|line| parse_line(line).unwrap()) {
        apply_instruction(&mut lights, instruction, |val, prev| match val {
            Some(true) => prev + 1,
            Some(false) => prev.saturating_sub(1),
            None => prev + 2,
        });
    }

    Ok(lights.iter().sum::<u64>().to_string())
}

/// For all lights mentioned in the instruction, replaces their value with the result of calling
/// morph with the instruction and their old value.
fn apply_instruction<E: Copy>(lights: &mut Vec<E>, instruction: Instruction, morph: fn(Option<bool>, E) -> E) {
    let Instruction { val, xs, ys } = instruction;

    for y in ys {
        for x in xs.clone() {
            let idx = 1000 * y + x;
            lights[idx] = morph(val, lights[idx]);
        }
    }
}

/// Produces an [`Instruction`] from a line of puzzle input.
fn parse_line(line: &str) -> Option<Instruction> {
    lazy_static::lazy_static! {
        static ref INSTRUCTION: Regex = Regex::new(r"^(turn on|turn off|toggle) (\d+),(\d+) through (\d+),(\d+)$").unwrap();
    }

    INSTRUCTION.captures(line).map(|captures| {
        let val = match captures.get(1).unwrap().as_str() {
            "turn on" => Some(true),
            "turn off" => Some(false),
            "toggle" => None,
            _ => unreachable!(), // only the three cases above are ever produced by the regex
        };
        let left = captures.get(2).and_then(|c| c.as_str().parse().ok()).unwrap();
        let top = captures.get(3).and_then(|c| c.as_str().parse().ok()).unwrap();
        let right = 1 + captures.get(4).and_then(|c| c.as_str().parse::<usize>().ok()).unwrap();
        let bottom = 1 + captures.get(5).and_then(|c| c.as_str().parse::<usize>().ok()).unwrap();
        
        Instruction { val, xs: left..right, ys: top..bottom, }
    })
}

/// An instruction from the puzzle input.
struct Instruction {
    /// The actual command; `turn on/off` is marked as `Some(true/false)`, while `toggle` is `None`.
    val: Option<bool>,
    /// The x coordinates affected by the instruction.
    xs: Range<usize>,
    /// The y coordinates affected by the instruction.
    ys: Range<usize>,
}