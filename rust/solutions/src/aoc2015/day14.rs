use regex::Regex;

/// Find the best result of the race.
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    Ok(input.iter().map(|&s| distance_after_time(parse_line(s).unwrap(), 2503)).max().unwrap().to_string())
}

/// Using a revised scoring system, find the best result of the race.
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    let reindeer_stats: Vec<_> = input.iter().map(|&s| parse_line(s).unwrap()).collect();
    Ok(points_after_time(&reindeer_stats, 2503).to_string())
}

/// Returns the distance after the given time for a reindeer.
fn distance_after_time((speed, travel_time, rest_time): (u32, u32, u32), seconds: u32) -> u32 {
    let cycles = seconds / (travel_time + rest_time);
    let leftover = seconds % (travel_time + rest_time);

    speed * travel_time * cycles + speed * std::cmp::min(travel_time, leftover)
}

/// Using the revised scoring system, returns the best score for a given herd of reindeer and a
/// specified time.
fn points_after_time(reindeer_stats: &[(u32, u32, u32)], seconds: u32) -> u32 {
    let mut scores = vec![0; reindeer_stats.len()];
    let mut distances = vec![0; reindeer_stats.len()];
    let mut cycle = vec![0; reindeer_stats.len()];
    let mut furthest = 0;

    for _ in 0..seconds {
        for i in 0..reindeer_stats.len() {
            let (speed, travel_time, rest_time) = reindeer_stats[i];
            if cycle[i] < travel_time {
                distances[i] += speed;
                furthest = std::cmp::max(distances[i], furthest);
            }

            cycle[i] = (cycle[i] + 1) % (travel_time + rest_time);
        }

        for i in 0..distances.len() {
            if distances[i] == furthest {
                scores[i] += 1;
            }
        }
    }

    scores.iter().copied().max().unwrap()
}

/// Extracts the speed, travel time and rest time from a line of puzzle input.
fn parse_line(s: &str) -> Option<(u32, u32, u32)> {
    lazy_static::lazy_static! {
        static ref RE: Regex = Regex::new(r"\d+").unwrap();
    }

    let mut matches = RE.find_iter(s);
    let speed = matches.next().and_then(|m| m.as_str().parse().ok())?;
    let travel_time = matches.next().and_then(|m| m.as_str().parse().ok())?;
    let rest_time = matches.next().and_then(|m| m.as_str().parse().ok())?;

    Some((speed, travel_time, rest_time))
}
