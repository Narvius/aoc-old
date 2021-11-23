use std::collections::HashSet;

/// Find the number of distinct molecules that can be built with one step of expansion using any of
/// the available expansion rules.
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    let (replaces, molecule) = parse_input(input);
    let mut set = HashSet::new();
    for replace in replaces {
        for (index, slice) in molecule.match_indices(replace.0) {
            set.insert(format!("{}{}{}", &molecule[0..index], replace.1, &molecule[index + slice.len()..]));
        }
    }
    Ok(set.len().to_string())
}

/// Find the number of steps required to build the target molecule starting from `e`. Though the
/// implementation solves the equivalent problem of finding the number of steps required to
/// collapse the target molecule down to `e` using the inverse rules.
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    let structure = build_structural_view(input.last().unwrap());
    let content_atoms = structure.chars().filter(|&c| c == '.').count();
    let divider_atoms = structure.chars().filter(|&c| c == '|').count();
    Ok((content_atoms - divider_atoms - 1).to_string())
}

/// Reads the expansion rules and target molecule from puzzle input.
fn parse_input<'a>(input: &[&'a str]) -> (Vec<(&'a str, &'a str)>, &'a str) {
    let mut result = vec![];

    for &line in input.iter() {
        if line.contains("=>") {
            let mut split = line.split(" => ");
            result.push((split.next().unwrap(), split.next().unwrap()));
        } else if line.len() > 0 {
            return (result, line);
        }
    }

    unreachable!()
}

/// Builds a "structural representation" of a molecule by replacing content atoms with a `.`,
/// and the bracket atoms with `(`, `)` and `|`, respectively.
/// 
/// # Explanation
/// 
/// If you look closely at the puzzle input, it turns out there's two major groups of atoms:
/// - content atoms: can expand, but only in specific patterns;
/// - bracket atoms: can never expand, and always show up alongside each other.
///
/// Bracket atoms are `Rn`, `Ar` and `Y`. Content atoms are all remaining ones.
/// 
/// Now it turns out that if we ignore the specific atom, and only focus on the STRUCTURE of a
/// molecule, we can simply count atoms to find the number of steps required to collapse the entire
/// thing down to one element.
/// 
/// Let's take a simple example: `SiRnFYFAr`. For my input, it's actually a fixed recipe,
/// collapsing down to `Ca`. So, the amount of steps required to collapse down to 1 element is 1.
/// 
/// If you closely examine the list of recipes, there's only a fixed number of expansion patterns
/// that exist. These boil down into these categories:
/// 
/// ```
/// #1: 1 atom => 2 atoms
/// #2: 1 atom => (1 atom) Rn (1 atom) Ar
/// #3: 1 atom => (1 atom) Rn (1 atom) Y (1 atom) Ar
/// #4: 1 atom => (1 atom) Rn (1 atom) Y (1 atom) Y (1 atom) Ar
/// ```
/// 
/// Note that in this recipe list, ALL "1 atom" placeholders specifically refer to content atoms.
/// This can be observed by checking all recipes, and this holding true seems crucial to being able
/// to solve the puzzle, so I'd assume it holds true for other puzzle inputs, too.
/// 
/// So just looking at these categories of expansions, we can invert them to get 4 categories of
/// "collapses".
/// 
/// Based on collapse rule #1, we can take two adjacent content atoms and replace them with one
/// content atom, at the cost of 1 step. Extending this logic, we can collapse `n` content atoms
/// down to 1 via `n - 1` steps.
/// 
/// Based on collapse rule #2, we we can take a structure of the form `X Rn Y Ar`, and replace it
/// with Z, at the cost of 1 step. Essentially, we are once again replacing two content atoms with
/// one, whilst the bracket atoms don't influence the step count at all!
/// 
/// Looking at collapse rules #3 and #4, we notice that they replace (respectively) 3 or 4 content
/// atoms with 1. The differentiating factor is the amount of `Y` atoms. Each `Y` atom "accounts"
/// for one additional content atom.
/// 
/// Note that in all three rules that include bracket atoms, `Rn` always comes first, followed by
/// zero or more `Y` atoms, followed by a terminating `Ar` atom. This is why we will represent them
/// as left parenthesis, pipe and right parenthesis from now on.
/// 
/// Using those substitutions, as well as `.` for arbitrary content atoms, we get this "structural
/// view":
/// 
/// `SiRnFYFAr => .(.|.)`
/// 
/// Now, based on the facts derived from each collapse rule, we know that we can ignore bracket
/// atoms, we know that `Y` counts against the number of steps required, and that content atoms
/// count for the amount of steps required.
/// 
/// So we count: Each `.` is +1 to the number of steps required, each `|` is a -1 to the number of
/// steps required, and the required count starts at -1 to account for the fact that we need one
/// fewer steps than content atoms to collapse them down to one.
/// 
/// ```
/// 3 . => +3
/// 1 | => -1
/// 
/// -1 + 3 - 1 = 1 step
/// ```
/// 
/// And this is the algorithm used in the solution.
fn build_structural_view(s: &str) -> String {
    s.replace("Rn", "(").replace("Ar", ")").replace("Y", "|")
     .replace("Al", ".").replace("Ca", ".").replace("Mg", ".").replace("Si", ".").replace("Th", ".").replace("Ti", ".")
     .replace("B", ".").replace("F", ".").replace("P", ".").replace("C", ".")
}
