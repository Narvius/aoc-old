use std::{fs::File, io::BufRead, time::Instant};
use anyhow::{Result, bail};

pub enum Part {
    One,
    Two,
}

pub struct Answer {
    pub day: usize,
    pub part: Part,
    pub answer: Result<String>,
    pub runtime: std::time::Duration,
}

pub trait Solution {
    fn data_file_name() -> &'static str;
    fn part1(input: &[&str], output: &mut String) -> Result<()>;
    fn part2(input: &[&str], output: &mut String) -> Result<()>;
}

pub fn run_solution(index: usize) -> Result<(Answer, Answer)> {
    Ok(match index {
        //1 => run(day01::Day01, 1),
        //2 => run(day02::Day02, 2),
        //3 => run(day03::Day03, 3),
        //4 => run(day04::Day04, 4),
        //5 => run(day05::Day05, 5),
        //6 => run(day06::Day06, 6),
        //7 => run(day07::Day07, 7),
        //8 => run(day08::Day08, 8),
        //9 => run(day09::Day09, 9),
        //10 => run(day10::Day10, 10),
        //11 => run(day11::Day11, 11),
        //12 => run(day12::Day12, 12),
        //13 => run(day13::Day13, 13),
        //14 => run(day14::Day14, 14),
        //15 => run(day15::Day15, 15),
        //16 => run(day16::Day16, 16),
        //17 => run(day17::Day17, 17),
        //18 => run(day18::Day18, 18),
        //19 => run(day19::Day19, 19),
        //20 => run(day20::Day20, 20),
        //21 => run(day21::Day21, 21),
        //22 => run(day22::Day22, 22),
        //23 => run(day23::Day23, 23),
        //24 => run(day24::Day24, 24),
        //25 => run(day25::Day25, 25),
        n if 1 <= n && n <= 25 => bail!("no solution yet"),
        _ => bail!("outside of valid range (1 to 25, inclusive)"),
    })
}

fn run<T: Solution>(_solution: T, number: usize) -> (Answer, Answer) {
    let (mut p1, mut p2) = (String::new(), String::new());
    let start = Instant::now();

    let raw_input: Vec<_> = {
        let file = File::open(format!("data/{}", <T as Solution>::data_file_name()))
            .expect(&format!("failed to open data file '{}'", <T as Solution>::data_file_name()));

        std::io::BufReader::new(file).lines().map(|l| l.unwrap()).collect()
    };

    let ref_input: Vec<_> = raw_input.iter().map(|s| &**s).collect();

    let p1_answer = <T as Solution>::part1(&ref_input, &mut p1);
    let p1_finished = Instant::now();
    
    let p2_answer = <T as Solution>::part2(&ref_input, &mut p2);
    let p2_finished = Instant::now();

    (Answer {
        day: number,
        part: Part::One,
        answer: p1_answer.and(Ok(p1)),
        runtime: p1_finished.duration_since(start),
    },
    Answer {
        day: number,
        part: Part::Two,
        answer: p2_answer.and(Ok(p2)),
        runtime: p2_finished.duration_since(p1_finished),
    })
}