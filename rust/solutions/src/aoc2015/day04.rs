/// Find the lowest i for which md5("{input}{i}") starts with at least 5 zeroes.
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    for i in 1.. {
        let hash = md5::compute(format!("{}{}", input[0], i)).0;
        // 5 hexadecimal digits means the first 20 bits, which is checked for here in a slightly
        // unintuitive fashion.
        if hash[0] == 0 && hash[1] == 0 && hash[2] < 16u8 {
            return Ok(i.to_string())
        }
    }

    unreachable!()
}

/// Find the lowest i for which md5("{input}{i}") starts with at least 6 zeroes.
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    for i in 1.. {
        let hash = md5::compute(format!("{}{}", input[0], i)).0;
        // 6 hexadecimal digits is 24 bits, so much easier to check for.
        if hash[0] == 0 && hash[1] == 0 && hash[2] == 0 {
            return Ok(i.to_string())
        }
    }

    // Note: This runs quite slowly. There must probably be a more efficient way than brute-force.
    // May be interesting to research what it is at some point.

    unreachable!()
}
