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
            if !nodes.contains(&a) {
                nodes.push(a);
            }
            if !nodes.contains(&b) {
                nodes.push(b);
            }
            let ap = nodes.iter().position(|&x| x == a).unwrap();
            let bp = nodes.iter().position(|&x| x == b).unwrap();
            edges[ap].push(bp);
            edges[bp].push(ap);
        }

        let start = nodes.iter().position(|&x| x == "start").unwrap();
        let end = nodes.iter().position(|&x| x == "end").unwrap();

        Graph {
            nodes,
            edges,
            start,
            end
        }
    }

    /// Counts the number of paths that can be taken through the graph, according to the rules of
    /// the puzzle. That is, each uppercase-labelled node can be visited any number of times, but
    /// each lowercase-labelled node can only be visited once.
    /// If `lowercase_grace` is set, then a single lowercase-labelled node can be visited a second
    /// time within a path.
    fn paths(&self, lowercase_grace: bool) -> u32 {
        // Recursively finds all paths. `open` and `grace` keep track of which nodes can be visited.
        fn sub_paths<'a>(g: &Graph<'a>, node: usize, open: &[bool], grace: bool) -> u32 {
            if node == g.end {
                return 1;
            }

            let mut paths = 0;
            for &target in &g.edges[node] {
                if open[target] || (grace && target != g.start) {
                    let mut next_open = open.into_iter().copied().collect::<Vec<_>>();
                    if g.nodes[target].chars().next().unwrap().is_ascii_lowercase() {
                        next_open[target] = false;
                    }
                    paths += sub_paths(g, target, &next_open, grace && open[target]);
                }
            }

            paths
        }
        
        let open: Vec<_> = (0..self.nodes.len()).map(|i| self.nodes[i] != "start").collect();
        sub_paths(self, self.start, &open, lowercase_grace)
    }
}