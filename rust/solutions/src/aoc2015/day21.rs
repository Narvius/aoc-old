use itertools::Itertools;

/// Find the least gold you can spend to win against the boss.
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    let boss = (
        input[0].split(' ').last().unwrap().parse().unwrap(),
        input[1].split(' ').last().unwrap().parse().unwrap(),
        input[2].split(' ').last().unwrap().parse().unwrap());

    Ok(all_loadouts()
        .map(resolve_loadout)
        .filter(|&(stats, _)| wins(stats, boss))
        .map(|(_, cost)| cost)
        .min().unwrap().to_string())
}

/// Find the most gold you can spend and still lose against the boss.
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    let boss = (
        input[0].split(' ').last().unwrap().parse().unwrap(),
        input[1].split(' ').last().unwrap().parse().unwrap(),
        input[2].split(' ').last().unwrap().parse().unwrap());

    Ok(all_loadouts()
        .map(resolve_loadout)
        .filter(|&(stats, _)| !wins(stats, boss))
        .map(|(_, cost)| cost)
        .max().unwrap().to_string())
}

/// Checks if `you` win against `enemy`.
fn wins(mut you: Stats, mut enemy: Stats) -> bool {
    loop {
        enemy.0 = enemy.0.saturating_sub(1.max(you.1.saturating_sub(enemy.2)));
        if enemy.0 == 0 { return true; }
        you.0 = you.0.saturating_sub(1.max(enemy.1.saturating_sub(you.2)));
        if you.0 == 0 { return false; }
    }
}

/// The HP, damage and defense of a fighter.
type Stats = (u32, u32, u32);
/// A tuple representing a possible weapon, armor, left ring and right ring choice.
type Loadout = (usize, usize, usize, usize);

/// An enumeration of all possible shop loadouts.
fn all_loadouts() -> impl Iterator<Item = Loadout> {
    // Since out-of-range indices are equivalent to "no item", we use ranges that are 1 too large
    // to also capture "no gear in this slot" in the iterator. The 'correct' way would be to use
    // Option<_>, but that's too much faffing about for this.
    [(0..5), (0..6), (0..7), (0..7)].iter().cloned().multi_cartesian_product().map(|v| (v[0], v[1], v[2], v[3]))
}

/// Given a shop loadout, gives you the stats it gives, as well as its cost.
fn resolve_loadout(loadout: Loadout) -> (Stats, u32) {
    static WEAPONS: &[(u32, u32, u32)] = &[(8, 4, 0), (10, 5, 0), (25, 6, 0), (40, 7, 0), (74, 8, 0)];
    static ARMORS: &[(u32, u32, u32)] = &[(13, 0, 1), (31, 0, 2), (53, 0, 3), (75, 0, 4), (102, 0, 5)];
    static RINGS: &[(u32, u32, u32)] = &[(25, 1, 0), (50, 2, 0), (100, 3, 0), (20, 0, 1), (40, 0, 2), (80, 0, 3)];

    let gear = [
        WEAPONS.get(loadout.0).copied().unwrap_or((0, 0, 0)),
        ARMORS.get(loadout.1).copied().unwrap_or((0, 0, 0)),
        RINGS.get(loadout.2).copied().unwrap_or((0, 0, 0)),
        // Shopkeep only has one of each item, so we can't buy two of the same ring.
        // If we try to, just use a nonsense index instead.
        RINGS.get(if loadout.2 == loadout.3 { 1000 } else { loadout.3 }).copied().unwrap_or((0, 0, 0)),
    ];

    let (gold, atk, def) = gear.iter().copied().reduce(|(a, b, c), (d, e, f)| (a + d, b + e, c + f)).unwrap();
    ((100, atk, def), gold)
}
