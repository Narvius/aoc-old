//! A proc-macro responsible for automatically including solution files, mapping solution files to
//! data files, as well as making solutions indexable via [`keys::Key`]s.
//! 
//! It assumes that the solutions crate depends on `anyhow`, as well as the following file layout:
//! 
//! lib.rs
//! aoc2015/
//!   day01.rs
//!   day02.rs
//!   ..dayXX.rs
//! aoc2016/
//!   ...
//! aocXXXX/
//!   ...
//! 
//! Then, put `autokey::events!("src");` in lib.rs, and the aforementioned `mod`s and helper
//! functions will be generated in it.
//! 
//! It is assumed solution files contain two functions with these names and signatures:
//! 
//! ```
//! pub fn part1(_input: &[&str]) -> anyhow::Result<String> {
//!     Err(anyhow::anyhow!("unimplemented"))
//! }
//!
//! pub fn part2(_input: &[&str]) -> anyhow::Result<String> {
//!     Err(anyhow::anyhow!("unimplemented"))
//! }
//! ```
//! 
//! These correspond to the two parts of an Advent of Code task.
//! 
//! Majority of the code is stolen straight from https://github.com/dtolnay/automod, and from there
//! kludged into place by trial-and-error.

mod error;

use crate::error::{Error, Result};
use proc_macro::{Span, TokenStream};
use std::ffi::OsStr;
use std::path::{Path, PathBuf};
use syn::parse::{Parse, ParseStream};
use syn::{parse_macro_input, LitStr};

/// Arguments for [`events`] and [`days`].
struct Arg {
    path: LitStr,
}

impl Parse for Arg {
    fn parse(input: ParseStream) -> syn::Result<Self> {
        Ok(Arg {
            path: input.parse()?,
        })
    }
}

/// Automatically includes all AoC solution files with the appropriate 'mod' directives, and
/// generates a 'get_solutions' 
#[proc_macro]
pub fn events(input: TokenStream) -> TokenStream {
    let input = parse_macro_input!(input as Arg);
    let rel_path = input.path.value();

    let dir = match std::env::var_os("CARGO_MANIFEST_DIR") {
        Some(manifest_dir) => PathBuf::from(manifest_dir).join(rel_path),
        None => PathBuf::from(rel_path),
    };

    let rel_path = input.path.value();

    let expanded = match event_folder_names(dir) {
        Ok(names) => names.clone().into_iter().map(|name| event_item(&name, &rel_path)).chain(std::iter::once(event_indexer(names))).collect::<TokenStream>(),
        Err(err) => syn::Error::new(input.path.span(), err).into_compile_error().into(),
    };

    TokenStream::from(expanded)
}

/// Automatically includes all dayXX.rs files in the corresponding folder. Used from within [`events`].
/// TODO: Make it not pub and not a proc_macro, but an internal expansion function.
#[proc_macro]
pub fn days(input: TokenStream) -> TokenStream {
    let input = parse_macro_input!(input as Arg);
    let rel_path = input.path.value();

    let dir = match std::env::var_os("CARGO_MANIFEST_DIR") {
        Some(manifest_dir) => PathBuf::from(manifest_dir).join(input.path.value()),
        None => PathBuf::from(input.path.value()),
    };

    let folder = rel_path.split('/').last().unwrap();

    let expanded = match source_file_names(dir) {
        Ok(names) => names.clone().into_iter().map(|name| day_item(&name)).chain(std::iter::once(day_indexer(names, folder))).collect::<TokenStream>(),
        Err(err) => syn::Error::new(input.path.span(), err).into_compile_error().into(),
    };

    TokenStream::from(expanded)
}

/// Generates a 'mod' entry for events with an existing 'aocXXXX' folder. The mod contains the
/// results of an expansion of [`days`].
fn event_item(name: &str, path: &str) -> TokenStream {
    let ident = syn::Ident::new(name.split('.').next().unwrap(), Span::call_site().into());
    let path = format!("{}/aoc{}", path, &name[3..]);

    (quote::quote! {
        mod #ident { autokey::days!(#path); }
    }).into()
}

