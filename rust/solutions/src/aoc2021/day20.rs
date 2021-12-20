use std::collections::HashSet;

/// Count the number of lit pixels after 2 iterations of the enhancing algorithm.
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    let mut image = Image::from_input(&input[2..]).unwrap();
    image.enhance(input[0].as_bytes());
    image.enhance(input[0].as_bytes());
    Ok(image.data.len().to_string())
}

/// Count the number of lit pixels after 50 iterations of the enhancing algorithm.
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    let mut image = Image::from_input(&input[2..]).unwrap();
    for _ in 0..50 {
        image.enhance(input[0].as_bytes());
    }
    Ok(image.data.len().to_string())
}

type Point = (i32, i32);

/// An image as per the puzzle description.
struct Image {
    /// The actual image data.
    data: HashSet<Point>,
    
    /// A second space that is written to, so as to not invalidate future reads within the same
    /// generation.
    backbuffer: HashSet<Point>,

    /// The rectangle within which points are uncertain, as a pair of top left point and size.
    active_rect: ((i32, i32), (i32, i32)),

    /// Whether tiles outside the `active_rect` are lit up.
    outside: bool,
}

impl Image {
    /// Applies one iteration of the enhancing algorithm to the image.
    fn enhance(&mut self, algo: &[u8]) {
        self.backbuffer.clear();
        self.widen_active_rect();
        let ((x0, y0), (w, h)) = self.active_rect;
        for y in y0..y0 + h {
            for x in x0..x0 + w {
                self.add_to_backbuffer((x, y), algo);
            }
        }
        self.outside = !self.outside;
        std::mem::swap(&mut self.data, &mut self.backbuffer);
    }

    /// Expands the borders of the [`Image::active_rec`] by one, taking into account whether the
    /// tiles that are now in range need to be lit up or not.
    fn widen_active_rect(&mut self) {
        self.active_rect.0.0 -= 1;
        self.active_rect.0.1 -= 1;
        self.active_rect.1.0 += 2;
        self.active_rect.1.1 += 2;

        if self.outside {
            let ((x0, y0), (w, h)) = self.active_rect;
            for x in x0..x0 + w {
                self.data.insert((x, y0));
                self.data.insert((x, y0 + h - 1));
            }
            for y in y0..y0 + h {
                self.data.insert((x0, y));
                self.data.insert((x0 + w - 1, y));
            }
        }
    }

    /// Checks the enhancing algorithm conditions for the given coordinate, and if so, writes it
    /// to the backbuffer.
    fn add_to_backbuffer(&mut self, (x, y): (i32, i32), algo: &[u8]) {
        let ((x0, y0), (w, h)) = self.active_rect;
        let mut index = 0;
        for y in (y - 1)..=(y + 1) {
            for x in (x - 1)..=(x + 1) {
                index *= 2;
                let outside_active = self.outside &&
                    (x < x0 || x >= x0 + w || y < y0 || y >= y0 + h);
                if outside_active || self.data.contains(&(x, y)) {
                    index += 1;
                }
            }
        }
        if algo[index] == b'#' {
            self.backbuffer.insert((x, y));
        }
    }

    /// Parses an image from the relevant block in the puzzle input.
    fn from_input(image: &[&str]) -> Option<Image> {
        let mut data = HashSet::new();

        for y in 0..image.len() {
            for x in 0..image[0].len() {
                if image[y].as_bytes()[x] == b'#' {
                    data.insert((x as i32, y as i32));
                }
            }
        }

        Some(Image {
            data,
            backbuffer: HashSet::new(),
            active_rect: ((0, 0), (image[0].len() as i32, image.len() as i32)),
            outside: false,
        })
    }    
}
