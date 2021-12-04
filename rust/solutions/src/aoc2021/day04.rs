use std::collections::VecDeque;

/// Find the Bingo board that wins first, and find its score at the time of the win.
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    let mut bingo = Bingo::from_input(input);
    while let Some(number) = bingo.draw_number() {
        if let Some(board) = bingo.find_finished_board() {
            let sum: u32 = bingo.boards[board].iter().filter(|f| !f.0).map(|f| f.1).sum();
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
                let sum: u32 = bingo.boards[board].iter().filter(|f| !f.0).map(|f| f.1).sum();
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
    boards: Vec<Vec<(bool, u32)>>,
}

impl Bingo {
    /// Parses `input` into a `Bingo` game.
    fn from_input(input: &[&str]) -> Bingo {
        let mut it = input.iter().map(|&s| s);

        let draws: VecDeque<_> = it.next().unwrap().split(',')
            .map(|s| s.parse::<u32>().unwrap()).collect();
        let mut boards = vec![];

        while let Some(_) = it.next() {
            let mut board = Vec::with_capacity(25);
            for _ in 0..5 {
                board.extend(it.next().unwrap().split(' ').filter(|s| s.len() > 0)
                .map(|s| (false, s.parse::<u32>().unwrap())));
            }
            boards.push(board);
        }

        Bingo {
            draws,
            boards,
        }
    }
    
    /// Draws a number, marks it off on all boards, and returns it. If there are no more numbers to
    /// be drawn, returns `None`.
    fn draw_number(&mut self) -> Option<u32> {
        if let Some(number) = self.draws.pop_front() {
            for board in self.boards.iter_mut() {
                for (marked, value) in board.iter_mut() {
                    if number == *value {
                        *marked = true;
                    }
                }
            }

            Some(number)
        } else {
            None
        }
    }

    /// Returns the index of the first board that's finished, or `None` if there are none yet.
    fn find_finished_board(&self) -> Option<usize> {
        for i in 0..self.boards.len() {
            let marked = |&n: &usize| self.boards[i][n].0;

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