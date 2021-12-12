/// Reorders the elements in `slice` such that all elements for which the result of calling `f`
/// matches `item` are in the beginning of the slice, followed by all elements for which it doesn't
/// match. No other guarantees about the order of the elements is made. Returns the amount of
/// matching elements.
pub fn separate_by<T, I, F>(slice: &mut [T], item: I, mut f: F) -> usize
where
    F: FnMut(&T) -> I,
    I: Eq,
{
    // At every step in the loop, we know that the interval [found, i) contains non-matches.
    // By maintaining this invariant for all `i`, we reach the desired separation.
    let mut found = 0;
    for i in 0..slice.len() {
        if f(&slice[i]) == item {
            slice.swap(found, i);
            found += 1;
        }
    }
    found
}
