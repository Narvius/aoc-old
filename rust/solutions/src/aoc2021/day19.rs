use itertools::Itertools;
use std::collections::HashSet;

/// Find the number of beacons.
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    let (map, _) = combine_into_one_map(
        parse(input).ok_or(anyhow::anyhow!("failed to parse input"))?
    );
    Ok(map.len().to_string())
}

/// Find the manhattan distance bewteen the two furthest-away scanners.
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    let (_, scanners) = combine_into_one_map(
        parse(input).ok_or(anyhow::anyhow!("failed to parse input"))?
    );
    Ok(scanners.iter()
        .cartesian_product(&scanners)
        .map(|((ax, ay, az), (bx, by, bz))| (ax - bx).abs() + (ay - by).abs() + (az - bz).abs())
        .max()
        .unwrap()
        .to_string())
}

/// Given the parsed puzzle input, returns the full combined map, as well as the positions of all
/// scanners.
fn combine_into_one_map((mut found, mut uncertain): (Vec<Map>, Vec<Map>)) -> (Map, Vec<(i32, i32, i32)>) {
    let mut f = 0;
    let mut positions = vec![(0, 0, 0)];
    while uncertain.len() > 0 {
        let mut u = 0;
        while u < uncertain.len() {
            if let Some((map, p)) = detect_match(&found[f], &uncertain[u]) {
                found.push(map);
                positions.push(p);
                uncertain.remove(u);
                continue;
            }
            u += 1;
        }
        f += 1;
    }

    let mut map = Map::new();
    for f in found {
        map.extend(f);
    }
    (map, positions)
}

/// Detects whether the given fixed scanner, and the provided uncertain scanner overlap. If so,
/// returns a map from the uncertain scanner reoriented to the point of view of the certain scanner,
/// as well as the position of the uncertain scanner.
fn detect_match(fixed: &Map, uncertain: &Map) -> Option<(Map, (i32, i32, i32))> {
    for transform in Transform::iter_all() {
        // Reorient the uncertain map to the given transform.
        let map: HashSet<_> = uncertain
            .into_iter()
            .map(|&p| transform.translate_into(Transform::default(), p))
            .collect();

        // Produce all possible positions for the uncertain scanner.
        let positions: HashSet<_> = fixed
            .into_iter()
            .cartesian_product(&map)
            .map(|((ax, ay, az), (bx, by, bz))| (ax - bx, ay - by, az - bz))
            .collect();

        // Try all positions; if there's enough overlaps, that's our result!
        for (x, y, z) in positions {
            let mut count = 0;
            for &(px, py, pz) in &map {
                if fixed.contains(&(px + x, py + y, pz + z)) {
                    count += 1;
                }
            }

            if count >= 12 {
                return Some((
                    map.into_iter().map(|(px, py, pz)| (px + x, py + y, pz + z)).collect(),
                    (x, y, z),
                ));
            }
        }
    }
    None
}

type Map = HashSet<(i32, i32, i32)>;

/// Parses the puzzle input into scanner maps, treating the 0th scanner as "certain" and the others
/// as "uncertain" with regards to their transform. The 0th scanner has the default transform.
fn parse(input: &[&str]) -> Option<(Vec<Map>, Vec<Map>)> {
    let mut blocks = input.split(|line| line.len() == 0);
    let mut map = Map::new();
    for line in blocks.next()?.into_iter().skip(1) {
        let mut cs = line.split(',');
        map.insert((cs.next()?.parse().ok()?, cs.next()?.parse().ok()?, cs.next()?.parse().ok()?));
    }

    let mut uncertains = Vec::with_capacity(blocks.size_hint().0);
    for block in blocks {
        let mut map = Map::new();
        for line in block.into_iter().skip(1) {
            let mut cs = line.split(',');
            map.insert((cs.next()?.parse().ok()?, cs.next()?.parse().ok()?, cs.next()?.parse().ok()?));
        }
        uncertains.push(map);
    }

    Some((vec![map], uncertains))
}

/// Describes the "tranform" of a scanner, influencing how it interprets positions. There's 6
/// facings, and 4 orientations per facing.
/// ```text
/// Facings = ((negative, positive) x (x, y, z))
/// Orientation = ((negative, positive) x (x, y, z minus the one in facing))
/// ```
#[derive(Copy, Clone, Eq, Hash, PartialEq)]
struct Transform {
    facing: (i32, i32, i32),
    orientation: (i32, i32, i32),
}

