/// Read triangles horizontally; count possible ones.
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    Ok(parse(input)
        .ok_or(anyhow::anyhow!("failed parse"))?
        .into_iter()
        .filter(|t| t.0 + t.1 > t.2 && t.1 + t.2 > t.0 && t.2 + t.0 > t.1)
        .count()
        .to_string())
}

/// Read triangles vertically; count possible ones.
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    Ok(parse_vertical(input)
        .ok_or(anyhow::anyhow!("failed parse"))?
        .into_iter()
        .filter(|t| t.0 + t.1 > t.2 && t.1 + t.2 > t.0 && t.2 + t.0 > t.1)
        .count()
        .to_string())
}

/// Reads each line as a triangle.
fn parse(input: &[&str]) -> Option<Vec<(u32, u32, u32)>> {
    let mut result = vec![];
    for line in input {
        let mut items = line.split_whitespace().filter(|s| !s.is_empty());
        result.push((
            items.next()?.parse().ok()?,
            items.next()?.parse().ok()?,
            items.next()?.parse().ok()?,
        ));
    }
    Some(result)
}

/// Reads each vertical groups of 3 as a triangle.
fn parse_vertical(input: &[&str]) -> Option<Vec<(u32, u32, u32)>> {
    let mut result = vec![];
    for chunk in input.chunks(3) {
        let mut items: Vec<_> = chunk
            .into_iter()
            .map(|s| s.split_whitespace().filter(|s| !s.is_empty()))
            .collect();
        for _ in 0..3 {
            result.push((
                items[0].next()?.parse().ok()?,
                items[1].next()?.parse().ok()?,
                items[2].next()?.parse().ok()?,
            ));
        }
    }
    Some(result)
}
