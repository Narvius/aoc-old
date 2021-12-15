/// Find the cheapest possible win.
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    Ok(least_mana_win(parse(input).ok_or(anyhow::anyhow!("failed to parse input"))?, false)
        .to_string())
}

/// Find the cheapest possible win in hard mode.
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    Ok(least_mana_win(parse(input).ok_or(anyhow::anyhow!("failed to parse input"))?, true)
        .to_string())
}

/// Runs through all possible outcomes, and finds the cheapest win.
fn least_mana_win(initial: State, hard: bool) -> usize {
    let mut realities = vec![initial];
    let mut best_win = usize::MAX;

    while let Some(state) = realities.pop() {
        // If we ALREADY have more mana spent than the best solution, no need to keep going.
        if state.spent_mana > best_win {
            continue;
        }

        // If we're a win, note that down and stop.
        if state.enemy_health <= 0 {
            best_win = best_win.min(state.spent_mana);
            continue;
        }

        // Try casting each spell on a new timeline, and keep the ones that make sense (we didn't
        // die or try casting illegally).
        realities.extend(
            [
                Spell::MagicMissile,
                Spell::Drain,
                Spell::Shield,
                Spell::Poison,
                Spell::Recharge,
            ]
            .into_iter()
            .filter_map(|s| simulate_turn(state, s, hard))
        );
    }

    best_win
}

/// Simulates one full player turn and one full enemy turn, casting the provided `spell`. Returns
/// `None` if the outcome is bad (we died or cast something illegal), otherwise `Some(new state)`.
/// If `hard` is set, the player loses 1 HP at the beginning of each turn of their turns.
fn simulate_turn(mut state: State, spell: Spell, hard: bool) -> Option<State> {
    // Player turn.
    if hard {
        state.health -= 1;
        if state.health <= 0 {
            return None;
        }
    }

    run_effects(&mut state);
    if state.enemy_health <= 0 {
        return Some(state);
    }

    let success = match spell {
        Spell::MagicMissile => state.spend(53),
        Spell::Drain => state.spend(73),
        Spell::Shield => state.shield == 0 && state.spend(113),
        Spell::Poison => state.poison == 0 && state.spend(173),
        Spell::Recharge => state.recharge == 0 && state.spend(229),
    };

    if !success {
        return None;
    }

    match spell {
        Spell::MagicMissile => state.enemy_health -= 4,
        Spell::Drain => { state.enemy_health -= 2; state.health += 2; },
        Spell::Shield => state.shield = 6,
        Spell::Poison => state.poison = 6,
        Spell::Recharge => state.recharge = 5,
    }

    // Enemy turn.
    run_effects(&mut state);
    if state.enemy_health <= 0 {
        return Some(state);
    }

    let damage = state.enemy_damage - if state.shield > 0 { 7 } else { 0 };
    state.health -= damage;
    if state.health <= 0 {
        return None;
    }

    Some(state)
}

/// Runs one tick of each effect, applying their effects and reducing their duration.
fn run_effects(state: &mut State) {
    if state.shield > 0 {
        state.shield -= 1;
    }
    if state.poison > 0 {
        state.enemy_health -= 3;
        state.poison -= 1;
    }
    if state.recharge > 0 {
        state.mana += 101;
        state.recharge -= 1;
    }
}

/// An enumeration of spells available to the player.
enum Spell {
    MagicMissile,
    Drain,
    Shield,
    Poison,
    Recharge,
}

/// A battle state.
#[derive(Copy, Clone)]
struct State {
    health: i32,
    mana: usize,
    enemy_health: i32,
    enemy_damage: i32,
    poison: usize,
    shield: usize,
    recharge: usize,
    spent_mana: usize,
}

impl State {
    /// Attempts to spend `amount` mana, and returns whether it was successful.
    fn spend(&mut self, amount: usize) -> bool {
        if self.mana < amount {
            false
        } else {
            self.mana -= amount;
            self.spent_mana += amount;
            true
        }
    }
}

/// Parses the puzzle input into a valid initial battle state.
fn parse(input: &[&str]) -> Option<State> {
    Some(State {
        health: 50,
        mana: 500,
        enemy_health: input[0].split_once(": ")?.1.parse().ok()?,
        enemy_damage: input[1].split_once(": ")?.1.parse().ok()?,
        poison: 0,
        shield: 0,
        recharge: 0,
        spent_mana: 0,
    })
}
