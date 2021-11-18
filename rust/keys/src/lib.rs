//! Contains the [`Key`] struct that is used as an index by the `get_solution` function generated
//! by the `autokey` crate.

use enum_iterator::IntoEnumIterator;
use itertools::Itertools;
use num_enum::{IntoPrimitive, TryFromPrimitive};

/// Returns an iterator of all possible [`Key`]s.
pub fn all_keys() -> impl Iterator<Item = Key> {
    let events = Event::into_enum_iter();
    let days = Day::into_enum_iter();
    let parts = Part::into_enum_iter();

    events.cartesian_product(days).cartesian_product(parts)
        .map(|((event, day), part)| Key { event, day, part })
}

/// An index into the space of existing Advent of Code problems.
#[derive(Copy, Clone, Eq, Hash, PartialEq)]
pub struct Key {
    /// The event to index into.
    pub event: Event,
    /// The day of the event to index into.
    pub day: Day,
    /// The part of the day of the event to index into.
    pub part: Part,
}

/// A specific Advent of Code event.
#[derive(Copy, Clone, Debug, Eq, Hash, PartialEq, IntoEnumIterator, IntoPrimitive, TryFromPrimitive)]
#[repr(u16)]
pub enum Event {
    AoC2015 = 2015,
    AoC2016 = 2016,
    AoC2017 = 2017,
    AoC2018 = 2018,
    AoC2019 = 2019,
    AoC2020 = 2020,
    AoC2021 = 2021,
    AoC2022 = 2022,
}

/// A day of an Advent of Code event.
#[derive(Copy, Clone, Debug, Eq, Hash, PartialEq, IntoEnumIterator, IntoPrimitive, TryFromPrimitive)]
#[repr(u8)]
pub enum Day {
    Day01 = 1,
    Day02 = 2,
    Day03 = 3,
    Day04 = 4,
    Day05 = 5,
    Day06 = 6,
    Day07 = 7,
    Day08 = 8,
    Day09 = 9,
    Day10 = 10,
    Day11 = 11,
    Day12 = 12,
    Day13 = 13,
    Day14 = 14,
    Day15 = 15,
    Day16 = 16,
    Day17 = 17,
    Day18 = 18,
    Day19 = 19,
    Day20 = 20,
    Day21 = 21,
    Day22 = 22,
    Day23 = 23,
    Day24 = 24,
    Day25 = 25,
}

/// A part of an Advent of Code day.
#[derive(Copy, Clone, Debug, Eq, Hash, PartialEq, IntoEnumIterator, IntoPrimitive, TryFromPrimitive)]
#[repr(u8)]
pub enum Part {
    One = 1,
    Two = 2,
}