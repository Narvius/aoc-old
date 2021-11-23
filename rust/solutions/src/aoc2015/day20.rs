/// Find the earliest house that gets enough presents. In this incarnation, it's equivalent to
/// finding the lowest number that has a high enough sum of proper factors.
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    // I tried to do this a clever way via prime factorization or w/e, but this is fast enough for
    // how trivial it is to implement.

    let mut cache = vec![0; 1_000_000];
    let target = input[0].parse::<i64>().unwrap();

    for n in 1..cache.len() {
        for d in 1.. {
            if let Some(p) = cache.get_mut(n * d) {
                *p += 10 * n as i64;
            } else {
                break;
            }
        }
    }

    Ok(cache.iter().enumerate().filter(|(_, &v)| v >= target).next().unwrap().0.to_string())
}

/// Find the earliest house that gets enough presents; but additional limitations no longer allow
/// it to be abstracted to a nice mathematical problems.
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    let mut cache = vec![0; 1_000_000];
    let target = input[0].parse::<i64>().unwrap();
    
    for n in 1..cache.len() {
        for d in 1..50 {
            if let Some(p) = cache.get_mut(n * d) {
                *p += 11 * n as i64;
            } else {
                break;
            }
        }
    }

    Ok(cache.iter().enumerate().filter(|(_, &v)| v >= target).next().unwrap().0.to_string())
}