/// Generates:
/// ```
/// pub fn get_solution(key: keys::Key) -> Option<(fn(&[&str]) -> anyhow::Result<String>, &'static str)> {
///     match key.event {
///         keys::Event::AoC2015 => aoc2015::get_solution(key.day, key),
///         ...,
///         _ => None,
///     }
/// }
/// ```
/// for all events that have an "aocXXXX" folder.
fn event_indexer(names: Vec<String>) -> TokenStream {
    let items: Vec<_> = names.into_iter().map(|name| {
        let pat = quote::format_ident!("AoC{}", &name[3..]);
        let fn_ident = quote::format_ident!("aoc{}", &name[3..]);
        let call = quote::quote! { #fn_ident::get_solution(key.day, key.part) };

        quote::quote! { keys::Event::#pat => #call }
    }).collect();

    (quote::quote! {
        pub fn get_solution(key: keys::Key) -> Option<(fn(&[&str]) -> anyhow::Result<String>, &'static str)> {
            match key.event {
                #(#items),*,
                _ => None,
            }
        }
    }).into()
}

/// Gets the names of all folders starting with "aoc" in the given folder.
fn event_folder_names<P: AsRef<Path>>(dir: P) -> Result<Vec<String>> {
    let mut names = Vec::new();
    let mut failures = Vec::new();

    for entry in std::fs::read_dir(dir)? {
        let entry = entry?;
        if !entry.file_type()?.is_dir() {
            continue;
        }

        match entry.file_name().into_string() {
            Ok(name) => if name.starts_with("aoc") { names.push(name) },
            Err(err) => failures.push(err),
        }
    }

    failures.sort();
    if let Some(failure) = failures.into_iter().next() {
        return Err(Error::Utf8(failure));
    }

    if names.is_empty() {
        return Err(Error::Empty);
    }

    names.sort();
    Ok(names)
}

/// Generates a 'pub mod' entry for an existing dayxx.rs file.
fn day_item(name: &str) -> TokenStream {
    let ident = syn::Ident::new(name.split('.').next().unwrap(), Span::call_site().into());
    (quote::quote! {
        pub mod #ident;
    }).into()
}

/// Generates:
/// ```
/// pub fn get_solution(day: keys::Day, part: keys::Part) -> Option<(fn(&[&str]) -> anyhow::Result<String>, &'static str)> {
///     match (day, part) {
///         (keys::Day::Day01, keys::Part::One) => Some(day01::part1, "path-to-data-file"),
///         ...,
///         _ => None
///     }
/// }
/// ```
/// for solution files that exist.
fn day_indexer(names: Vec<String>, folder: &str) -> TokenStream {
    let part_one_ident = quote::format_ident!("One");
    let part_two_ident = quote::format_ident!("Two");
    let working_dir = std::env::var_os("CARGO_MANIFEST_DIR").unwrap();

    let items: Vec<_> = names.into_iter().map(|name| {
        let data_file = Path::new(&working_dir).parent().unwrap()
            .join("data")
            .join(folder)
            .join(&format!("day{:02}.txt", &name[3..]));
        
        let data_file = format!("{}", data_file.display());

        let pat_ident = quote::format_ident!("Day{:02}", &name[3..]);
        let pat1 = quote::quote! { (keys::Day::#pat_ident, keys::Part::#part_one_ident) };
        let pat2 = quote::quote! { (keys::Day::#pat_ident, keys::Part::#part_two_ident) };
        let fn_ident = quote::format_ident!("day{:02}", &name[3..]);

        quote::quote! { #pat1 => Some((#fn_ident::part1, #data_file)), #pat2 => Some((#fn_ident::part2, #data_file)) }
    }).collect();

    (quote::quote! {
        pub fn get_solution(day: keys::Day, part: keys::Part) -> Option<(fn(&[&str]) -> anyhow::Result<String>, &'static str)> {
            match (day, part) {
                #(#items),*,
                _=> None,
            }
        }
    }).into()
}

/// Gets the names of all source files in a directory (excluding extension).
fn source_file_names<P: AsRef<Path>>(dir: P) -> Result<Vec<String>> {
    let mut names = Vec::new();
    let mut failures = Vec::new();

    for entry in std::fs::read_dir(dir)? {
        let entry = entry?;
        if !entry.file_type()?.is_file() {
            continue;
        }

        let file_name = entry.file_name();
        if file_name == "mod.rs" || file_name == "lib.rs" || file_name == "main.rs" {
            continue;
        }

        let path = Path::new(&file_name);
        if path.extension() == Some(OsStr::new("rs")) {
            match file_name.into_string() {
                Ok(mut utf8) => {
                    utf8.truncate(utf8.len() - ".rs".len());
                    names.push(utf8);
                }
                Err(non_utf8) => {
                    failures.push(non_utf8);
                }
            }
        }
    }

    failures.sort();
    if let Some(failure) = failures.into_iter().next() {
        return Err(Error::Utf8(failure));
    }

    if names.is_empty() {
        return Err(Error::Empty);
    }

    names.sort();
    Ok(names)
}
