use std::collections::HashMap;

/// Expand the polymer 10 times, get the difference in occurences of the most common and the least
/// common byte.
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    let (polymer, pairs) = parse(input).ok_or(anyhow::anyhow!("failed to parse input"))?;
    Ok(score_after_steps(polymer, &pairs, 10).to_string())
}

/// Expand the polymer 40 times, get the difference in occurences of the most common and the least
/// common byte.
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    let (polymer, pairs) = parse(input).ok_or(anyhow::anyhow!("failed to parse input"))?;
    Ok(score_after_steps(polymer, &pairs, 40).to_string())
}

/// Expands the `polymer` by the given amount of `steps`, using `rules`; then returns the difference
/// in quantity of the most common byte and the least common byte in the output.
fn score_after_steps(polymer: &[u8], rules: &HashMap<(u8, u8), u8>, steps: usize) -> usize {
    let mut counts = HashMap::new();
    let mut pairs = HashMap::new();

    // Count the actual bytes in the input polymer.
    for &c in polymer {
        *counts.entry(c).or_insert(0) += 1;
    }

    // Find pairs that are in the input polymer.
    for w in polymer.windows(2) {
        *pairs.entry((w[0], w[1])).or_insert(0) += 1;
    }

    // Iteratively construct new pair lists until all steps are done.
    for _ in 0..steps {
        pairs = {
            let mut temp = HashMap::new();
            // There are `v` of this pair. That means `v` of a new letter will spawn, and `v`
            // of two new pairs will be formed. Count all of those.
            for (&(c1, c2), &v) in &pairs {
                let new = *rules.get(&(c1, c2)).unwrap();
                *counts.entry(new).or_insert(0) += v;
                *temp.entry((c1, new)).or_insert(0) += v;
                *temp.entry((new, c2)).or_insert(0) += v;
            }
            temp
        }
    }

    counts.values().max().unwrap() - counts.values().min().unwrap()
}

/// Expands the `polymer` by the given amount of `steps`, using `rules`; then returns the difference
/// in quantity of the most common byte and the least common byte in the output.
#[allow(dead_code)]
fn score_after_steps_old(polymer: &[u8], rules: &HashMap<(u8, u8), u8>, steps: usize) -> usize {
    // Counts the characters resulting from expanding one `pair`, `steps` amount of times. The
    // result is stored in `counts`. This is hugely recursive, and results for computed subtrees
    // are memoized into `cache`.
    fn recurse(steps: usize, pair: (u8, u8), rules: &HashMap<(u8, u8), u8>, counts: &mut HashMap<u8, usize>, cache: &mut HashMap<((u8, u8), usize), HashMap<u8, usize>>) {
        // Recursion base case check.
        if steps > 0 {
            // As a result of this call, one new byte is spawned. Count it.
            let new = *rules.get(&pair).unwrap();
            *counts.entry(new).or_insert(0) += 1;

            // There's two new pairs we need to handle.
            for pair in [(pair.0, new), (new, pair.1)] {
                // Result is not memoized. So, precompute it and store it in the memoization cache.
                if !cache.contains_key(&(pair, steps - 1)) {
                    let mut cached = HashMap::new();
                    recurse(steps - 1, pair, rules, &mut cached, cache);
                    cache.insert((pair, steps - 1), cached);
                }

                // Add the counts in the cache for this call to the overall counts.
                let sub_counts = cache.get(&(pair, steps - 1)).unwrap();
                for (&key, &value) in sub_counts.iter() {
                    *counts.entry(key).or_insert(0) += value;
                }
            }
        }
    }

    let mut cache: HashMap<((u8, u8), usize), HashMap<u8, usize>> = HashMap::new();
    let mut counts = HashMap::new();
    // Count the actual bytes in the input polymer.
    for &c in polymer {
        *counts.entry(c).or_insert(0) += 1;
    }
    // Put initial conditions in the memoization cache.
    for (&key, &value) in rules {
        let mut map = HashMap::new();
        map.insert(value, 1);
        cache.insert((key, 1), map);
    }
    // For each pair, count everything in the expanded tree.
    for window in polymer.windows(2) {
        recurse(steps, (window[0], window[1]), rules, &mut counts, &mut cache);
    }

    counts.values().max().unwrap() - counts.values().min().unwrap()
}

/// Parses the puzzle input into an input polymer and an expansion rule map.
fn parse<'a>(input: &[&'a str]) -> Option<(&'a [u8], HashMap<(u8, u8), u8>)> {
    let mut lines = input.into_iter();
    let polymer = lines.next().unwrap().as_bytes();
    let mut pairs = HashMap::new();

    for line in lines {
        if line.contains(" -> ") {
            let (pair, new) = line.split_once(" -> ")?;
            pairs.insert((pair.as_bytes()[0], pair.as_bytes()[1]), new.as_bytes()[0]);
        }
    }

    Some((polymer, pairs))
}