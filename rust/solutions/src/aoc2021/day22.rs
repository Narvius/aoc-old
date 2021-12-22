/// Finds the number of lit points after running all initialization instructions from the input;
/// that is, instructions that are within 50 points of the origin in all directions.
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    part(input, true)
}

/// Finds the number of lit points after running all instructions from the input.
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    part(input, false)
}

/// Finds the number of lit points after executing all (relevant) instructions from the `input`.
/// `initialization_only` decides which instructions are relevant, as per puzzle description.
fn part(input: &[&str], initialization_only: bool) -> anyhow::Result<String> {
    let mut diffs: Vec<Diff> = vec![];
    let mut to_add: Vec<Diff> = vec![];
    let mut v = parse(input)
        .ok_or_else(|| anyhow::anyhow!("faile to parse input"))?;
    
    // Filter out instructions not relevant to this run.
    v.retain(|i| !initialization_only || i.is_initialization());

    for Instruction { add, cube } in v {
        // Whenever we overlap with any cube, we have to add a diff with the opposing polarity.
        // This happens regardless of the polarity of the instruction--if we have matching
        // polarity, we need to compensate to not count the overlap twice; if we have opposite
        // polarity, we need to explicitly count the overlap. Either way this handles both cases.
        for diff in &diffs {
            if let Some(shared) = cube.intersection(diff.cube) {
                to_add.push(Diff { positive: !diff.positive, cube: shared });
            }
        }

        diffs.extend(to_add.drain(..));

        // "on" instructions from the input also just get added to the list, straight.
        if add {
            diffs.push(Diff { positive: add, cube });
        }
    }

    Ok(diffs
        .into_iter()
        .fold(0i64, |acc, d| acc + d.cube.point_count() * if d.positive { 1 } else { -1 })
        .to_string())
}

/// Parses the puzzle input into a list of instructions.
fn parse(input: &[&str]) -> Option<Vec<Instruction>> {
    Some(input
        .into_iter()
        .map(|s| s.parse())
        .collect::<Result<Vec<_>, _>>()
        .ok()?)
}

/// A cube annotated with whether it's something to add or remove in the final count.
#[derive(Copy, Clone, Debug, Eq, Hash, PartialEq)]
struct Diff {
    positive: bool,
    cube: Cube,
}

/// Contains the bounds of a cube, start-inclusive and end-exclusive. Note that the puzzle input
/// is inclusive on both ends--this is compensated for during parsing.
#[derive(Copy, Clone, Debug, Eq, Hash, PartialEq)]
struct Cube {
    x: (i32, i32),
    y: (i32, i32),
    z: (i32, i32),
}

impl Cube {
    /// Finds the intersection between two cubes, if it exists.
    fn intersection(self, other: Cube) -> Option<Cube> {
        fn interval_intersect((xs, xe): (i32, i32), (ys, ye): (i32, i32)) -> Option<(i32, i32)> {
            if ys > xe || xs > ye {
                None
            } else {
                Some((xs.max(ys), xe.min(ye)))
            }
        }

        Some(Cube {
            x: interval_intersect(self.x, other.x)?,
            y: interval_intersect(self.y, other.y)?,
            z: interval_intersect(self.z, other.z)?,
        })
    }

    /// Returns the number of points in this cube.
    fn point_count(self) -> i64 {
        let x = (self.x.1 - self.x.0) as i64;
        let y = (self.y.1 - self.y.0) as i64;
        let z = (self.z.1 - self.z.0) as i64;
        x * y * z
    }
}

/// An instruction from the puzzle input.
#[derive(Copy, Clone, Debug, Eq, Hash, PartialEq)]
struct Instruction {
    add: bool,
    cube: Cube,
}

impl Instruction {
    /// Checks whether the instruction is an "initialization" instruction; that is, within 50
    /// points of origin in all directions.
    fn is_initialization(&self) -> bool {
        let Cube { x: (x0, x1), y: (y0, y1), z: (z0, z1) } = self.cube;
        [x0, x1, y0, y1, z0, z1]
            .into_iter()
            .all(|c| -50 <= c && c <= 50)
    }
}

impl std::str::FromStr for Instruction {
    type Err = ();

    fn from_str(s: &str) -> Result<Self, Self::Err> {
        fn to_tuple(s: &str) -> Option<(i32, i32)> {
            let (a, b) = s.split_once("..")?;
            Some((a.parse().ok()?, b.parse::<i32>().ok()? + 1))
        }

        let mut data = s.split(&[' ', '=', ','][..]);
        let add = data.next().map(|s| s == "on").ok_or(())?;
        data.next();
        let x = data.next().and_then(to_tuple).ok_or(())?;
        data.next();
        let y = data.next().and_then(to_tuple).ok_or(())?;
        data.next();
        let z = data.next().and_then(to_tuple).ok_or(())?;

        Ok(Self {
            add,
            cube: Cube {
                x,
                y,
                z,
            }
        })
    }
}
