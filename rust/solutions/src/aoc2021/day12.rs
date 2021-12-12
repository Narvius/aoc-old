/// Find the number of paths through the caves where each big cave can be visited any number of
/// times, but each small cave can only be visited once.
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    Ok(Graph::from_input(input).paths(false).to_string())
}

/// Like part 1, except one single small cave can be visited twice within a path.
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    Ok(Graph::from_input(input).paths(true).to_string())
}

/// Adjacency list-based graph.
struct Graph<'a> {
    nodes: Vec<&'a str>,
    edges: Vec<Vec<usize>>,
    start: usize,
    end: usize,
}

impl<'a> Graph<'a> {
    /// Parses the puzzle input into a graph.
    fn from_input(input: &[&'a str]) -> Graph<'a> {
        fn index<T: PartialEq>(v: &Vec<T>, item: &T) -> usize {
            v.iter().position(|t| t == item).unwrap()
        }

        let mut nodes = vec![];
        let mut edges = vec![];
        for &line in input {
            let (a, b) = line.split_once('-').unwrap();
            for node in [a, b] {
                if !nodes.contains(&node) {
                    nodes.push(node);
                    edges.push(vec![]);
                }
            }
            edges[index(&nodes, &a)].push(index(&nodes, &b));
            edges[index(&nodes, &b)].push(index(&nodes, &a));
        }

        let (start, end) = (index(&nodes, &"start"), index(&nodes, &"end"));
        Graph { nodes, edges, start, end }
    }

    /// Counts the number of paths that can be taken through the graph, according to the rules of
    /// the puzzle. That is, each uppercase-labelled node can be visited any number of times, but
    /// each lowercase-labelled node can only be visited once.
    /// If `lowercase_grace` is set, then a single lowercase-labelled node can be visited a second
    /// time within a path.
    fn paths(&self, lowercase_grace: bool) -> u32 {
        // Recursively finds all paths. `open` and `grace` keep track of which nodes can be visited.
        fn sub_paths(g: &Graph, node: usize, open: &[bool], grace: bool) -> u32 {
            let mut paths = 0;
            for &target in &g.edges[node] {
                paths += match (open[target], grace) {
                    // Reaching either 'start' or 'end' with the path terminates it.
                    _ if target == g.start => 0,
                    _ if target == g.end => 1,
                    // Small, open cave. Have to copy the "open" table, with the corresponding entry
                    // set to false.
                    (true, _) if (b'a'..=b'z').contains(&g.nodes[target].as_bytes()[0]) => {
                        let mut next_open = open.to_vec();
                        next_open[target] = false;
                        sub_paths(g, target, &next_open, grace)
                    },
                    // Big, open cave. No need to modify state.
                    (true, _) => sub_paths(g, target, open, grace),
                    // Small, closed cave, but we have a grace. Spend the grace.
                    (_, true) => sub_paths(g, target, open, false),
                    // Small, closed cave. Terminates the path.
                    _ => 0,
                };
            }
            paths
        }
        
        sub_paths(self, self.start, &vec![true; self.nodes.len()], lowercase_grace)
    }
}