impl Default for Transform {
    fn default() -> Self {
        Self {
            facing: (1, 0, 0),
            orientation: (0, 0, 1),
        }
    }
}

impl Transform {
    /// Returns an iterator over all 24 possible transforms.
    fn iter_all() -> impl Iterator<Item = Transform> {
        fn same_axis((x1, y1, z1): (i32, i32, i32), (x2, y2, z2): (i32, i32, i32)) -> bool {
            (x1 != 0 && x2 != 0) || (y1 != 0 && y2 != 0) || (z1 != 0 && z2 != 0)
        }
        
        let axes = [(1, 0, 0), (0, 1, 0), (0, 0, 1), (-1, 0, 0), (0, -1, 0), (0, 0, -1)];
        axes
            .clone()
            .into_iter()
            .cartesian_product(axes)
            .filter_map(|(v1, v2)| (!same_axis(v1, v2)).then(|| Transform {
                facing: v1,
                orientation: v2,
            }))
    }

    /// Translates a point from one transform into another.
    fn translate_into(&self, target: Transform, mut point: (i32, i32, i32)) -> (i32, i32, i32) {
        let mut transform = *self;

        fn rotate(axis: Axis, transform: &mut Transform, point: &mut (i32, i32, i32)) {
            transform.facing = axis.rotate_right(transform.facing);
            transform.orientation = axis.rotate_right(transform.orientation);
            *point = axis.rotate_right(*point);
        }

        // step 1: rotate facing so it matches the target. If it already matches, we do nothing;
        // otherwise we rotate right once around the third axis.
        if Axis::from(transform.facing) != Axis::from(target.facing) {
            let axis = Axis::from(self.facing).other(Axis::from(target.facing));
            rotate(axis, &mut transform, &mut point);
        }

        // step 2: if the facing is on the right axis but flipped around wrong, we need to rotate
        // twice around any of the other axes.
        if transform.facing != target.facing {
            let axis = Axis::from(transform.facing).next();
            rotate(axis, &mut transform, &mut point);
            rotate(axis, &mut transform, &mut point);
        }

        // step 3: facing now matches correctly. The only thing left is to rotate around the facing
        // axis until the orientation agrees as well.
        let axis = Axis::from(transform.facing);
        while transform.orientation != target.orientation {
            rotate(axis, &mut transform, &mut point);
        }

        point
    }
}

#[derive(Copy, Clone, Eq, Hash, PartialEq)]
enum Axis {
    X,
    Y,
    Z,
}

impl Axis {
    fn from(p: (i32, i32, i32)) -> Axis {
        match p {
            (1, 0, 0) | (-1, 0, 0) => Axis::X,
            (0, 1, 0) | (0, -1, 0) => Axis::Y,
            (0, 0, 1) | (0, 0, -1) => Axis::Z,
            _ => panic!("({}, {}, {}) is not an axis", p.0, p.1, p.2),
        }
    }

    /// Rotates a vector (point) around the given axis to the right by 90 degrees.
    fn rotate_right(self, (x, y, z): (i32, i32, i32)) -> (i32, i32, i32) {
        match self {
            Axis::X => ( x, -z,  y),
            Axis::Y => ( z,  y, -x),
            Axis::Z => ( y, -x,  z),
        }
    }

    /// Returns the next axis after this one.
    fn next(self) -> Axis {
        match self {
            Axis::X => Axis::Y,
            Axis::Y => Axis::Z,
            Axis::Z => Axis::X,
        }
    }

    /// Returns an axis that is not equal to `a1` or `a2`. If there's multiple options, the answer
    /// is the first one from the list `[x, y, z]` that works.
    fn other(self, a: Axis) -> Axis {
        match (self, a) {
            (Axis::X, Axis::X) => Axis::Y,
            (Axis::X, Axis::Y) => Axis::Z,
            (Axis::X, Axis::Z) => Axis::Y,
            (Axis::Y, Axis::X) => Axis::Z,
            (Axis::Y, Axis::Y) => Axis::X,
            (Axis::Y, Axis::Z) => Axis::X,
            (Axis::Z, Axis::X) => Axis::Y,
            (Axis::Z, Axis::Y) => Axis::X,
            (Axis::Z, Axis::Z) => Axis::X,
        }
    }
}
