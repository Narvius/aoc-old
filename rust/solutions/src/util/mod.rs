use anyhow::{anyhow, Result};

/// Returns the sum of a slice of numbers.
pub fn sum_of(input: &[u32]) -> u32 { input.iter().sum() }

/// Returns the smallest of a slice of numbers.
pub fn min_of(input: &[u32]) -> Result<u32> { input.iter().copied().min().ok_or(anyhow!("min_of: empty slice")) }

/// Returns the largest of a slice of numbers.
pub fn max_of(input: &[u32]) -> Result<u32> { input.iter().copied().max().ok_or(anyhow!("max_of: empty slice")) }