use std::collections::VecDeque;

/// Find the Bingo board that wins first, and find its score at the time of the win.
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    let mut bingo = Bingo::from_input(input);
    while let Some(number) = bingo.draw_number() {
        if let Some(board) = bingo.find_finished_board() {
            let sum: u32 = bingo.boards[board].iter().filter_map(|&f| f).sum();
            return Ok((sum * number).to_string());
        }
    }

    Err(anyhow::anyhow!("unreachable"))
}

/// Find the Bingo board that wins last, and find its score at the time of the win.
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    let mut bingo = Bingo::from_input(input);
    while let Some(number) = bingo.draw_number() {
        while let Some(board) = bingo.find_finished_board() {
            if bingo.boards.len() == 1 {
                let sum: u32 = bingo.boards[board].iter().filter_map(|&f| f).sum();
                return Ok((sum * number).to_string());
            } else {
                bingo.boards.remove(board);
            }
        }
    }

    Err(anyhow::anyhow!("unreachable"))
}

/// Represents the whole Bingo game described by `input`.
struct Bingo {
    draws: VecDeque<u32>,
    boards: Vec<Vec<Option<u32>>>,
}

impl Bingo {
    /// Parses `input` into a `Bingo` game.
    fn from_input(input: &[&str]) -> Bingo {
        let draws: VecDeque<_> = input[0].split(',').map(|s| s.parse().unwrap()).collect();
        let mut boards = vec![Vec::with_capacity(25); input.len() / 6];

        for i in 0..boards.capacity() {
            boards[i].extend(input[(2 + i * 6)..(7 + i * 6)].iter()
                .flat_map(|s| s.split(' ').filter_map(|t| (t.len() > 0).then(|| t.parse().ok()))));
        }

        Bingo {
            draws,
            boards,
        }
    }
    
    /// Draws a number, marks it off on all boards, and returns it. If there are no more numbers to
    /// be drawn, returns `None`.
    fn draw_number(&mut self) -> Option<u32> {
        self.draws.pop_front().map(|number| {
            for value in self.boards.iter_mut().flat_map(|b| b.iter_mut()) {
                if value.is_some() && number == value.unwrap() {
                    *value = None;
                }
            }

            number
        })
    }

    /// Returns the index of the first board that's finished, or `None` if there are none yet.
    fn find_finished_board(&self) -> Option<usize> {
        for i in 0..self.boards.len() {
            let marked = |&n: &usize| self.boards[i][n].is_none();

            for d in 0..5 {
                // horizontal
                if [0 + 5 * d, 1 + 5 * d, 2 + 5 * d, 3 + 5 * d, 4 + 5 * d].iter().all(marked) {
                    return Some(i);
                }
                // vertical
                if [0 + d, 5 + d, 10 + d, 15 + d, 20 + d].iter().all(marked) {
                    return Some(i);
                }
            }
        }

        None
    }
}