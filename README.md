Word Counter

A command-line tool that counts words from text files in a directory, with:

Stopword exclusion from exclude.txt
Parallel counting for performance
Per-letter output files FILE_A.txt … FILE_Z.txt
A summary file for the total number of excluded tokens
Note: The current implementation reads files as raw text. Supporting JSON/CSV parsing can be added later without changing the core counting logic.

Prerequisites

.NET SDK 9 (or whatever your project targets)

Quick Start

Build & (optionally) run tests
WordCounter -- --input "C:\data\inputs" --out "C:\data\results" --exclude "C:\data\inputs\exclude.txt"
