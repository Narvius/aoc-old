use serde_json::Value;

/// Sum all numbers in the file.
pub fn part1(input: &[&str]) -> anyhow::Result<String> {
    let num = regex::Regex::new(r"-?\d+").unwrap();

    let mut sum = 0i32;
    for m in num.find_iter(input[0]) {
        sum += m.as_str().parse::<i32>().unwrap();
    }
    
    Ok(sum.to_string())
}

/// Sum all numbers, excluding numbers on objects with a property with the value of "red".
pub fn part2(input: &[&str]) -> anyhow::Result<String> {
    let tree: Value = serde_json::from_str(input[0]).unwrap();
    Ok(sum_excluding_red_objects(&tree).to_string())
}

/// Sums all numbers in the json object, but ignoring objects that have a property with a value of
/// "red".
fn sum_excluding_red_objects(node: &Value) -> i64 {
    if node.is_number() {
        node.as_i64().unwrap()
    } else if node.is_array() {
        let array = node.as_array().unwrap();
        array.iter().map(sum_excluding_red_objects).sum()
    } else if node.is_object() {
        let object = node.as_object().unwrap();
        let is_red = object.values().any(|v| v.is_string() && v.as_str().unwrap() == "red");

        if is_red {
            0
        } else {
            object.values().map(sum_excluding_red_objects).sum()
        }
    } else {
        0
    }
}